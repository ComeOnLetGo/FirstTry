using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Sdl.ProjectApi.Implementation
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class ErrorMessages
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("Sdl.ProjectApi.Implementation.ErrorMessages", typeof(ErrorMessages).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string Application_ErrorDeserializing => ResourceManager.GetString("Application_ErrorDeserializing", resourceCulture);

		internal static string Application_ErrorSerializing => ResourceManager.GetString("Application_ErrorSerializing", resourceCulture);

		internal static string AttachProject_Cancelled => ResourceManager.GetString("AttachProject_Cancelled", resourceCulture);

		internal static string AutomaticTask_CannotStartSubTask => ResourceManager.GetString("AutomaticTask_CannotStartSubTask", resourceCulture);

		internal static string AutomaticTaskExecuter_CouldNotLoadType => ResourceManager.GetString("AutomaticTaskExecuter_CouldNotLoadType", resourceCulture);

		internal static string AutomaticTaskExecuter_TypeIsNoIAutomaticTaskImplementation => ResourceManager.GetString("AutomaticTaskExecuter_TypeIsNoIAutomaticTaskImplementation", resourceCulture);

		internal static string AutomaticTaskExecutor_TaskFailed => ResourceManager.GetString("AutomaticTaskExecutor_TaskFailed", resourceCulture);

		internal static string AutomaticTaskExecutor_TaskPartiallyCompleted => ResourceManager.GetString("AutomaticTaskExecutor_TaskPartiallyCompleted", resourceCulture);

		internal static string ExecutingTaskFile_CannotUpdateUntranslatedWordCount => ResourceManager.GetString("ExecutingTaskFile_CannotUpdateUntranslatedWordCount", resourceCulture);

		internal static string ExecutingTaskFile_CannotUpdateWordCounts => ResourceManager.GetString("ExecutingTaskFile_CannotUpdateWordCounts", resourceCulture);

		internal static string ImportProject_LanguagesNotSupported => ResourceManager.GetString("ImportProject_LanguagesNotSupported", resourceCulture);

		internal static string ImportProjectTemplate_Cancelled => ResourceManager.GetString("ImportProjectTemplate_Cancelled", resourceCulture);

		internal static string InvalidLocalDataFolderException_Project_CantIntersect => ResourceManager.GetString("InvalidLocalDataFolderException_Project_CantIntersect", resourceCulture);

		internal static string InvalidLocalDataFolderException_Project_InvalidCharacters => ResourceManager.GetString("InvalidLocalDataFolderException_Project_InvalidCharacters", resourceCulture);

		internal static string InvalidLocalDataFolderException_Project_ShouldBeEmpty => ResourceManager.GetString("InvalidLocalDataFolderException_Project_ShouldBeEmpty", resourceCulture);

		internal static string InvalidLocalDataFolderException_Server_CantIntersect => ResourceManager.GetString("InvalidLocalDataFolderException_Server_CantIntersect", resourceCulture);

		internal static string InvalidLocalDataFolderException_Server_InvalidCharacters => ResourceManager.GetString("InvalidLocalDataFolderException_Server_InvalidCharacters", resourceCulture);

		internal static string InvalidLocalDataFolderException_Server_ShouldBeEmpty => ResourceManager.GetString("InvalidLocalDataFolderException_Server_ShouldBeEmpty", resourceCulture);

		internal static string InvalidProjectNameException_InvalidCharacters => ResourceManager.GetString("InvalidProjectNameException_InvalidCharacters", resourceCulture);

		internal static string OpenServerProject_AlreadyOpen => ResourceManager.GetString("OpenServerProject_AlreadyOpen", resourceCulture);

		internal static string OpenServerProject_CannotCancelAlreadyCancelling => ResourceManager.GetString("OpenServerProject_CannotCancelAlreadyCancelling", resourceCulture);

		internal static string OpenServerProject_CannotCancelNotInProgress => ResourceManager.GetString("OpenServerProject_CannotCancelNotInProgress", resourceCulture);

		internal static string OpenServerProject_CannotExecuteCompleted => ResourceManager.GetString("OpenServerProject_CannotExecuteCompleted", resourceCulture);

		internal static string OpenServerProject_CannotExecuteStillRunning => ResourceManager.GetString("OpenServerProject_CannotExecuteStillRunning", resourceCulture);

		internal static string OpenServerProject_NonEmptyLocaldataFolder => ResourceManager.GetString("OpenServerProject_NonEmptyLocaldataFolder", resourceCulture);

		internal static string OpenServerProjectOperation_NoPermissions => ResourceManager.GetString("OpenServerProjectOperation_NoPermissions", resourceCulture);

		internal static string OpenServerProjectOperation_NotPublished => ResourceManager.GetString("OpenServerProjectOperation_NotPublished", resourceCulture);

		internal static string OpenServerProjectOperation_ProjectDeleted => ResourceManager.GetString("OpenServerProjectOperation_ProjectDeleted", resourceCulture);

		internal static string OpenServerProjectPackageImport_LocalProjectFolderNotEmpty => ResourceManager.GetString("OpenServerProjectPackageImport_LocalProjectFolderNotEmpty", resourceCulture);

		internal static string OpenServerProjectPackageImport_ProjectFileNotFound => ResourceManager.GetString("OpenServerProjectPackageImport_ProjectFileNotFound", resourceCulture);

		internal static string PackageHelper_NoSynergyPackage => ResourceManager.GetString("PackageHelper_NoSynergyPackage", resourceCulture);

		internal static string PackageHelper_WrongPackageVersion => ResourceManager.GetString("PackageHelper_WrongPackageVersion", resourceCulture);

		internal static string PackageImport_CannotGetFilesBeforeImported => ResourceManager.GetString("PackageImport_CannotGetFilesBeforeImported", resourceCulture);

		internal static string PackageImport_LocalServerNotFound => ResourceManager.GetString("PackageImport_LocalServerNotFound", resourceCulture);

		internal static string PackageImport_NoProjectFound => ResourceManager.GetString("PackageImport_NoProjectFound", resourceCulture);

		internal static string PackageImport_OnlyOneProjectSupported => ResourceManager.GetString("PackageImport_OnlyOneProjectSupported", resourceCulture);

		internal static string PackageOperation_PropertyNotAvailable => ResourceManager.GetString("PackageOperation_PropertyNotAvailable", resourceCulture);

		internal static string PackagePreviewInfo_ErrorSerializing => ResourceManager.GetString("PackagePreviewInfo_ErrorSerializing", resourceCulture);

		internal static string PackagePreviewInfo_PackageFileDoesNotExist => ResourceManager.GetString("PackagePreviewInfo_PackageFileDoesNotExist", resourceCulture);

		internal static string PackageProject_ProjectFileNotFound => ResourceManager.GetString("PackageProject_ProjectFileNotFound", resourceCulture);

		internal static string Project_ApplySynchronizationInfo_expects_dueDate_specified_lical_timezone => ResourceManager.GetString("Project_ApplySynchronizationInfo_expects_dueDate_specified_lical_timezone", resourceCulture);

		internal static string Project_CannotCreateReturnPackageBecauseProjectNotImported => ResourceManager.GetString("Project_CannotCreateReturnPackageBecauseProjectNotImported", resourceCulture);

		internal static string Project_CreateMergedTranslatableFile_A_merged_file_should_consist_of_at_least_2_child_files_ => ResourceManager.GetString("Project_CreateMergedTranslatableFile_A_merged_file_should_consist_of_at_least_2_child_files_", resourceCulture);

		internal static string Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_file_role_translatable => ResourceManager.GetString("Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_file_role_translatable", resourceCulture);

		internal static string Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_same_language => ResourceManager.GetString("Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_same_language", resourceCulture);

		internal static string Project_ErrorDeserializing => ResourceManager.GetString("Project_ErrorDeserializing", resourceCulture);

		internal static string Project_ErrorLoading => ResourceManager.GetString("Project_ErrorLoading", resourceCulture);

		internal static string Project_ErrorSerializing => ResourceManager.GetString("Project_ErrorSerializing", resourceCulture);

		internal static string Project_FileTypeInformationNotFound => ResourceManager.GetString("Project_FileTypeInformationNotFound", resourceCulture);

		internal static string Project_ProjectAlreadyExists => ResourceManager.GetString("Project_ProjectAlreadyExists", resourceCulture);

		internal static string ProjectFileMigration_UnableToDetermineProjectFileType => ResourceManager.GetString("ProjectFileMigration_UnableToDetermineProjectFileType", resourceCulture);

		internal static string ProjectPackageConverter_UnableToCreatePackageFileTypesFolder => ResourceManager.GetString("ProjectPackageConverter_UnableToCreatePackageFileTypesFolder", resourceCulture);

		internal static string ProjectPackageCreation_CannotAddSourceFile => ResourceManager.GetString("ProjectPackageCreation_CannotAddSourceFile", resourceCulture);

		internal static string ProjectPackageCreation_CannotFindProjectTm => ResourceManager.GetString("ProjectPackageCreation_CannotFindProjectTm", resourceCulture);

		internal static string ProjectPackageCreation_CouldNotFindTaskTemplate => ResourceManager.GetString("ProjectPackageCreation_CouldNotFindTaskTemplate", resourceCulture);

		internal static string ProjectPackageCreation_TaskFailed => ResourceManager.GetString("ProjectPackageCreation_TaskFailed", resourceCulture);

		internal static string ProjectPackageImport_Cancelled => ResourceManager.GetString("ProjectPackageImport_Cancelled", resourceCulture);

		internal static string ProjectPackageImport_CreatedYourself => ResourceManager.GetString("ProjectPackageImport_CreatedYourself", resourceCulture);

		internal static string ProjectPackageImport_Error => ResourceManager.GetString("ProjectPackageImport_Error", resourceCulture);

		internal static string ProjectPackageImport_FileAlreadyExistsLocally => ResourceManager.GetString("ProjectPackageImport_FileAlreadyExistsLocally", resourceCulture);

		internal static string ProjectPackageImport_FileNotFound => ResourceManager.GetString("ProjectPackageImport_FileNotFound", resourceCulture);

		internal static string ProjectPackageImport_InvalidEntryDefinition => ResourceManager.GetString("ProjectPackageImport_InvalidEntryDefinition", resourceCulture);

		internal static string ProjectPackageImport_MultipleSourceLanguagesNotSupported => ResourceManager.GetString("ProjectPackageImport_MultipleSourceLanguagesNotSupported", resourceCulture);

		internal static string ProjectPackageImport_PackageXmlExtensionNotFound => ResourceManager.GetString("ProjectPackageImport_PackageXmlExtensionNotFound", resourceCulture);

		internal static string ProjectPackageImport_ProjectIsServerProject => ResourceManager.GetString("ProjectPackageImport_ProjectIsServerProject", resourceCulture);

		internal static string ProjectPackageImport_ProjectNotImported => ResourceManager.GetString("ProjectPackageImport_ProjectNotImported", resourceCulture);

		internal static string ProjectPackageImport_WrongFileRole => ResourceManager.GetString("ProjectPackageImport_WrongFileRole", resourceCulture);

		internal static string ProjectsProvider_UserIdInUse => ResourceManager.GetString("ProjectsProvider_UserIdInUse", resourceCulture);

		internal static string ProjectTemplate_ErrorDeserializing => ResourceManager.GetString("ProjectTemplate_ErrorDeserializing", resourceCulture);

		internal static string ProjectTemplate_ErrorSerializing => ResourceManager.GetString("ProjectTemplate_ErrorSerializing", resourceCulture);

		internal static string ProjectTemplate_ProjectTemplateAlreadyExists => ResourceManager.GetString("ProjectTemplate_ProjectTemplateAlreadyExists", resourceCulture);

		internal static string ProjectTrackingHelper_FailedToCheckProjectExists => ResourceManager.GetString("ProjectTrackingHelper_FailedToCheckProjectExists", resourceCulture);

		internal static string ProjectTrackingHelper_FileOwnerUpdateFailed => ResourceManager.GetString("ProjectTrackingHelper_FileOwnerUpdateFailed", resourceCulture);

		internal static string ProjectTrackingHelper_FileStatusUpdateFailed => ResourceManager.GetString("ProjectTrackingHelper_FileStatusUpdateFailed", resourceCulture);

		internal static string ProjectTrackingHelper_InvalidDateTime => ResourceManager.GetString("ProjectTrackingHelper_InvalidDateTime", resourceCulture);

		internal static string ProjectTrackingHelper_SocketException_ConnectionRefused => ResourceManager.GetString("ProjectTrackingHelper_SocketException_ConnectionRefused", resourceCulture);

		internal static string ProjectTrackingHelper_SocketException_HostNotFound => ResourceManager.GetString("ProjectTrackingHelper_SocketException_HostNotFound", resourceCulture);

		internal static string ProjectTrackingService_ErrorDeserializing => ResourceManager.GetString("ProjectTrackingService_ErrorDeserializing", resourceCulture);

		internal static string ProjectTrackingService_FailedToCreateProject => ResourceManager.GetString("ProjectTrackingService_FailedToCreateProject", resourceCulture);

		internal static string ProjectTrackingService_FailedToGetProject => ResourceManager.GetString("ProjectTrackingService_FailedToGetProject", resourceCulture);

		internal static string ProjectTrackingService_FailedToGetTmServer => ResourceManager.GetString("ProjectTrackingService_FailedToGetTmServer", resourceCulture);

		internal static string ProjectTrackingService_LogoutFailed => ResourceManager.GetString("ProjectTrackingService_LogoutFailed", resourceCulture);

		internal static string RemoteServer_CannotPublishProjectFromDifferentServer => ResourceManager.GetString("RemoteServer_CannotPublishProjectFromDifferentServer", resourceCulture);

		internal static string RemoteServer_ImportProject_NotLoggedIn => ResourceManager.GetString("RemoteServer_ImportProject_NotLoggedIn", resourceCulture);

		internal static string RemoteServer_ProjectTrackingService_FailedToConnect => ResourceManager.GetString("RemoteServer_ProjectTrackingService_FailedToConnect", resourceCulture);

		internal static string RemoteServer_ProjectTrackingService_InvalidPassword => ResourceManager.GetString("RemoteServer_ProjectTrackingService_InvalidPassword", resourceCulture);

		internal static string RemoteServer_ProjectTrackingService_InvalidUserName => ResourceManager.GetString("RemoteServer_ProjectTrackingService_InvalidUserName", resourceCulture);

		internal static string RemoteServer_ProjectTrackingService_RequiresLogon => ResourceManager.GetString("RemoteServer_ProjectTrackingService_RequiresLogon", resourceCulture);

		internal static string Report_ErrorCreatingReport => ResourceManager.GetString("Report_ErrorCreatingReport", resourceCulture);

		internal static string Report_ErrorCreatingReportFolder => ResourceManager.GetString("Report_ErrorCreatingReportFolder", resourceCulture);

		internal static string Report_ErrorUpdatingReport => ResourceManager.GetString("Report_ErrorUpdatingReport", resourceCulture);

		internal static string ReturnPackageImport_Cancelled => ResourceManager.GetString("ReturnPackageImport_Cancelled", resourceCulture);

		internal static string ReturnPackageImport_CouldNotFindUpdateTmTaskTemplate => ResourceManager.GetString("ReturnPackageImport_CouldNotFindUpdateTmTaskTemplate", resourceCulture);

		internal static string ReturnPackageImport_CreatedYourself => ResourceManager.GetString("ReturnPackageImport_CreatedYourself", resourceCulture);

		internal static string ReturnPackageImport_ProjectDoesNotExist => ResourceManager.GetString("ReturnPackageImport_ProjectDoesNotExist", resourceCulture);

		internal static string ReturnPackageImport_ServerDoesNotExist => ResourceManager.GetString("ReturnPackageImport_ServerDoesNotExist", resourceCulture);

		internal static string ReturnPackageImport_SourceFileDoesNotExist => ResourceManager.GetString("ReturnPackageImport_SourceFileDoesNotExist", resourceCulture);

		internal static string ReturnPackageImport_TargetFileDoesNotExist => ResourceManager.GetString("ReturnPackageImport_TargetFileDoesNotExist", resourceCulture);

		internal static string ReturnPackageImport_TargetFileIsReferenceFile => ResourceManager.GetString("ReturnPackageImport_TargetFileIsReferenceFile", resourceCulture);

		internal static string Server_CustomerAlreadyExists => ResourceManager.GetString("Server_CustomerAlreadyExists", resourceCulture);

		internal static string Server_CustomerNameEmpty => ResourceManager.GetString("Server_CustomerNameEmpty", resourceCulture);

		internal static string Server_ErrorLoadingXmlFile => ResourceManager.GetString("Server_ErrorLoadingXmlFile", resourceCulture);

		internal static string Server_ErrorSavingXmlFile => ResourceManager.GetString("Server_ErrorSavingXmlFile", resourceCulture);

		internal static string ServerConnectionProfile_DefaultShouldBeInList => ResourceManager.GetString("ServerConnectionProfile_DefaultShouldBeInList", resourceCulture);

		internal static string Task_ParameterAlreadyExists => ResourceManager.GetString("Task_ParameterAlreadyExists", resourceCulture);

		internal static string TaskReportTypeManager_NoReportDefinitionForTaskTemplate => ResourceManager.GetString("TaskReportTypeManager_NoReportDefinitionForTaskTemplate", resourceCulture);

		internal static string TaskReportTypeManager_TaskTempateDoesNotExist => ResourceManager.GetString("TaskReportTypeManager_TaskTempateDoesNotExist", resourceCulture);

		internal static string TaskReportTypeManager_TaskTemplateIsNotAutomatic => ResourceManager.GetString("TaskReportTypeManager_TaskTemplateIsNotAutomatic", resourceCulture);

		internal static string TranslatableFile_CannotGetBilingualReferenceFileForSource => ResourceManager.GetString("TranslatableFile_CannotGetBilingualReferenceFileForSource", resourceCulture);

		internal static string TranslatableFile_CannotGetBilingualReferenceFilesForTarget => ResourceManager.GetString("TranslatableFile_CannotGetBilingualReferenceFilesForTarget", resourceCulture);

		internal static string TranslatableFile_CannotRemoveBilingualReferenceFilesForTarget => ResourceManager.GetString("TranslatableFile_CannotRemoveBilingualReferenceFilesForTarget", resourceCulture);

		internal static string TranslatableFile_CannotSetBilingualReferenceFileForSource => ResourceManager.GetString("TranslatableFile_CannotSetBilingualReferenceFileForSource", resourceCulture);

		internal static string TranslatableFile_OnlyOnePreviousBilingualFilePerTargetLanguage => ResourceManager.GetString("TranslatableFile_OnlyOnePreviousBilingualFilePerTargetLanguage", resourceCulture);

		internal static string TranslationMemory_InvalidSourceLanguage => ResourceManager.GetString("TranslationMemory_InvalidSourceLanguage", resourceCulture);

		internal static string TranslationMemory_InvalidTargetLanguage => ResourceManager.GetString("TranslationMemory_InvalidTargetLanguage", resourceCulture);

		internal static string TranslationMemory_MissingTargetLanguage => ResourceManager.GetString("TranslationMemory_MissingTargetLanguage", resourceCulture);

		internal static string TranslationMemory_PhysicalTranslationMemoryAlreadyHasParent => ResourceManager.GetString("TranslationMemory_PhysicalTranslationMemoryAlreadyHasParent", resourceCulture);

		internal static string TranslationMemoryServer_CannotGetConnectionUri => ResourceManager.GetString("TranslationMemoryServer_CannotGetConnectionUri", resourceCulture);

		internal static string TranslationMemoryServer_CannotGetDefaultCredential => ResourceManager.GetString("TranslationMemoryServer_CannotGetDefaultCredential", resourceCulture);

		internal static string VersionUtil_FileVersionNotFound => ResourceManager.GetString("VersionUtil_FileVersionNotFound", resourceCulture);

		internal static string VersionUtil_GetFileVersionError => ResourceManager.GetString("VersionUtil_GetFileVersionError", resourceCulture);

		internal static string VersionUtil_InvalidApplicationFileVersion => ResourceManager.GetString("VersionUtil_InvalidApplicationFileVersion", resourceCulture);

		internal static string VersionUtil_InvalidOlderProjectFileVersion => ResourceManager.GetString("VersionUtil_InvalidOlderProjectFileVersion", resourceCulture);

		internal static string VersionUtil_InvalidProjectFileVersion => ResourceManager.GetString("VersionUtil_InvalidProjectFileVersion", resourceCulture);

		internal static string VersionUtil_InvalidProjectTemplateFileVersion => ResourceManager.GetString("VersionUtil_InvalidProjectTemplateFileVersion", resourceCulture);

		internal static string VersionUtil_InvalidServerFileVersion => ResourceManager.GetString("VersionUtil_InvalidServerFileVersion", resourceCulture);

		internal static string Workflow_FileStatusAlreadyExists => ResourceManager.GetString("Workflow_FileStatusAlreadyExists", resourceCulture);

		internal static string Worklow_InvalidFileStatusOrdinal => ResourceManager.GetString("Worklow_InvalidFileStatusOrdinal", resourceCulture);

		internal ErrorMessages()
		{
		}
	}
}
