using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SDL.ApiClientSDK.GS.Models;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.StudioServer.ProjectServer.Package;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class PublishProjectPackageCreation : PackageCreation
	{
		private readonly IWorkflow _workflow;

		public PublishProjectPackageCreation(IProject project, IProjectPathUtil projectPathUtil)
			: base(project, new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlPackageCreation(project), projectPathUtil)
		{
			_workflow = ((IProjectConfiguration)project).ProjectsProvider.Workflow;
		}

		protected override void StartImpl()
		{
			UserDetails serverUser = ServerUserManager.GetServerUser(base.Project.PublishProjectOperation.UnqualifiedServerUri, base.Project.ServerUserName);
			Create(serverUser);
		}

		public string Create(UserDetails serverUser)
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Invalid comparison between Unknown and I4
			string text = base.ProjectPathUtil.MakeAbsolutePath(base.Project, base.XmlPackageCreation.Path, false);
			try
			{
				SetCurrentOperationDescription(StringResources.ProjectPackageCreation_InitPackage, 0);
				UpdateProject(serverUser);
				if (!TrySaveProject())
				{
					return null;
				}
				if (!TryCreatePackage(text))
				{
					return null;
				}
				if (ShouldCancel())
				{
					return null;
				}
				SetPercentComplete(100);
				SetStatus((PackageStatus)5);
			}
			finally
			{
				DeletePackageIfCompleted(text);
			}
			if ((int)base.Status != 5)
			{
				return null;
			}
			return text;
		}

		protected virtual bool TryCreatePackage(string absolutePackagePath)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePackagePath));
			using (FileStream fileStream = File.Create(absolutePackagePath))
			{
				ProjectPackage val = new ProjectPackage((Stream)fileStream, (PackageAccessMode)0);
				try
				{
					if (!CreateManifest(val))
					{
						return false;
					}
					if (!CreateMappingDictionary(out var projectFiles))
					{
						return false;
					}
					if (!AddAllFilesToPackage(projectFiles, val))
					{
						return false;
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			return true;
		}

		private void UpdateProject(UserDetails serverUser)
		{
			ApplyServerTimestamps();
			ApplyServerUser(serverUser);
			UpdateConfirmationStatistics();
		}

		private void DeletePackageIfCompleted(string absolutePackagePath)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)base.Status != 5 && File.Exists(absolutePackagePath))
			{
				try
				{
					File.Delete(absolutePackagePath);
				}
				catch
				{
				}
			}
		}

		protected virtual bool TrySaveProject()
		{
			((IProjectConfiguration)base.Project).Save();
			SetPercentComplete(20);
			if (ShouldCancel())
			{
				return false;
			}
			return true;
		}

		private bool CreateMappingDictionary(out Dictionary<string, string> projectFiles)
		{
			projectFiles = base.ProjectImpl.ProjectRepository.GetPhysicalFiles(base.Project);
			SetPercentComplete(40);
			if (ShouldCancel())
			{
				return false;
			}
			return true;
		}

		private bool CreateManifest(ProjectPackage projectPackage)
		{
			projectPackage.ProjectName = base.Project.Name;
			projectPackage.ManifestXml = CreatePackageManifest();
			SetPercentComplete(30);
			if (ShouldCancel())
			{
				return false;
			}
			return true;
		}

		private bool AddAllFilesToPackage(Dictionary<string, string> projectFiles, ProjectPackage projectPackage)
		{
			SetCurrentOperationDescription(StringResources.ProjectPackageCreation_AddingFiles);
			int num = 0;
			foreach (KeyValuePair<string, string> projectFile in projectFiles)
			{
				num++;
				using (Stream stream = File.OpenRead(projectFile.Key))
				{
					projectPackage.AddFile(projectFile.Value, stream);
				}
				SetPercentComplete(40 + 60 * num / projectFiles.Count);
				if (ShouldCancel())
				{
					return false;
				}
			}
			return true;
		}

		private void ApplyServerTimestamps()
		{
			IProjectFile[] allProjectFiles = base.Project.GetAllProjectFiles();
			IProjectFile[] array = allProjectFiles;
			for (int i = 0; i < array.Length; i++)
			{
				LanguageFile languageFile = (LanguageFile)(object)array[i];
				languageFile.ApplyServerTimestamps(delegate(string message)
				{
					ReportMessage(StringResources.PublishProjectPackageCreation_Name, message, (MessageLevel)2);
				});
			}
		}

		private void ApplyServerUser(UserDetails serverUser)
		{
			IUser val = serverUser.ToProjectApiUser();
			base.ProjectImpl.CreatedBy = val;
			IProjectFile[] allProjectFiles = base.Project.GetAllProjectFiles();
			IProjectFile[] array = allProjectFiles;
			for (int i = 0; i < array.Length; i++)
			{
				LanguageFile languageFile = (LanguageFile)(object)array[i];
				IFileRevision[] revisions = languageFile.Revisions;
				foreach (IFileRevision val2 in revisions)
				{
					val2.CreatedBy = val;
				}
			}
			if (base.Project.AutomaticTasks == null)
			{
				return;
			}
			IAutomaticTask[] automaticTasks = base.Project.AutomaticTasks;
			for (int k = 0; k < automaticTasks.Length; k++)
			{
				ScheduledTask scheduledTask = (ScheduledTask)(object)automaticTasks[k];
				scheduledTask.CreatedBy = val;
				if (scheduledTask is ManualTask manualTask)
				{
					manualTask.AssignedBy = val;
				}
			}
		}

		private void UpdateConfirmationStatistics()
		{
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			ITranslatableFile[] array = (from pf in base.Project.GetAllProjectFiles()
				select (ITranslatableFile)(object)((pf is ITranslatableFile) ? pf : null) into tf
				where tf != null && !((ILanguageFile)tf).IsSource && (int)tf.ConfirmationStatistics.Status != 3
				select tf).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			IProject project = base.Project;
			ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById("Sdl.ProjectApi.AutomaticTasks.TranslationCount");
			IProjectFile[] array2 = (IProjectFile[])(object)array;
			IAutomaticCollaborativeTask val = project.CreateNewAutomaticTask(taskTemplateById, array2);
			val.Start();
			if (((ITaskBase)val).Result.HasErrors || ((ITaskBase)val).Result.HasWarnings || ((ITaskBase)val).Result.HasInformation)
			{
				IExecutionMessage[] messages = ((ITaskBase)val).Result.Messages;
				foreach (IExecutionMessage val2 in messages)
				{
					ReportMessage(val2.Source, val2.Message, val2.Level, val2.Exception);
				}
			}
		}

		private string CreatePackageManifest()
		{
			return PackageTransforms.TransformProjectToPackage(base.Project.ProjectFilePath, base.XmlPackageCreation.PackageGuid, metaDataOnly: false);
		}

		private bool ShouldCancel()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)base.Status == 6)
			{
				SetStatus((PackageStatus)7);
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return StringResources.PublishProjectPackageCreation_Name;
		}
	}
}
