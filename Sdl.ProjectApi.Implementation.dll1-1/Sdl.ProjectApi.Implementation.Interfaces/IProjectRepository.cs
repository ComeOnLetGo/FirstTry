using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	public interface IProjectRepository : IProjectConfigurationRepository
	{
		bool IsInitialized { get; }

		string SourceLanguage { get; set; }

		Guid ProjectGuid { get; }

		GeneralProjectInfo ProjectInfo { get; }

		Guid ProjectTemplateGuid { get; set; }

		Guid ReferenceProjectGuid { get; set; }

		IEnumerable<Guid> LanguageDirectionSettingsGuids { get; }

		void Load(string projectFilePath);

		void Save(string projectFilePath);

		void MarkProjectAsStarted();

		void UpdateSourceLanguageForFiles(string oldLanguageCode, string replacementLanguage);

		IProjectFile AddProjectFile(IProject project, string fileToAdd, string folderInProject, Language sourceLanguage, FileRole fileRole = 0);

		ITranslatableFile AddTranslatableFile(IProject project, ITranslatableFile translatableFile, ITranslatableFile referenceFile, IPackageOperationMessageReporter messageReporter);

		ITranslatableFile AddTranslatableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, string filterDefinitionId, ITranslatableFile referenceFile);

		ILocalizableFile AddLocalizableFile(IProject project, ILocalizableFile localizableFile, ILocalizableFile referenceFile);

		IReferenceFile AddReferenceFile(IProject project, IReferenceFile referenceFile, IReferenceFile parentReferenceFile);

		IMergedTranslatableFile CreateMergedTranslatableFile(IProject project, string mergedFileName, string folderInProject, Language language, string fileTypeDefinitionId, ITranslatableFile[] childFiles);

		bool SplitFileIntoTargetLanguage(IProject project, ILanguageFile languageFile, Language targetLanguage, out IProjectFile targetLanguageFile);

		IProjectFile GetProjectFile(IProject project, string fileName, string folderName, Language language);

		IProjectFile GetProjectFile(IProject project, Guid projectFileGuid);

		ITranslatableFile GetTranslatableFile(IProject project, Guid languageFileGuid);

		ITranslatableFile GetTranslatableFile(IProject project, string fileName, string folderName, Language language);

		List<ITranslatableFile> GetTranslatableTargetOrSingleDocumentFiles(IProject project);

		List<IProjectFile> GetProjectFiles(IProject project, bool allFiles = false);

		List<IProjectFile> GetProjectFiles(IProject project, Language language);

		List<IProjectFile> GetPagedProjectFiles(IProject project, Language language, int pageSize, int skip);

		IMergedTranslatableFile GetMergedTranslatableFile(IProject project, Guid projectFileGuid);

		List<IMergedTranslatableFile> GetMergedTranslatableFileHistory(IProject project, Guid projectFileGuid);

		ILanguageFile GetSourceLanguageFile(IProject project, ILanguageFile targetLanguageFile);

		void ResetAnalysisStatics(IProject project);

		void ResetWordCountStatistics();

		IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, DateTime dueDate, IUser createdBy, DateTime createdAt);

		IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, string externalId, DateTime startedAt, DateTime dueDate, IUser createdBy, DateTime createdAt, DateTime completedAt, int percentComplete);

		List<IScheduledTask> GetTasks(IProject project);

		void RemoveTask(IScheduledTask task);

		AutomaticTask AddAutomaticTasks(IProject project, Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask);

		AutomaticTask AddAutomaticTasks(IProject project, Guid newGuid, Guid previousTaskGuid, ITaskTemplate[] subTaskTemplates, IUser currentUser, string complexTaskTemplateId);

		AutomaticTask AddAutomaticTasks(IProject project, IUser currentUser, string taskTemplateId);

		AutomaticTask AddAutomaticTasks(IProject project, IAutomaticTask task);

		List<IUser> GetUsers();

		void RemoveLanguageFile(ILanguageFile languageFile);

		void AddProjectPackageCreationOperation(IProjectPackageCreation c);

		void AddProjectPackageImportOperation(IProjectPackageImport i);

		void AddReturnPackageCreationOperation(IReturnPackageCreation c);

		void AddReturnPackageImportOperation(IReturnPackageImport i);

		List<IProjectPackageCreation> LoadProjectPackageCreations(IProject project, IProjectPackageInitializer packageInitializer);

		List<IProjectPackageImport> LoadProjectPackageImports(IProject project);

		List<IReturnPackageCreation> LoadReturnPackageCreations(IProject project, IProjectPackageInitializer packageInitializer);

		List<IReturnPackageImport> LoadReturnPackageImports(IProject project, IProjectPackageInitializer packageInitializer);

		Dictionary<string, string> GetPhysicalFiles(IProject project);

		void AddNativeFileVersionForSourceFile(IProject project, ITranslatableFile file, string nativeFilePath);

		ILocalizableFile AddLocalizableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, ILocalizableFile referenceFile);
	}
}
