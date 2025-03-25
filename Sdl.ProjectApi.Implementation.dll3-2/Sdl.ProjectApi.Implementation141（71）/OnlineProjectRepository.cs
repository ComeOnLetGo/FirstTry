using System;
using System.Collections.Generic;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public class OnlineProjectRepository : IProjectRepository, IProjectConfigurationRepository
	{
		private readonly Project _lazyLanguageCloudProject;

		private readonly IProjectPathUtil _projectPathUtil;

		private ISettingsBundlesList _lazySettingsBundlesList;

		private readonly ProjectFileFactory _projectFileFactory;

		public bool IsInitialized => true;

		public string SourceLanguage
		{
			get
			{
				return _lazyLanguageCloudProject.SourceLanguage;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Guid ProjectGuid => Guid.Parse(_lazyLanguageCloudProject.Id);

		public GeneralProjectInfo ProjectInfo { get; }

		public Guid ProjectTemplateGuid
		{
			get
			{
				return Guid.NewGuid();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Guid ReferenceProjectGuid
		{
			get
			{
				return Guid.NewGuid();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public IEnumerable<Guid> LanguageDirectionSettingsGuids
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string LanguageResourceFilePath
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Guid SettingsBundleGuid
		{
			get
			{
				return Guid.NewGuid();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public ISettingsBundlesList SettingsBundles
		{
			get
			{
				if (_lazySettingsBundlesList == null)
				{
					List<SettingsBundle> list = new List<SettingsBundle>();
					list.Add(new SettingsBundle
					{
						Guid = SettingsBundleGuid,
						Name = "SBGuid"
					});
					_lazySettingsBundlesList = (ISettingsBundlesList)(object)new SettingsBundlesList(list);
				}
				return _lazySettingsBundlesList;
			}
			set
			{
				_lazySettingsBundlesList = value;
			}
		}

		public OnlineProjectRepository(Project languageCloudProject, IProjectPathUtil projectPathUtil)
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			_lazyLanguageCloudProject = languageCloudProject;
			_projectPathUtil = projectPathUtil;
			_projectFileFactory = new ProjectFileFactory(_projectPathUtil);
			ProjectInfo = new GeneralProjectInfo
			{
				CreatedAt = ((languageCloudProject.CreatedAtDateTime != null) ? DateTime.Parse(languageCloudProject.CreatedAtDateTime) : DateTime.MinValue),
				CreatedBy = languageCloudProject.CreatedBy,
				DueDateSpecified = (languageCloudProject.DueDateTime != null),
				DueDate = ((languageCloudProject.DueDateTime != null) ? DateTime.Parse(languageCloudProject.DueDateTime) : DateTime.MaxValue),
				Status = MapProjectStatus(languageCloudProject.Status),
				Name = languageCloudProject.Name,
				Description = languageCloudProject.Description,
				IsCloudBased = true,
				LanguageCloudLocation = languageCloudProject.Location,
				Customer = new Sdl.ProjectApi.Implementation.Xml.Customer
				{
					Guid = (Guid.TryParse(languageCloudProject.Customer.Id, out var _) ? Guid.Parse(languageCloudProject.Customer.Id) : Guid.NewGuid()),
					Name = languageCloudProject.Customer.Name,
					Email = languageCloudProject.Customer.Email
				}
			};
		}

		public AutomaticTask AddAutomaticTasks(IProject project, Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask)
		{
			throw new NotImplementedException();
		}

		public AutomaticTask AddAutomaticTasks(IProject project, Guid newGuid, Guid previousTaskGuid, ITaskTemplate[] subTaskTemplates, IUser currentUser, string complexTaskTemplateId)
		{
			throw new NotImplementedException();
		}

		public AutomaticTask AddAutomaticTasks(IProject project, IUser currentUser, string taskTemplateId)
		{
			throw new NotImplementedException();
		}

		public AutomaticTask AddAutomaticTasks(IProject project, IAutomaticTask task)
		{
			throw new NotImplementedException();
		}

		public ILanguageDirection AddLanguageDirection(IProjectConfiguration projectConfig, Language sourceLanguage, Language targetLanguage)
		{
			throw new NotImplementedException();
		}

		public ILocalizableFile AddLocalizableFile(IProject project, ILocalizableFile localizableFile, ILocalizableFile referenceFile)
		{
			throw new NotImplementedException();
		}

		public void AddManualTaskTemplate(IManualTaskTemplate template)
		{
			throw new NotImplementedException();
		}

		public IProjectFile AddProjectFile(IProject project, string fileToAdd, string folderInProject, Language sourceLanguage, FileRole fileRole = 0)
		{
			throw new NotImplementedException();
		}

		public void AddProjectPackageCreationOperation(IProjectPackageCreation c)
		{
			throw new NotImplementedException();
		}

		public void AddProjectPackageImportOperation(IProjectPackageImport i)
		{
			throw new NotImplementedException();
		}

		public IReferenceFile AddReferenceFile(IProject project, IReferenceFile referenceFile, IReferenceFile parentReferenceFile)
		{
			throw new NotImplementedException();
		}

		public void AddReturnPackageCreationOperation(IReturnPackageCreation c)
		{
			throw new NotImplementedException();
		}

		public void AddReturnPackageImportOperation(IReturnPackageImport i)
		{
			throw new NotImplementedException();
		}

		public ITranslatableFile AddTranslatableFile(IProject project, ITranslatableFile translatableFile, ITranslatableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			throw new NotImplementedException();
		}

		public ITranslatableFile AddTranslatableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, string filterDefinitionId, ITranslatableFile referenceFile)
		{
			throw new NotImplementedException();
		}

		public void AddUsers(IUser[] users)
		{
			throw new NotImplementedException();
		}

		public void AddUser(IUser user)
		{
			throw new NotImplementedException();
		}

		public IManualTaskTemplate CreateManualTaskTemplate(string id)
		{
			throw new NotImplementedException();
		}

		public IMergedTranslatableFile CreateMergedTranslatableFile(IProject project, string mergedFileName, string folderInProject, Language language, string fileTypeDefinitionId, ITranslatableFile[] childFiles)
		{
			throw new NotImplementedException();
		}

		public IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, DateTime dueDate, IUser createdBy, DateTime createdAt)
		{
			throw new NotImplementedException();
		}

		public IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, string externalId, DateTime startedAt, DateTime dueDate, IUser createdBy, DateTime createdAt, DateTime completedAt, int percentComplete)
		{
			throw new NotImplementedException();
		}

		public void DiscardData()
		{
			throw new NotImplementedException();
		}

		public IAnalysisBand[] GetAnalysisBands()
		{
			throw new NotImplementedException();
		}

		public ProjectCascadeItem GetCascadeItem(IRelativePathManager pathManager)
		{
			return null;
		}

		public IComplexTaskTemplate GetInitialComplexTaskTemplate(IWorkflow workflow)
		{
			throw new NotImplementedException();
		}

		public List<ILanguageDirection> GetLanguageDirections(IProjectConfiguration projectConfig)
		{
			List<ILanguageDirection> list = new List<ILanguageDirection>();
			foreach (string targetLanguage in _lazyLanguageCloudProject.TargetLanguages)
			{
				list.Add((ILanguageDirection)(object)new LanguageDirection(projectConfig, new Sdl.ProjectApi.Implementation.Xml.LanguageDirection
				{
					SourceLanguageCode = _lazyLanguageCloudProject.SourceLanguage,
					TargetLanguageCode = targetLanguage
				}));
			}
			return list;
		}

		public Guid GetLanguageDirectionSettingsBundleGuid(ILanguageDirection languageDirection)
		{
			throw new NotImplementedException();
		}

		public IMergedTranslatableFile GetMergedTranslatableFile(IProject project, Guid projectFileGuid)
		{
			return null;
		}

		public List<IMergedTranslatableFile> GetMergedTranslatableFileHistory(IProject project, Guid projectFileGuid)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, string> GetPhysicalFiles(IProject project)
		{
			throw new NotImplementedException();
		}

		public IProjectFile GetProjectFile(IProject project, string fileName, string folderName, Language language)
		{
			throw new NotImplementedException();
		}

		public IProjectFile GetProjectFile(IProject project, Guid projectFileGuid)
		{
			throw new NotImplementedException();
		}

		public List<IProjectFile> GetProjectFiles(IProject project, bool allFiles = false)
		{
			throw new NotImplementedException();
		}

		public List<IProjectFile> GetProjectFiles(IProject project, Language language)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			if (_lazyLanguageCloudProject.SourceLanguage.Equals(((LanguageBase)language).IsoAbbreviation, StringComparison.InvariantCultureIgnoreCase))
			{
				for (int i = 0; i < _lazyLanguageCloudProject.FileAnalytics.TranslatableFileCount; i++)
				{
					list.Add((IProjectFile)(object)CreateProjectFile(project, new ProjectFile
					{
						Role = FileRole.Translatable
					}, new Sdl.ProjectApi.Implementation.Xml.LanguageFile
					{
						LanguageCode = ((LanguageBase)language).IsoAbbreviation
					}));
				}
				for (int j = 0; j < _lazyLanguageCloudProject.FileAnalytics.ReferenceFileCount; j++)
				{
					list.Add((IProjectFile)(object)CreateProjectFile(project, new ProjectFile
					{
						Role = FileRole.Reference
					}, new Sdl.ProjectApi.Implementation.Xml.LanguageFile
					{
						LanguageCode = ((LanguageBase)language).IsoAbbreviation
					}));
				}
			}
			return list;
		}

		public ILanguageFile GetSourceLanguageFile(IProject project, ILanguageFile targetLanguageFile)
		{
			return null;
		}

		public List<IScheduledTask> GetTasks(IProject project)
		{
			return new List<IScheduledTask>();
		}

		public IProjectTermbaseConfiguration GetTermbaseConfiguration(IRelativePathManager pathManager)
		{
			throw new NotImplementedException();
		}

		public ITranslatableFile GetTranslatableFile(IProject project, Guid languageFileGuid)
		{
			throw new NotImplementedException();
		}

		public ITranslatableFile GetTranslatableFile(IProject project, string fileName, string folderName, Language language)
		{
			throw new NotImplementedException();
		}

		public List<ITranslatableFile> GetTranslatableTargetOrSingleDocumentFiles(IProject project)
		{
			return new List<ITranslatableFile>();
		}

		public IUser GetUserById(IProjectsProvider projectsProvider, string userId)
		{
			return null;
		}

		public List<IUser> GetUsers()
		{
			throw new NotImplementedException();
		}

		public void Load(string projectFilePath)
		{
			MarkAsInitialized();
		}

		public List<IProjectPackageCreation> LoadProjectPackageCreations(IProject project, IProjectPackageInitializer packageInitializer)
		{
			return new List<IProjectPackageCreation>();
		}

		public List<IProjectPackageImport> LoadProjectPackageImports(IProject project)
		{
			return new List<IProjectPackageImport>();
		}

		public List<IReturnPackageCreation> LoadReturnPackageCreations(IProject project, IProjectPackageInitializer packageInitializer)
		{
			return new List<IReturnPackageCreation>();
		}

		public List<IReturnPackageImport> LoadReturnPackageImports(IProject project, IProjectPackageInitializer packageInitializer)
		{
			return new List<IReturnPackageImport>();
		}

		public void MarkProjectAsStarted()
		{
			throw new NotImplementedException();
		}

		public void RemoveLanguageDirection(ILanguageDirection languageDirection)
		{
			throw new NotImplementedException();
		}

		public void RemoveLanguageFile(ILanguageFile languageFile)
		{
			throw new NotImplementedException();
		}

		public void RemoveTask(IScheduledTask task)
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void ResetAnalysisStatics(IProject project)
		{
			throw new NotImplementedException();
		}

		public void ResetWordCountStatistics()
		{
			throw new NotImplementedException();
		}

		public void Save(string projectFilePath)
		{
		}

		public void SetAnalysisBands(int[] minimumMatchValues)
		{
			throw new NotImplementedException();
		}

		public void SetCascadeItem(IRelativePathManager pathManager, ProjectCascadeItem cascadeItem)
		{
			throw new NotImplementedException();
		}

		public void SetInitialComplexTaskTemplate(IComplexTaskTemplate complexTaskTemplate)
		{
			throw new NotImplementedException();
		}

		public void SetTermbaseConfiguration(IProjectTermbaseConfiguration termbaseConfiguration, IRelativePathManager pathManager)
		{
			throw new NotImplementedException();
		}

		public bool SplitFileIntoTargetLanguage(IProject project, ILanguageFile languageFile, Language targetLanguage, out IProjectFile targetLanguageFile)
		{
			throw new NotImplementedException();
		}

		public void UpdateSourceLanguageForFiles(string oldLanguageCode, string replacementLanguage)
		{
			throw new NotImplementedException();
		}

		public void ValidateLanguageResources(IProjectConfiguration projectConfiguration, IServerEvents serverEvents)
		{
		}

		protected void MarkAsInitialized()
		{
		}

		private ProjectStatus MapProjectStatus(ProjectStatus status)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected I4, but got Unknown
			return (int)status switch
			{
				0 => ProjectStatus.Started, 
				2 => ProjectStatus.Completed, 
				1 => ProjectStatus.Archived, 
				_ => ProjectStatus.Started, 
			};
		}

		private ILanguageFile CreateProjectFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			ILanguageFile val = _projectFileFactory.CreateLanguageFile(project, xmlProjectFile, xmlLanguageFile);
			if (val == null)
			{
				throw new Exception("Unexpected file role: " + xmlProjectFile.Role);
			}
			return val;
		}

		public void AddNativeFileVersionForSourceFile(IProject project, ITranslatableFile file, string nativeFilePath)
		{
			throw new NotImplementedException();
		}

		public List<IProjectFile> GetPagedProjectFiles(IProject project, Language language, int pageSize, int skip)
		{
			throw new NotImplementedException();
		}

		public ILocalizableFile AddLocalizableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, ILocalizableFile referenceFile)
		{
			throw new NotImplementedException();
		}
	}
}
