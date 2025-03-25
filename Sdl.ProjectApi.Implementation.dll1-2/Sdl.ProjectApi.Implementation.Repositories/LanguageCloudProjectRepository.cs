using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Services;
using Sdl.Platform.Interfaces.Telemetry;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class LanguageCloudProjectRepository : ProjectRepository
	{
		private readonly ManualTaskBuilder _manualTaskBuilder;

		private readonly SourceFileBuilder _sourceFileBuilder;

		private readonly AutomaticTaskBuilder _automaticTaskBuilder;

		private readonly TargetFileBuilder _targetFileBuilder;

		private readonly ITelemetryService _telemetryService;

		internal List<ProjectFile> ProjectFiles => XmlProject.ProjectFiles;

		internal List<Task> ProjectTasks => XmlProject.Tasks.Items;

		internal List<Sdl.ProjectApi.Implementation.Xml.User> Users => XmlProject.Users;

		public LanguageCloudProjectRepository(IApplication application, IProjectPathUtil pathUtil)
			: base(application, pathUtil)
		{
			_manualTaskBuilder = new ManualTaskBuilder();
			_sourceFileBuilder = new SourceFileBuilder();
			_automaticTaskBuilder = new AutomaticTaskBuilder();
			_targetFileBuilder = new TargetFileBuilder();
		}

		public LanguageCloudProjectRepository(IApplication application, IProjectPathUtil pathUtil, DetailedProject languageCloudProject, IUser projectCreatedBy, ITelemetryService telemetryService)
			: base(application, pathUtil)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			_manualTaskBuilder = new ManualTaskBuilder();
			_sourceFileBuilder = new SourceFileBuilder();
			_automaticTaskBuilder = new AutomaticTaskBuilder();
			_targetFileBuilder = new TargetFileBuilder();
			Initialize(((Project)languageCloudProject).Name, Guid.Parse(((Project)languageCloudProject).Id), projectCreatedBy, DateTime.Parse(((Project)languageCloudProject).CreatedAtDateTime), inPlace: false, new Language(((Project)languageCloudProject).SourceLanguage));
			SetGeneralProjectInfo(languageCloudProject);
			AddOrUpdateProjectContent(languageCloudProject);
			_telemetryService = telemetryService;
		}

		public void Refresh(DetailedProject languageCloudProject)
		{
			SetGeneralProjectInfo(languageCloudProject);
			AddOrUpdateProjectContent(languageCloudProject);
		}

		public void SetGeneralProjectInfo(DetailedProject detailedProject)
		{
			XmlProject.GeneralProjectInfo = new GeneralProjectInfo
			{
				Name = ((Project)detailedProject).Name,
				Description = ((Project)detailedProject).Description,
				Status = detailedProject.MapProjectStatus(),
				CreatedAt = DateTime.Parse(((Project)detailedProject).CreatedAtDateTime).ToUniversalTime(),
				CreatedBy = ((Project)detailedProject).CreatedBy,
				DueDateSpecified = true,
				DueDate = DateTime.Parse(((Project)detailedProject).DueDateTime).ToUniversalTime(),
				LanguageCloudLocation = ((Project)detailedProject).Location,
				Customer = MapCustomer(((Project)detailedProject).Customer),
				IsImported = XmlProject.GeneralProjectInfo.IsImported
			};
		}

		public void AddFolderInfoForLCProject(DetailedProject languageCloudProject)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Invalid comparison between Unknown and I4
			int num = 0;
			foreach (LightFile targetFile in languageCloudProject.TargetFiles)
			{
				if (targetFile.Path != null && targetFile.Path.Any())
				{
					AddFolderInfoForLCFile(targetFile);
					num++;
				}
			}
			foreach (LightFile sourceFile in languageCloudProject.SourceFiles)
			{
				if ((int)sourceFile.Role == 1 && sourceFile.Path != null && sourceFile.Path.Any())
				{
					AddFolderInfoForLCFile(sourceFile);
					num++;
				}
			}
			if (num > 0)
			{
				RecordProjectsWithFolderStructureTelemetryData(num);
			}
		}

		public override IAnalysisBand[] GetAnalysisBands()
		{
			IAnalysisBand[] array = (IAnalysisBand[])(object)new IAnalysisBand[XmlConfiguration.AnalysisBands.Count];
			for (int i = 0; i < array.Length; i++)
			{
				int minimumMatchValue = XmlConfiguration.AnalysisBands[i].MinimumMatchValue;
				int maximumMatchValue = ((i < array.Length - 1) ? (XmlConfiguration.AnalysisBands[i + 1].MinimumMatchValue - 1) : 99);
				array[i] = (IAnalysisBand)(object)new AnalysisBand(minimumMatchValue, maximumMatchValue);
			}
			return array;
		}

		private void AddFolderInfoForLCFile(LightFile file)
		{
			ProjectFile projectFile = DetermineProjectFile(file);
			if (projectFile == null || !string.IsNullOrEmpty(projectFile.Path))
			{
				return;
			}
			projectFile.Path = string.Join("\\", file.Path) + "\\";
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
			{
				foreach (FileVersion fileVersion in languageFile.FileVersions)
				{
					fileVersion.PhysicalPath = Path.GetDirectoryName(fileVersion.PhysicalPath) + "\\" + projectFile.Path + Path.GetFileName(fileVersion.PhysicalPath);
				}
			}
		}

		private void AddLCUsers(DetailedProject languageCloudProject)
		{
			if (languageCloudProject.AccountUsers == null || !languageCloudProject.AccountUsers.Any())
			{
				return;
			}
			foreach (AccountUser accountUser in languageCloudProject.AccountUsers)
			{
				User user = new User(accountUser.Id);
				user.FullName = accountUser.Name;
				AddUser((IUser)(object)user);
			}
		}

		private void AddOrUpdateProjectContent(DetailedProject languageCloudProject)
		{
			AddLCUsers(languageCloudProject);
			foreach (LightFile sourceFile in languageCloudProject.SourceFiles)
			{
				AddOrUpdateSourceFile(sourceFile, ((Project)languageCloudProject).TargetLanguages);
			}
			AddOrUpdateProjectFiles(languageCloudProject);
			foreach (Task task in languageCloudProject.Tasks)
			{
				if (task.IsAutomatic)
				{
					AddOrUpdateAutomaticTask(task);
				}
				else
				{
					AddOrUpdateManualTasks(task, task.DueDateTime ?? ((Project)languageCloudProject).DueDateTime);
				}
			}
		}

		private void AddReferenceLanguageFilesToCache(ProjectFile referenceProjectFile)
		{
			referenceProjectFile.LanguageFiles.ForEach(delegate(Sdl.ProjectApi.Implementation.Xml.LanguageFile lf)
			{
				_languageFileCache.Add(referenceProjectFile, lf);
			});
		}

		private void AddOrUpdateSourceFile(LightFile sourceFile, ICollection<string> targetLanguages)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Invalid comparison between Unknown and I4
			if ((int)sourceFile.Role == 1)
			{
				ProjectFile projectFile = XmlProject.ProjectFiles.Where((ProjectFile pf) => pf.Guid == Guid.Parse(sourceFile.Id)).FirstOrDefault();
				if (projectFile == null)
				{
					ProjectFile projectFile2 = _sourceFileBuilder.CreateReferenceProjectFile(sourceFile, targetLanguages);
					AddReferenceLanguageFilesToCache(projectFile2);
					XmlProject.ProjectFiles.Add(projectFile2);
				}
				else
				{
					_sourceFileBuilder.UpdateReferenceProjectFile(sourceFile, projectFile, targetLanguages);
				}
			}
			else
			{
				ProjectFile projectFile3 = XmlProject.ProjectFiles.Where((ProjectFile pf) => pf.Guid.ToString().Equals(sourceFile.Id)).FirstOrDefault();
				if (projectFile3 == null)
				{
					projectFile3 = _sourceFileBuilder.CreateTranslatableSourceProjectFile(sourceFile);
					XmlProject.ProjectFiles.Add(projectFile3);
					_languageFileCache.Add(projectFile3);
				}
				else
				{
					_sourceFileBuilder.UpdateSourceTranslatableProjectFile(sourceFile, projectFile3);
				}
			}
		}

		private void AddOrUpdateManualTasks(Task task, string dueDate)
		{
			if (!(XmlProject.Tasks.Items.Where((Task t) => t.Guid == Guid.Parse(task.Id)).FirstOrDefault() is Sdl.ProjectApi.Implementation.Xml.ManualTask manualTask))
			{
				XmlProject.Tasks.Items.Add(_manualTaskBuilder.CreateManualTask(task, dueDate));
			}
			else
			{
				_manualTaskBuilder.UpdateManualTask(manualTask, task, dueDate);
			}
		}

		private void AddOrUpdateProjectFiles(DetailedProject languageCloudProject)
		{
			foreach (LightFile targetFile in languageCloudProject.TargetFiles)
			{
				AddOrUpdateTranslatableFileForLC(targetFile);
			}
		}

		private void AddOrUpdateTranslatableFileForLC(LightFile file)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			Language language = new Language(file.LanguageCode);
			ProjectFile projectFile = DetermineProjectFile(file);
			if (projectFile != null)
			{
				Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = projectFile.LanguageFiles.FirstOrDefault((Sdl.ProjectApi.Implementation.Xml.LanguageFile f) => f.LanguageCode.Equals(((LanguageBase)language).IsoAbbreviation, StringComparison.InvariantCultureIgnoreCase));
				if (languageFile != null)
				{
					_targetFileBuilder.UpdateTargetFile(file, languageFile);
				}
				else
				{
					projectFile.LanguageFiles.Add(_targetFileBuilder.CreateTargetLanguageFile(file, language));
				}
			}
		}

		private ProjectFile DetermineProjectFile(LightFile file)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			string sourceFileId = null;
			if ((int)file.Role == 1)
			{
				sourceFileId = file.Id;
			}
			else
			{
				object obj;
				if (!string.IsNullOrEmpty(file.SourceFileId))
				{
					obj = file.SourceFileId;
				}
				else
				{
					CurrentTaskFile obj2 = file.CurrentTask.Files.FirstOrDefault();
					obj = ((obj2 != null) ? obj2.SourceFileId : null);
				}
				sourceFileId = (string)obj;
			}
			if (!string.IsNullOrEmpty(sourceFileId))
			{
				return XmlProject.ProjectFiles?.FirstOrDefault((ProjectFile pf) => pf.Guid == Guid.Parse(sourceFileId));
			}
			return null;
		}

		private void AddOrUpdateAutomaticTask(Task task)
		{
			if (!(XmlProject.Tasks.Items.Where((Task t) => t.Guid == Guid.Parse(task.Id)).FirstOrDefault() is Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask))
			{
				Sdl.ProjectApi.Implementation.Xml.AutomaticTask item = _automaticTaskBuilder.CreateAutomaticTask(task);
				XmlProject.Tasks.Items.Add(item);
			}
			else
			{
				_automaticTaskBuilder.UpdateAutomaticTask(automaticTask, task);
			}
		}

		private Sdl.ProjectApi.Implementation.Xml.Customer MapCustomer(Customer customer)
		{
			if (customer == null)
			{
				return null;
			}
			Guid result;
			return new Sdl.ProjectApi.Implementation.Xml.Customer
			{
				Guid = (Guid.TryParse(customer.Id, out result) ? result : Guid.NewGuid()),
				Name = customer.Name,
				Email = customer.Email
			};
		}

		private void RecordProjectsWithFolderStructureTelemetryData(int filesCount)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			TrackingEvent val = new TrackingEvent("Download Cloud Projects With Folder Structure");
			val.Metrics.Add("Files Count", filesCount);
			_telemetryService.TrackEvent((ITrackingEvent)(object)val);
		}
	}
}
