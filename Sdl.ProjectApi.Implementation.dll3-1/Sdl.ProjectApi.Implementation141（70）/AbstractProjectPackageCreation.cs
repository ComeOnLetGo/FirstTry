using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectApi.Implementation.Services;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Settings;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class AbstractProjectPackageCreation : PackageCreation
	{
		protected class MyMessageReporter : IPackageOperationMessageReporter
		{
			private readonly AbstractProjectPackageCreation _creation;

			public MyMessageReporter(AbstractProjectPackageCreation creation)
			{
				_creation = creation;
			}

			public void ReportMessage(string source, string message, MessageLevel level)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, message, level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, message, level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage(projectFile, message), level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage(projectFile, message), level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, ITranslatableFile translatableFile, IMessageLocation fromLocation, IMessageLocation uptoLocation)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage((IProjectFile)(object)translatableFile, message), level, fromLocation, uptoLocation);
			}

			public void ReportCurrentOperation(string currentOperationDescription)
			{
				_creation.SetCurrentOperationDescription(currentOperationDescription);
			}

			private string GetFileMessage(IProjectFile projectFile, string message)
			{
				return string.Format(StringResources.ProjectPackageCreation_FileMessage, Path.GetFileName(projectFile.LocalFilePath), message);
			}
		}

		private readonly ProjectPackageCreationOptions _options;

		private readonly IWorkflow _workflow;

		protected IProjectPackageInitializer PackageInitializer { get; }

		internal Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation XmlProjectPackageCreation => (Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation)base.XmlPackageCreation;

		internal AbstractProjectPackageCreation(IProject project, Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreation, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, projectPackageCreation, projectPathUtil)
		{
			PackageInitializer = packageInitializer;
			_workflow = ((IProjectConfiguration)project).ProjectsProvider.Workflow;
		}

		internal AbstractProjectPackageCreation(IProject project, Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreation, ProjectPackageCreationOptions options, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, projectPackageCreation, projectPathUtil)
		{
			PackageInitializer = packageInitializer;
			_workflow = ((IProjectConfiguration)project).ProjectsProvider.Workflow;
			_options = options;
		}

		protected override void StartImpl()
		{
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Invalid comparison between Unknown and I4
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Invalid comparison between Unknown and I4
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Invalid comparison between Unknown and I4
			try
			{
				string text = CreateTemporaryPackageFolder();
				SetCurrentOperationDescription(StringResources.ProjectPackageCreation_InitPackage, 0);
				if (IsCancelling())
				{
					CleanUp(text);
					return;
				}
				if (_options.IncludeTermbases)
				{
					string readOnlyFileBasedTermbase = GetReadOnlyFileBasedTermbase(base.Project);
					if (!string.IsNullOrEmpty(readOnlyFileBasedTermbase))
					{
						throw new Exception(string.Format(StringResources.Local_Termbase_Is_Read_Only_Message, readOnlyFileBasedTermbase));
					}
				}
				string path = Path.GetFileNameWithoutExtension(GetAbsolutePackagePath()) + FileTypes.ProjectFileExtension;
				string text2 = Path.Combine(text, path);
				PackageProject packageProject = (PackageProject)(object)PackageInitializer.Create(base.Project.Name, (PackageType)0, base.Project.SourceLanguage, text2, base.Project.Guid, base.Project.CreatedAt, base.Project.CreatedBy);
				XmlProjectPackageCreation.PackageGuid = packageProject.PackageGuid;
				packageProject.Comment = base.Comment;
				CopyProjectProperties(base.Project, (IProject)(object)packageProject);
				IEnumerable<ILanguageDirection> languageDirectionsForPackage = GetLanguageDirectionsForPackage();
				ProjectCascadeItemProvider projectCascadeItemProvider = new ProjectCascadeItemProvider(base.Project);
				ProjectCascadeItem cascadeItemWithoutRedundantEntries = projectCascadeItemProvider.GetCascadeItemWithoutRedundantEntries();
				TranslationMemoryInfoFactory translationMemoryInfoFactory = new TranslationMemoryInfoFactory();
				FilteredProjectCascadeItem filteredProjectCascadeItem = new FilteredProjectCascadeItem(cascadeItemWithoutRedundantEntries, (ITranslationMemoryInfoFactory)(object)translationMemoryInfoFactory, languageDirectionsForPackage);
				packageProject.CopyTranslationProviderCascade(filteredProjectCascadeItem.FilteredCascadeItem, packageProject.CascadeItem, _options.IncludeMainTranslationMemories, (int)_options.ProjectTranslationMemoryOptions == 1, _options.RemoveAutomatedTranslationProviders, null);
				foreach (ILanguageDirection item in languageDirectionsForPackage)
				{
					packageProject.AddLanguageDirection(item, _options.IncludeMainTranslationMemories, (int)_options.ProjectTranslationMemoryOptions == 1, _options.IncludeAutoSuggestDictionaries, _options.RemoveAutomatedTranslationProviders);
				}
				CopyProjectTermbases(base.Project, (IProject)(object)packageProject, _options.IncludeTermbases);
				SetCurrentOperationDescription(StringResources.ProjectPackageCreation_AddingFiles, 20);
				if (IsCancelling())
				{
					CleanUp(text);
					return;
				}
				SyncTranslationMemoriesBetweenProjectAndPackage((IPackageProject)(object)packageProject);
				AddFilesToPackage((IPackageProject)(object)packageProject);
				if ((int)_options.ProjectTranslationMemoryOptions == 2)
				{
					ILanguageDirection[] languageDirections = packageProject.LanguageDirections;
					foreach (ILanguageDirection val in languageDirections)
					{
						ProjectTranslationMemoryTaskSettings settingsGroup = ((IObjectWithSettings)val).Settings.GetSettingsGroup<ProjectTranslationMemoryTaskSettings>();
						settingsGroup.CreateServerBasedProjectTranslationMemories.Value = false;
					}
				}
				SetCurrentOperationDescription(StringResources.ProjectPackageCreation_AddingFiles, 40);
				RunBatchTasks(packageProject);
				DisconnectTermbases(packageProject);
				SetCurrentOperationDescription(StringResources.ProjectPackageCreation_CompressingPackage, 70);
				if (IsCancelling())
				{
					CleanUp(text);
					return;
				}
				SetMainTranslationMemoriesEnabled((IPackageProject)(object)packageProject, !_options.RemoveServerBasedTranslationMemories, _options.IncludeMainTranslationMemories);
				RemoveUnrequiredTranslationMemories((IPackageProject)(object)packageProject);
				packageProject.Save();
				packageProject.CreatePackage(GetAbsolutePackagePath(), saveProject: false);
				SetPercentComplete(100);
				if (IsCancelling())
				{
					CleanUp(text, GetAbsolutePackagePath());
				}
			}
			finally
			{
				OnCreationCompleted();
				((IProjectConfiguration)base.Project).Save();
			}
		}

		private static void DisconnectTermbases(PackageProject packageProject)
		{
			TerminologyDisconnector terminologyDisconnector = new TerminologyDisconnector(packageProject.TermbaseConfiguration, TerminologyProviderManager.Instance);
			terminologyDisconnector.DisconnectTermbases();
		}

		private void RunBatchTasks(PackageProject packageProject)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Invalid comparison between Unknown and I4
			if (_options.RecomputeAnalysisStatistics || (int)_options.ProjectTranslationMemoryOptions == 2)
			{
				RecomputeAnalysisStatisticsAndCreateProjectTM((IPackageProject)(object)packageProject, (IPackageOperationMessageReporter)(object)new MyMessageReporter(this));
			}
			if (_options.IncludeExistingReports)
			{
				IncludeExistingReportsInThePackage(packageProject);
			}
		}

		private void IncludeExistingReportsInThePackage(PackageProject packageProject)
		{
			List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasksContainingReports = GetXmlAutomaticTasksContainingReports(base.Project.AutomaticTasks, packageProject);
			if (xmlAutomaticTasksContainingReports.Any())
			{
				packageProject.AddAutomaticTasks(xmlAutomaticTasksContainingReports);
				CopyProjectReports(xmlAutomaticTasksContainingReports, base.Project.LocalDataFolder, packageProject.LocalDataFolder);
			}
		}

		private List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> GetXmlAutomaticTasksContainingReports(IEnumerable<IAutomaticTask> automaticTasks, PackageProject packageProject)
		{
			List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasksCloned = GetXmlAutomaticTasksCloned(automaticTasks, packageProject);
			RemoveReportsDependingOnPackageLanguageDirection(xmlAutomaticTasksCloned, packageProject.LanguageDirections);
			xmlAutomaticTasksCloned.RemoveAll((Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask) => !xmlAutomaticTask.Reports.Any());
			UpdateLanguageGuids(xmlAutomaticTasksCloned, packageProject.LanguageDirections);
			return xmlAutomaticTasksCloned;
		}

		private List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> GetXmlAutomaticTasksCloned(IEnumerable<IAutomaticTask> automaticTasks, PackageProject packageProject)
		{
			List<AutomaticTask> list = (from automaticTask in automaticTasks.OfType<AutomaticTask>()
				where automaticTask.Reports.Any()
				select automaticTask).ToList();
			IEnumerable<string> filesIncludedInPackage = GetFilesIncludedInPackage(packageProject);
			List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> list2 = new List<Sdl.ProjectApi.Implementation.Xml.AutomaticTask>();
			foreach (AutomaticTask item2 in list)
			{
				if (IsTaskContainingAtLeastOneIncludedFile(item2, filesIncludedInPackage))
				{
					Sdl.ProjectApi.Implementation.Xml.AutomaticTask item = CloneableService.Clone((Sdl.ProjectApi.Implementation.Xml.AutomaticTask)item2.XmlTask);
					list2.Add(item);
				}
			}
			return list2;
		}

		private IEnumerable<string> GetFilesIncludedInPackage(PackageProject packageProject)
		{
			return (from file in packageProject.ManualTasks.SelectMany((IManualTask task) => task.Files)
				select Path.GetFileNameWithoutExtension(((ITaskFile)file).ProjectFile.Filename)).Distinct();
		}

		private bool IsTaskContainingAtLeastOneIncludedFile(AutomaticTask automaticTask, IEnumerable<string> filesIncludedInPackage)
		{
			return filesIncludedInPackage.Any((string file) => automaticTask.Files.Any((ITaskFile automaticTaskFile) => GetFileName(automaticTaskFile) == file));
		}

		private string GetFileName(ITaskFile automaticTaskFile)
		{
			string filename = automaticTaskFile.ProjectFile.Filename;
			if (!filename.Contains(".sdlxliff"))
			{
				return filename;
			}
			return Path.GetFileNameWithoutExtension(filename);
		}

		private void RemoveReportsDependingOnPackageLanguageDirection(IEnumerable<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasks, IEnumerable<ILanguageDirection> languageDirections)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask in xmlAutomaticTasks)
			{
				xmlAutomaticTask.Reports.RemoveAll((TaskReport report) => IsReportLanguageNotContainedInLanguageDirections(languageDirections, report));
			}
		}

		private bool IsReportLanguageNotContainedInLanguageDirections(IEnumerable<ILanguageDirection> languageDirections, TaskReport report)
		{
			return languageDirections.All((ILanguageDirection language) => language.TargetLanguage.DisplayName != GetProjectTargetLanguageDirectionName(report));
		}

		private void UpdateLanguageGuids(IEnumerable<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasks, ILanguageDirection[] languageDirections)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask in xmlAutomaticTasks)
			{
				foreach (TaskReport report in xmlAutomaticTask.Reports)
				{
					string targetLanguageDisplayName = GetProjectTargetLanguageDirectionName(report);
					ILanguageDirection val = languageDirections.FirstOrDefault((ILanguageDirection languageDirection) => languageDirection.TargetLanguage.DisplayName == targetLanguageDisplayName);
					report.LanguageDirectionGuid = val.Guid;
				}
			}
		}

		private string GetProjectTargetLanguageDirectionName(TaskReport taskReport)
		{
			ILanguageDirection val = ((IProjectConfiguration)base.Project).LanguageDirections.FirstOrDefault((ILanguageDirection languageDirection) => languageDirection.Guid == taskReport.LanguageDirectionGuid);
			if (val != null)
			{
				return val.TargetLanguage.DisplayName;
			}
			return null;
		}

		protected abstract void AddFilesToPackage(IPackageProject pkgProject);

		protected abstract IEnumerable<ILanguageDirection> GetLanguageDirectionsForPackage();

		protected abstract void OnCreationCompleted();

		private bool IsCancelling()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)base.Status == 6;
		}

		private void CleanUp(string tempPackageLocalFolder0)
		{
			if (Directory.Exists(tempPackageLocalFolder0))
			{
				Directory.Delete(tempPackageLocalFolder0, recursive: true);
			}
		}

		private void CleanUp(string tempPackageLocalFolder0, string packageFilePath)
		{
			if (File.Exists(packageFilePath))
			{
				File.Delete(packageFilePath);
			}
			CleanUp(tempPackageLocalFolder0);
		}

		public override string ToString()
		{
			return StringResources.ProjectPackageCreation_Source;
		}

		private void RecomputeAnalysisStatisticsAndCreateProjectTM(IPackageProject packageProject, IPackageOperationMessageReporter messageReporter)
		{
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Invalid comparison between Unknown and I4
			List<ITranslatableFile> list = new List<ITranslatableFile>();
			List<ITaskTemplate> list2 = new List<ITaskTemplate>();
			Language[] targetLanguages = ((IProject)packageProject).TargetLanguages;
			foreach (Language val in targetLanguages)
			{
				list.AddRange(((IProject)packageProject).GetTranslatableFiles(val));
			}
			IProjectFile[] array;
			if (_options.RecomputeAnalysisStatistics)
			{
				list2.Add(_workflow.GetTaskTemplateById("Sdl.ProjectApi.AutomaticTasks.Analysis"));
				ITranslatableFile[] sourceLanguageFileForPackage = GetSourceLanguageFileForPackage(packageProject);
				if (sourceLanguageFileForPackage.Length != 0)
				{
					ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById("Sdl.ProjectApi.AutomaticTasks.WordCount");
					array = (IProjectFile[])(object)sourceLanguageFileForPackage;
					IAutomaticCollaborativeTask val2 = ((IProject)packageProject).CreateNewAutomaticTask(taskTemplateById, array);
					val2.Start();
					if (((ITaskBase)val2).Result.HasErrors || ((ITaskBase)val2).Result.HasWarnings || ((ITaskBase)val2).Result.HasInformation)
					{
						ProcessTaskResults(((ITaskBase)val2).Result, messageReporter);
					}
				}
			}
			if ((int)_options.ProjectTranslationMemoryOptions == 2)
			{
				list2.Add(_workflow.GetTaskTemplateById("Sdl.ProjectApi.AutomaticTasks.ProjectTm"));
			}
			IComplexTaskTemplate val3 = _workflow.CreateComplexTaskTemplate("PackageTestExecution", "PackageTestExecution", list2.ToArray());
			array = (IProjectFile[])(object)list.ToArray();
			IAutomaticCollaborativeTask val4 = ((IProject)packageProject).CreateNewAutomaticTask((ITaskTemplate)(object)val3, array);
			val4.Start();
			if (((ITaskBase)val4).Result.HasErrors || ((ITaskBase)val4).Result.HasWarnings || ((ITaskBase)val4).Result.HasInformation)
			{
				ProcessTaskResults(((ITaskBase)val4).Result, messageReporter);
			}
		}

		private void ProcessTaskResults(IExecutionResult executionResult, IPackageOperationMessageReporter messageReporter)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			IExecutionMessage[] messages = executionResult.Messages;
			foreach (IExecutionMessage val in messages)
			{
				messageReporter.ReportMessage(val.Source, val.Message, val.Level, val.Exception);
			}
		}

		private ITranslatableFile[] GetSourceLanguageFileForPackage(IPackageProject packageProject)
		{
			IEnumerable<ITranslatableFile> source = (from task in Tasks
				from file in task.Files
				join pkgFile in ((IProject)packageProject).GetTranslatableFiles(((IProject)packageProject).SourceLanguage) on ((ITaskFile)file).ProjectFile.Guid equals ((IProjectFile)pkgFile).Guid
				where (int)((ITaskFile)file).Purpose == 1 && ((ITaskFile)file).ProjectFile is ITranslatableFile && ((ILanguageFile)pkgFile).IsSource
				select pkgFile).Distinct();
			return source.ToArray();
		}

		private void SyncTranslationMemoriesBetweenProjectAndPackage(IPackageProject project)
		{
			SyncTranslationMemoriesBetweenProjectAndPackage(((IProjectConfiguration)project).CascadeItem.CascadeEntryItems, null);
			ILanguageDirection[] languageDirections = ((IProjectConfiguration)project).LanguageDirections;
			foreach (ILanguageDirection languageDirection in languageDirections)
			{
				ILanguageDirection projectLanguageDirection = ((IProjectConfiguration)base.Project).LanguageDirections.First((ILanguageDirection ld) => ((object)ld.TargetLanguage).Equals((object)languageDirection.TargetLanguage));
				SyncTranslationMemoriesBetweenProjectAndPackage(languageDirection.CascadeItem.CascadeEntryItems, projectLanguageDirection);
			}
		}

		private void SyncTranslationMemoriesBetweenProjectAndPackage(IEnumerable<ProjectCascadeEntryItem> cascadeEntryItems, ILanguageDirection projectLanguageDirection = null)
		{
			foreach (ProjectCascadeEntryItem entry in cascadeEntryItems)
			{
				ProjectCascadeEntryItem val = ((projectLanguageDirection == null) ? ((IProjectConfiguration)base.Project).CascadeItem.CascadeEntryItems.Where((ProjectCascadeEntryItem c) => c.MainTranslationProviderItem.Uri == entry.MainTranslationProviderItem.Uri).FirstOrDefault() : projectLanguageDirection.CascadeItem.CascadeEntryItems.Where((ProjectCascadeEntryItem c) => c.MainTranslationProviderItem.Uri == entry.MainTranslationProviderItem.Uri).FirstOrDefault());
				if (val == null)
				{
					continue;
				}
				entry.MainTranslationProviderItem.Enabled = val.MainTranslationProviderItem.Enabled;
				foreach (ITranslationProviderItem projectProvider in val.ProjectTranslationProviderItems)
				{
					ITranslationProviderItem val2 = val.ProjectTranslationProviderItems.Where((ITranslationProviderItem p) => p.Uri == projectProvider.Uri).FirstOrDefault();
					if (val2 != null)
					{
						projectProvider.Enabled = val2.Enabled;
					}
				}
			}
		}

		private void SetMainTranslationMemoriesEnabled(IPackageProject project, bool enableServerBased, bool enableFileBased)
		{
			SetMainTranslationMemoriesEnabled((IEnumerable<ProjectCascadeEntryItem>)((IProjectConfiguration)project).CascadeItem.CascadeEntryItems, enableServerBased, enableFileBased);
			ILanguageDirection[] languageDirections = ((IProjectConfiguration)project).LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				SetMainTranslationMemoriesEnabled((IEnumerable<ProjectCascadeEntryItem>)val.CascadeItem.CascadeEntryItems, enableServerBased, enableFileBased);
			}
		}

		private void SetMainTranslationMemoriesEnabled(IEnumerable<ProjectCascadeEntryItem> cascadeEntryItems, bool enableServerBased, bool enableFileBased)
		{
			foreach (ProjectCascadeEntryItem cascadeEntryItem in cascadeEntryItems)
			{
				if (FileBasedTranslationMemory.IsFileBasedTranslationMemory(cascadeEntryItem.MainTranslationProviderItem.Uri))
				{
					cascadeEntryItem.MainTranslationProviderItem.Enabled = enableFileBased;
				}
				else if (ServerBasedTranslationMemory.IsServerBasedTranslationMemory(cascadeEntryItem.MainTranslationProviderItem.Uri))
				{
					cascadeEntryItem.MainTranslationProviderItem.Enabled = enableServerBased;
				}
			}
		}

		private void RemoveUnrequiredTranslationMemories(IList<ProjectCascadeEntryItem> cascadeEntryItems)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			for (int num = cascadeEntryItems.Count - 1; num >= 0; num--)
			{
				ProjectCascadeEntryItem val = cascadeEntryItems[num];
				if ((int)_options.ProjectTranslationMemoryOptions == 0)
				{
					val.ProjectTranslationProviderItems.Clear();
				}
				else if (_options.RemoveServerBasedTranslationMemories)
				{
					val.ProjectTranslationProviderItems = val.ProjectTranslationProviderItems.Where((ITranslationProviderItem item) => !item.IsServerBasedTranslationMemory()).ToList();
				}
				if (!val.ProjectTranslationProviderItems.Any())
				{
					if (val.MainTranslationProviderItem.IsFileBasedTranslationMemory() && !_options.IncludeMainTranslationMemories)
					{
						cascadeEntryItems.RemoveAt(num);
					}
					else if (val.MainTranslationProviderItem.IsServerBasedTranslationMemory() && _options.RemoveServerBasedTranslationMemories)
					{
						cascadeEntryItems.RemoveAt(num);
					}
					else if (val.MainTranslationProviderItem.IsMachineTranslation() && _options.RemoveAutomatedTranslationProviders)
					{
						cascadeEntryItems.RemoveAt(num);
					}
				}
			}
		}

		private void RemoveUnrequiredTranslationMemories(IPackageProject project)
		{
			RemoveUnrequiredTranslationMemories(((IProjectConfiguration)project).CascadeItem.CascadeEntryItems);
			ILanguageDirection[] languageDirections = ((IProjectConfiguration)project).LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				RemoveUnrequiredTranslationMemories(val.CascadeItem.CascadeEntryItems);
			}
			((IProjectConfiguration)(object)project).RemoveRedundantCascadeEntryItemsFromAllLanguagePairs(!_options.RemoveServerBasedTranslationMemories);
		}
	}
}
