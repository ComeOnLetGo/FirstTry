using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.Core.TM.ImportExport;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.MultiTerm.Core.Settings;
using Sdl.MultiTerm.TMO.Interop;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.TermbaseApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectPackageImport : PackageImport, IProjectPackageImport, IPackageImport, IPackageOperation
	{
		private class MyMessageReporter : IPackageOperationMessageReporter
		{
			private readonly ProjectPackageImport _import;

			public MyMessageReporter(ProjectPackageImport import)
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

		private const string LCTermbasesFolder = "LCTermbases";

		private const string SdlxliffExtension = ".sdlxliff";

		private readonly IPackageProject _packageProject;

		private readonly IPackageImportEvents _packageImportEvents;

		private readonly IProjectsProvider _projectsProvider;

		public string ProposedProjectFolder { get; }

		internal Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport XmlProjectPackageImport => (Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport)base.XmlPackageImport;

		internal ProjectPackageImport(IProjectsProvider projectsProvider, string packagePath, string proposedProjectFolder, IPackageProject packageProject, IProjectPathUtil projectPathUtil)
			: base(new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlProjectPackageImport(packagePath), projectPathUtil)
		{
			ProposedProjectFolder = proposedProjectFolder;
			_packageProject = packageProject;
			_packageImportEvents = projectsProvider.Application.PackageImportEvents;
			_projectsProvider = projectsProvider;
		}

		internal ProjectPackageImport(IProject project, Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport projectPackageImport, IProjectPathUtil projectPathUtil)
			: base(project, projectPackageImport, projectPathUtil)
		{
			_packageImportEvents = ((IProjectConfiguration)project).ProjectsProvider.Application.PackageImportEvents;
			_projectsProvider = ((IProjectConfiguration)project).ProjectsProvider;
		}

		protected override void StartImpl()
		{
			Project project = null;
			bool isNewProject = false;
			try
			{
				SetCurrentOperationDescription(StringResources.ProjectPackageImport_OpenPackage);
				IPackageProject val = _packageProject ?? _projectsProvider.PackageInitializer.OpenFrom(XmlProjectPackageImport.Path);
				PackageFilesRepairUtility packageFilesRepairUtility = new PackageFilesRepairUtility((IProject)(object)val);
				if (packageFilesRepairUtility.CheckIfRepairRequired())
				{
					packageFilesRepairUtility.RepairDamagedFiles();
				}
				base.XmlPackageOperation.Comment = val.Comment;
				base.XmlPackageOperation.PackageGuid = val.PackageGuid;
				project = ImportPackageProject(val, out isNewProject);
				if (project == null)
				{
					SetStatus((PackageStatus)7);
					return;
				}
				if (!string.IsNullOrEmpty(val.PackageLicenseInfo))
				{
					ISettingsGroup settingsGroup = project.Settings.GetSettingsGroup("PackageLicenseInfo");
					settingsGroup.GetSetting<string>("Grant").Value = val.PackageLicenseInfo;
				}
				SetCurrentOperationDescription(StringResources.ReturnPackageImport_BackingUpPackage, 90);
				string text = XmlProjectPackageCreationBuilder.CreatePackageFilePath((IProject)(object)project, Path.GetFileNameWithoutExtension(XmlProjectPackageImport.Path), (PackageType)0, imported: true);
				Util.CopyFile(XmlProjectPackageImport.Path, text);
				XmlProjectPackageImport.Path = base.ProjectPathUtil.MakeRelativePath((IProject)(object)project, text, false);
				SetPercentComplete(100);
				project.AddProjectPackageImportOperation((IProjectPackageImport)(object)this);
				project.Save();
				SetProject((IProject)(object)project);
			}
			catch (CancelException)
			{
			}
			catch
			{
				try
				{
					if (project != null && isNewProject)
					{
						project.Delete();
					}
				}
				catch
				{
				}
				throw;
			}
		}

		public override string ToString()
		{
			return StringResources.ProjectPackageImport_Source;
		}

		private Project ImportPackageProject(IPackageProject packageProject, out bool isNewProject)
		{
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			Project project = (Project)(object)_projectsProvider.GetProject(((IProject)packageProject).Guid);
			isNewProject = project?.IsCloudBased ?? true;
			if (isNewProject)
			{
				SetCurrentOperationDescription(StringResources.ProjectPackageImport_CreatingProject, 20);
				string text = ProposedProjectFolder;
				if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
				{
					text = _packageImportEvents.GetLocalDataFolderForNewProject((IProjectPackageImport)(object)this, _projectsProvider, ((IProject)packageProject).Name);
				}
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				string text2 = Path.Combine(text, ((IProject)packageProject).Name + FileTypes.ProjectFileExtension);
				project = _projectsProvider.CreateNewProject(text2, packageProject) as Project;
				CopyProjectProperties((IProject)(object)packageProject, (IProject)(object)project);
			}
			else
			{
				VerifyIfFileExistsLocally((IProject)(object)project, packageProject);
				SetCurrentOperationDescription(StringResources.ProjectPackageImport_UpdatingProject, 20);
				PublicationStatus[] source = (PublicationStatus[])(object)new PublicationStatus[3]
				{
					(PublicationStatus)7,
					(PublicationStatus)5,
					default(PublicationStatus)
				};
				if (!source.Contains(project.PublishProjectOperation.Status))
				{
					throw new ProjectApiException(ErrorMessages.ProjectPackageImport_ProjectIsServerProject);
				}
				if (!project.IsImported && !project.IsLCProject)
				{
					throw new ProjectApiException(ErrorMessages.ProjectPackageImport_ProjectNotImported);
				}
				if (packageProject.PackageGuid != Guid.Empty && project.ProjectPackageCreationOperations.Cast<ProjectPackageCreation>().Any((ProjectPackageCreation creation) => creation.XmlPackageOperation.PackageGuid == packageProject.PackageGuid))
				{
					throw new ProjectApiException(ErrorMessages.ProjectPackageImport_CreatedYourself);
				}
				if (packageProject.PackageGuid != Guid.Empty)
				{
					foreach (ProjectPackageImport projectPackageImportOperation in project.ProjectPackageImportOperations)
					{
						if (projectPackageImportOperation.XmlPackageOperation.PackageGuid == packageProject.PackageGuid)
						{
							if (!_packageImportEvents.ShouldImportPackageAgain((IPackageImport)(object)this))
							{
								throw new ProjectApiException(ErrorMessages.ProjectPackageImport_Cancelled);
							}
							break;
						}
					}
				}
				UpdateProject(project, packageProject);
			}
			project.PackageConvertor = ((IProject)packageProject).PackageConvertor;
			SetProject((IProject)(object)project);
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_ImportingLanguageDirections, 30);
			UpdateTranslationProviderCascade(((IProjectConfiguration)packageProject).CascadeItem, project.CascadeItem, project, null);
			AddOrUpdateLanguageDirections(packageProject, project);
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_ImportingTermbases, 40);
			ConvertLCTermbasesToLocalTb((IProject)(object)packageProject);
			CopyProjectTermbases((IProject)(object)packageProject, (IProject)(object)project, includeFileBasedTermbases: true);
			SetCurrentOperationDescription(StringResources.ProjectPackageImport_ImportingTasks, 50);
			List<IManualTask> list = new List<IManualTask>();
			List<ITaskFile> list2 = new List<ITaskFile>();
			IManualTask[] packageManualTasks = packageProject.PackageManualTasks;
			foreach (IManualTask val in packageManualTasks)
			{
				Project project2 = project;
				ITaskFile[] files = (ITaskFile[])(object)val.Files;
				IManualTask val2 = project2.AddManualTask(val, files, project.CurrentUser, (IPackageOperationMessageReporter)(object)new MyMessageReporter(this));
				list.Add(val2);
				list2.AddRange((IEnumerable<ITaskFile>)(object)val2.Files);
			}
			Tasks = list.ToArray();
			Files = list2.ToArray();
			AddNativeFileVersionForSourceFiles(packageProject, project);
			AddAutomaticTaskReports(packageProject, project);
			return project;
		}

		private void AddNativeFileVersionForSourceFiles(IPackageProject packageProject, Project project)
		{
			string path = Path.Combine(((IProject)packageProject).LocalDataFolder, ((LanguageBase)((IProject)packageProject).SourceLanguage).IsoAbbreviation);
			if (!Directory.Exists(path))
			{
				return;
			}
			IEnumerable<string> enumerable = from file in Directory.GetFiles(path)
				where !file.EndsWith(".sdlxliff")
				select file;
			ITranslatableFile[] translatableFiles = project.GetTranslatableFiles(project.SourceLanguage);
			foreach (string file2 in enumerable)
			{
				ITranslatableFile val = translatableFiles.FirstOrDefault((ITranslatableFile item) => Path.GetFileNameWithoutExtension(((ILanguageFile)item).Revisions.First().LocalFilePath).Equals(Path.GetFileName(file2)));
				if (val != null)
				{
					project.ProjectRepository.AddNativeFileVersionForSourceFile((IProject)(object)project, val, file2);
				}
			}
		}

		private void VerifyIfFileExistsLocally(IProject project, IPackageProject packageProject)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			if (!project.IsLCProject)
			{
				return;
			}
			IProjectFile projectFile = ((ITaskFile)packageProject.PackageManualTasks[0].Files[0]).ProjectFile;
			string currentFilename = ((ILanguageFile)((projectFile is ILanguageFile) ? projectFile : null)).CurrentFilename;
			IManualTask[] manualTasks = project.ManualTasks;
			foreach (IManualTask val in manualTasks)
			{
				IProjectFile projectFile2 = ((ITaskFile)val.Files[0]).ProjectFile;
				ILanguageFile val2 = (ILanguageFile)(object)((projectFile2 is ILanguageFile) ? projectFile2 : null);
				if (val2.CurrentFilename.Equals(currentFilename, StringComparison.InvariantCultureIgnoreCase) && File.Exists(((IProjectFile)val2).LocalFilePath))
				{
					throw new ProjectApiException(ErrorMessages.ProjectPackageImport_FileAlreadyExistsLocally);
				}
			}
		}

		private void ConvertLCTermbasesToLocalTb(IProject packageProject)
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			//IL_0079: Expected O, but got Unknown
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			string path = packageProject.LocalDataFolder + "\\LCTermbases\\";
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] files = Directory.GetFiles(path, "*.xml");
			string[] array = files;
			foreach (string text in array)
			{
				CheckCancel();
				string termbaseName = Path.GetFileNameWithoutExtension(text);
				SetCurrentOperationDescription(string.Format(StringResources.ProjectPackageImport_ImportingTermbaseFile, termbaseName), 40);
				TermbaseImport termbaseImport = new TermbaseImport((IApplication)new ApplicationClass(), (IFile)new FileWrapper());
				string path2 = termbaseImport.Import(termbaseName, text);
				TermbaseSettings val = new TermbaseSettings
				{
					Path = path2,
					Local = true,
					IsCustom = false
				};
				if (!((IEnumerable<IProjectTermbase>)((IProjectConfiguration)packageProject).TermbaseConfiguration.Termbases).Where((IProjectTermbase tb) => tb.Name == termbaseName).Any())
				{
					((ICollection<IProjectTermbase>)((IProjectConfiguration)packageProject).TermbaseConfiguration.Termbases).Add((IProjectTermbase)(object)new ProjectTermbase(termbaseName, val.ToXml(), null, enabled: true));
				}
			}
		}

		internal void UpdateProject(Project project, IPackageProject packageProject)
		{
			CopyProjectProperties((IProject)(object)packageProject, (IProject)(object)project);
		}

		private void AddOrUpdateLanguageDirections(IPackageProject packageProject, Project project)
		{
			ILanguageDirection[] languageDirections = ((IProjectConfiguration)packageProject).LanguageDirections;
			foreach (ILanguageDirection packageLanguageDirection in languageDirections)
			{
				AddOrUpdateLanguageDirection(packageProject, project, packageLanguageDirection);
			}
		}

		private LanguageDirection AddOrUpdateLanguageDirection(IPackageProject packageProject, Project project, ILanguageDirection packageLanguageDirection)
		{
			LanguageDirection languageDirection = (LanguageDirection)(object)project.GetLanguageDirection(project.SourceLanguage, packageLanguageDirection.TargetLanguage);
			if (languageDirection == null)
			{
				languageDirection = (LanguageDirection)(object)project.AddLanguageDirection(project.SourceLanguage, packageLanguageDirection.TargetLanguage);
			}
			UpdateLanguageDirection(project, packageLanguageDirection, languageDirection, packageProject);
			return languageDirection;
		}

		private void UpdateLanguageDirection(Project project, ILanguageDirection packageLanguageDirection, LanguageDirection languageDirection, IPackageProject packageProject)
		{
			languageDirection.InitializeSettingsFromConfiguration((IProjectConfiguration)(object)packageProject, copyTms: false);
			UpdateAutoSuggestDictionaries(project, packageLanguageDirection, languageDirection);
			UpdateTranslationProviderCascade(packageLanguageDirection.CascadeItem, languageDirection.CascadeItem, project, languageDirection);
		}

		private void UpdateAutoSuggestDictionaries(Project project, ILanguageDirection packageLanguageDirection, LanguageDirection languageDirection)
		{
			project.CopyAutoSuggestDictionaries(packageLanguageDirection, (ILanguageDirection)(object)languageDirection, copyAutoSuggestDictionaries: true);
		}

		private void UpdateTranslationProviderCascade(ProjectCascadeItem fromCascadeItem, ProjectCascadeItem toCascadeItem, Project toProject, LanguageDirection toLanguageDirection)
		{
			toCascadeItem.CascadeEntryItems.Clear();
			toCascadeItem.StopSearchingWhenResultsFound = fromCascadeItem.StopSearchingWhenResultsFound;
			toCascadeItem.OverrideParent = fromCascadeItem.OverrideParent;
			if (!fromCascadeItem.OverrideParent)
			{
				return;
			}
			foreach (ProjectCascadeEntryItem cascadeEntryItem in fromCascadeItem.CascadeEntryItems)
			{
				ITranslationProviderItem firstProjectTranslationProviderItem = cascadeEntryItem.GetFirstProjectTranslationProviderItem();
				if (firstProjectTranslationProviderItem != null && firstProjectTranslationProviderItem.IsFileBasedTranslationMemory())
				{
					string fileBasedTranslationMemoryFilePath = FileBasedTranslationMemory.GetFileBasedTranslationMemoryFilePath(firstProjectTranslationProviderItem.Uri);
					string fileName = Path.GetFileName(fileBasedTranslationMemoryFilePath);
					string text = Path.Combine(ProjectTranslationMemoryUtil.GetProjectTmFolder((IProject)(object)toProject, (ILanguageDirection)(object)toLanguageDirection), fileName);
					bool flag = File.Exists(text);
					ProjectCascadeEntryItem val = toProject.CopyTranslationProviderCascadeEntry(cascadeEntryItem, toCascadeItem, copyMainTm: true, !flag, (ILanguageDirection)(object)toLanguageDirection);
					if (flag)
					{
						FileBasedTranslationMemory fileBasedTranslationMemory = GetFileBasedTranslationMemory(toProject, firstProjectTranslationProviderItem, performUpdate: false);
						MergeProjectTranslationMemories(toProject, fileBasedTranslationMemory, text);
						ITranslationProviderItem val2 = ((ICopyable<ITranslationProviderItem>)(object)firstProjectTranslationProviderItem).Copy();
						val2.Uri = FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(text);
						val.ProjectTranslationProviderItems.Add(val2);
					}
				}
				else
				{
					toProject.CopyTranslationProviderCascadeEntry(cascadeEntryItem, toCascadeItem, copyMainTm: true, copyProjectTm: true, (ILanguageDirection)(object)toLanguageDirection);
				}
			}
		}

		private void MergeProjectTranslationMemories(Project project, FileBasedTranslationMemory fromTm, string toTmFilePath)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			Uri fileBasedTranslationMemoryUri = FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(toTmFilePath);
			string empty = string.Empty;
			TranslationProviderItem translationProviderItem = new TranslationProviderItem(fileBasedTranslationMemoryUri, empty, true);
			FileBasedTranslationMemory fileBasedTranslationMemory = GetFileBasedTranslationMemory(project, (ITranslationProviderItem)(object)translationProviderItem, performUpdate: true);
			string tempFile = Util.GetTempFile("tmx");
			TranslationMemoryExporter val = new TranslationMemoryExporter(((AbstractLocalTranslationMemory)fromTm).LanguageDirection);
			val.Export(tempFile, true);
			ImportSettings val2 = new ImportSettings();
			val2.IsDocumentImport = false;
			val2.NewFields = (NewFieldsOption)0;
			TranslationMemoryImporter val3 = new TranslationMemoryImporter(((AbstractLocalTranslationMemory)fileBasedTranslationMemory).LanguageDirection);
			((Importer)val3).ImportSettings = val2;
			val3.Import(tempFile);
		}

		private FileBasedTranslationMemory GetFileBasedTranslationMemory(Project project, ITranslationProviderItem translationProviderItem, bool performUpdate)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			ITranslationProviderCache translationProviderCache = project.TranslationProviderCache;
			Uri uri = translationProviderItem.Uri;
			string state = translationProviderItem.State;
			ITranslationProviderCredentialStore translationProviderCredentialStore = project.ProjectsProvider.Application.TranslationProviderCredentialStore;
			return (FileBasedTranslationMemory)translationProviderCache.GetTranslationProvider(uri, state, translationProviderCredentialStore, performUpdate);
		}

		private void AddAutomaticTaskReports(IPackageProject packageProject, Project project)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			IAutomaticTask[] automaticTasks = ((IProject)packageProject).AutomaticTasks;
			foreach (IAutomaticTask val in automaticTasks)
			{
				TaskId id = ((ITaskBase)val).Id;
				AutomaticTask automaticTask = project.GetAutomaticTask(((TaskId)(ref id)).ToGuidArray()[0]);
				if (automaticTask != null)
				{
					IReport[] reports = ((ITaskBase)val).Reports;
					for (int j = 0; j < reports.Length; j++)
					{
						Report report = (Report)(object)reports[j];
						string reportXml = report.GetReportXml();
						ILanguageDirection languageDirection = ((report.LanguageDirection != null) ? project.GetLanguageDirection(report.LanguageDirection.SourceLanguage, report.LanguageDirection.TargetLanguage) : null);
						automaticTask.AddReport(report.TaskTemplate, report.Name, report.Description, reportXml, languageDirection);
					}
				}
			}
		}

		private void CheckCancel()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if ((int)base.Status == 6)
			{
				throw new CancelException();
			}
		}
	}
}
