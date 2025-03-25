using System;
using System.Collections.Generic;
using System.IO;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ReturnPackageImport : PackageImport, IReturnPackageImport, IPackageImport, IPackageOperation
	{
		private class MyMessageReporter : IPackageOperationMessageReporter
		{
			private readonly ReturnPackageImport _import;

			public MyMessageReporter(ReturnPackageImport import)
			{
				_import = import;
			}

			public void ReportMessage(string source, string message, MessageLevel level)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_import.ReportMessage(source, message, level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_import.ReportMessage(source, message, level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_import.ReportMessage(source, GetFileMessage(projectFile, message), level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_import.ReportMessage(source, GetFileMessage(projectFile, message), level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, ITranslatableFile translatableFile, IMessageLocation fromLocation, IMessageLocation uptoLocation)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_import.ReportMessage(source, GetFileMessage((IProjectFile)(object)translatableFile, message), level, fromLocation, uptoLocation);
			}

			public void ReportCurrentOperation(string currentOperationDescription)
			{
				_import.SetCurrentOperationDescription(currentOperationDescription);
			}

			private string GetFileMessage(IProjectFile projectFile, string message)
			{
				return string.Format(StringResources.ProjectPackageImport_FileMessage, Path.GetFileName(projectFile.LocalFilePath), message);
			}
		}

		private readonly IProjectPackageInitializer _packageInitializer;

		private readonly IPackageImportEvents _packageImportEvents;

		private readonly IProjectsProvider _projectProvider;

		internal Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport XmlReturnPackageImport => (Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport)base.XmlPackageImport;

		internal ReturnPackageImport(IProjectsProvider projectsProvider, string packagePath, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlReturnPackageImport(packagePath), projectPathUtil)
		{
			_packageInitializer = packageInitializer;
			_projectProvider = projectsProvider;
			_packageImportEvents = projectsProvider.Application.PackageImportEvents;
		}

		internal ReturnPackageImport(IProject project, Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport returnPackageImport, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, returnPackageImport, projectPathUtil)
		{
			_projectProvider = ((IProjectConfiguration)project).ProjectsProvider;
			_packageInitializer = packageInitializer;
			_packageImportEvents = _projectProvider.Application.PackageImportEvents;
		}

		protected override void StartImpl()
		{
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_OpenPackage);
			IPackageProject val = _packageInitializer.OpenFrom(XmlReturnPackageImport.Path);
			PackageFilesRepairUtility packageFilesRepairUtility = new PackageFilesRepairUtility((IProject)(object)val);
			if (packageFilesRepairUtility.CheckIfRepairRequired())
			{
				packageFilesRepairUtility.RepairDamagedFiles();
			}
			IProject val2 = FindAffectedProject(val);
			if (val2 == null)
			{
				return;
			}
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_ImportingTasks, 30);
			List<IManualTask> list = new List<IManualTask>();
			List<ITaskFile> list2 = new List<ITaskFile>();
			IManualTask[] packageManualTasks = val.PackageManualTasks;
			foreach (IManualTask val3 in packageManualTasks)
			{
				IManualTask val4 = val2.AddManualTask(val3, (IPackageOperationMessageReporter)(object)new MyMessageReporter(this));
				list.Add(val4);
				list2.AddRange((IEnumerable<ITaskFile>)(object)val4.Files);
			}
			SetProject(val2);
			Tasks = list.ToArray();
			Files = list2.ToArray();
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_ImportingTasks, 50);
			IManualTask[] packageManualTasks2 = val.PackageManualTasks;
			foreach (IManualTask val5 in packageManualTasks2)
			{
				IManualTaskFile[] files = val5.Files;
				foreach (IManualTaskFile val6 in files)
				{
					if (!XliffUtil.IsSdlXliffFile(((ITaskFile)val6).ProjectFile.LocalFilePath))
					{
						string text = ((ITaskFile)val6).ProjectFile.LocalFilePath + ".sdlxliff";
						if (File.Exists(text))
						{
							string targetFilePath = val2.LocalDataFolder + text.Substring(((IProject)val).LocalDataFolder.Length);
							Util.CopyFile(text, targetFilePath);
						}
					}
				}
			}
			SetCurrentOperationDescription(StringResources.ReturnPackageImport_RecomputingStats, 60);
			RecomputeConfirmationStatistics();
			string text2 = XmlProjectPackageCreationBuilder.CreatePackageFilePath(val2, Path.GetFileNameWithoutExtension(XmlReturnPackageImport.Path), (PackageType)1, imported: true);
			Util.CopyFile(XmlReturnPackageImport.Path, text2);
			XmlReturnPackageImport.Path = base.ProjectPathUtil.MakeRelativePath(val2, text2, false);
			XmlReturnPackageImport.PackageGuid = val.PackageGuid;
			XmlReturnPackageImport.Comment = val.Comment;
			SetPercentComplete(100);
			val2.AddReturnPackageImportOperation((IReturnPackageImport)(object)this);
			((IProjectConfiguration)val2).Save();
		}

		private void RecomputeConfirmationStatistics()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			List<ITranslatableFile> list = new List<ITranslatableFile>();
			ITaskFile[] files = Files;
			foreach (ITaskFile val in files)
			{
				if ((int)val.Purpose == 1)
				{
					IProjectFile projectFile = val.ProjectFile;
					ITranslatableFile val2 = (ITranslatableFile)(object)((projectFile is ITranslatableFile) ? projectFile : null);
					if (val2 != null)
					{
						list.Add(val2);
					}
				}
			}
			if (list.Count > 0)
			{
				IProject project = base.Project;
				ITaskTemplate taskTemplateById = _projectProvider.Workflow.GetTaskTemplateById("Sdl.ProjectApi.AutomaticTasks.TranslationCount");
				IProjectFile[] array = (IProjectFile[])(object)list.ToArray();
				IAutomaticCollaborativeTask val3 = project.CreateNewAutomaticTask(taskTemplateById, array);
				val3.Start();
				ProcessTaskResults(((ITaskBase)val3).Result);
			}
		}

		private void ProcessTaskResults(IExecutionResult executionResult)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			MyMessageReporter myMessageReporter = new MyMessageReporter(this);
			IExecutionMessage[] messages = executionResult.Messages;
			foreach (IExecutionMessage val in messages)
			{
				myMessageReporter.ReportMessage(val.Source, val.Message, val.Level, val.Exception);
			}
		}

		private IProject FindAffectedProject(IPackageProject packageProject)
		{
			if (!(_projectProvider.GetProject(((IProject)packageProject).Guid) is Project project))
			{
				ReportMessage(StringResources.ReturnPackageImport_Source, StringResources.ReturnPackageImport_ProjectDoesNotExist, (MessageLevel)2);
				return null;
			}
			if (project.IsImported)
			{
				foreach (ReturnPackageCreation returnPackageCreationOperation in project.ReturnPackageCreationOperations)
				{
					if (returnPackageCreationOperation.XmlPackageOperation.PackageGuid == packageProject.PackageGuid)
					{
						ReportMessage(StringResources.ReturnPackageImport_Source, StringResources.ReturnPackageImport_CreatedYourself, (MessageLevel)2);
						return null;
					}
				}
			}
			foreach (ReturnPackageImport returnPackageImportOperation in project.ReturnPackageImportOperations)
			{
				if (returnPackageImportOperation.XmlPackageOperation.PackageGuid == packageProject.PackageGuid)
				{
					if (!_packageImportEvents.ShouldImportPackageAgain((IPackageImport)(object)this))
					{
						ReportMessage(StringResources.ReturnPackageImport_Source, StringResources.ReturnPackageImport_Cancelled, (MessageLevel)2);
						return null;
					}
					break;
				}
			}
			return (IProject)(object)project;
		}

		public override string ToString()
		{
			return StringResources.ReturnPackageImport_Source;
		}
	}
}
