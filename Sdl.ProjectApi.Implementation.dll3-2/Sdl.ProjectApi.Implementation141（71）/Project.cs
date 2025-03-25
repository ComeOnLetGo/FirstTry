using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.Desktop.Platform.Services;
using Sdl.FileTypeSupport.Framework;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectApi.Implementation.Events;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.ProjectSettings;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Settings;
using Sdl.ProjectApi.Settings.SettingTypes;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	[Guid("D6D29EAB-70A0-4150-96AA-FAF845215BB5")]
	public class Project : AbstractProjectConfiguration, IProject, IProjectConfiguration, ISettingsBundleProvider, IObjectWithSettings, INotifyPropertyChanged, IRelativePathManager
	{
		private class ShouldOverwriteLocalizableFileQuestion : IOverwriteFileQuestion
		{
			private readonly IProject _project;

			private readonly ILocalizableFile _localizableTargetFile;

			public ShouldOverwriteLocalizableFileQuestion(IProject project, ILocalizableFile localizableTargetFile)
			{
				_project = project;
				_localizableTargetFile = localizableTargetFile;
			}

			public OverwriteFileEventResult ShouldOverwriteFile()
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				return ((IProjectConfiguration)_project).ProjectsProvider.Application.PackageImportEvents.ShouldOverwriteLocalizableFile(_project, _localizableTargetFile);
			}
		}

		private const string AUTOSUGGEST_DICTIONARIES_FOLDER_NAME = "AutoSuggest Dictionaries";

		private const string REPORTS_FOLDER_NAME = "Reports";

		private const string INPLACE_DIRECTORY_FORMAT = "{0}.ProjectFiles";

		private ProjectListItem _xmlProjectListItem;

		private ISettingsBundle _listItemSettingsBundle;

		private readonly IProjectRepository _projectRepository;

		private List<IScheduledTask> _tasksList;

		private IProjectTaskFileManager _projectTaskFileManager;

		private IProjectTemplate _lazyProjectTemplate;

		private IProject _lazyReferenceProject;

		private List<IProjectPackageCreation> _projectPackageCreations;

		private List<IProjectPackageImport> _projectPackageImports;

		private List<IReturnPackageCreation> _returnPackageCreations;

		private List<IReturnPackageImport> _returnPackageImports;

		private bool _isDeleted;

		private readonly OverwriteFileHelper _overwriteFileHelper = new OverwriteFileHelper();

		private PublishProjectOperation _lazyPublishProjectOperation;

		private readonly IProjectOperation _operation;

		private readonly IEventAggregator _eventAggregator;

		private readonly DirectoryWrapper _directoryWrapper = new DirectoryWrapper();

		protected bool LicenseOverrideRequired;

		public string Name
		{
			get
			{
				lock (GetLockObject())
				{
					return _xmlProjectListItem.ProjectInfo.Name;
				}
			}
		}

		public bool IsProjectDisabled { get; set; }

		public bool IsTaskHandleAndFileDownloadInProgress { get; set; }

		public bool IsInPlace => _xmlProjectListItem.ProjectInfo.IsInPlace;

		public bool IsCloudBased => _xmlProjectListItem.ProjectInfo.IsCloudBased;

		public bool IsLCProject => ProjectOrigin == "LC project";

		public Guid Guid => _xmlProjectListItem.Guid;

		public bool IsImported => _xmlProjectListItem.ProjectInfo.IsImported;

		public bool IsSecure => _xmlProjectListItem.ProjectInfo.IsSecure;

		public string EncryptionKey
		{
			get
			{
				if (_projectRepository is SecureProjectRepository)
				{
					SecureProjectRepository secureProjectRepository = _projectRepository as SecureProjectRepository;
					return secureProjectRepository.EncryptionKey;
				}
				return null;
			}
		}

		public bool Exists
		{
			get
			{
				if (!_isDeleted)
				{
					return File.Exists(ProjectFilePath);
				}
				return false;
			}
		}

		public string ProjectFilePath => base.PathUtil.MakeAbsolutePath(_xmlProjectListItem.ProjectFilePath, base.ProjectsProvider.LocalDataFolder);

		public TranslationManagementSystemInfoSettings TranslationManagementSystemInfoSettings => _listItemSettingsBundle.GetSettingsGroup<TranslationManagementSystemInfoSettings>();

		public string LocalDataFolder
		{
			get
			{
				if (!IsInPlace)
				{
					return Path.GetDirectoryName(ProjectFilePath);
				}
				return GetLocalDataFolder(ProjectFilePath, Name);
			}
			set
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				if (IsInPlace)
				{
					throw new ProjectApiException("Cannot change the local data folder of an in-place project.");
				}
				lock (GetLockObject())
				{
					string fileName = Path.GetFileName(ProjectFilePath);
					string localDataFolder = LocalDataFolder;
					if (!object.Equals(localDataFolder, value))
					{
						if (Util.DoDirectoriesIntersect(localDataFolder, value))
						{
							throw new InvalidLocalDataFolderException(ErrorMessages.InvalidLocalDataFolderException_Project_CantIntersect);
						}
						ValidateProjectLocalDataFolder(value);
						if (Directory.Exists(localDataFolder))
						{
							DirectoryInfo diSource = new DirectoryInfo(localDataFolder);
							DirectoryInfo diDestination = new DirectoryInfo(value);
							Util.CopyDirectory(diSource, diDestination, "*", "*", Overwrite: true, -1);
							Directory.Delete(localDataFolder, recursive: true);
						}
						_xmlProjectListItem.ProjectFilePath = base.PathUtil.MakeRelativePath(Path.Combine(value, fileName), base.ProjectsProvider.LocalDataFolder);
						AdjustProjectTranslationMemoriesUri(localDataFolder, value);
						this.LocalDataFolderChanged?.Invoke(this, EventArgs.Empty);
					}
				}
				OnPropertyChanged("LocalDataFolder");
			}
		}

		public Language SourceLanguage
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				return new Language(ProjectRepository.SourceLanguage);
			}
			set
			{
				if (LanguageBase.Equals(ProjectRepository.SourceLanguage, ((LanguageBase)value).IsoAbbreviation))
				{
					return;
				}
				ProjectRepository.SourceLanguage = ((LanguageBase)value).IsoAbbreviation;
				foreach (LanguageDirection languageDirections in base.LanguageDirectionsList)
				{
					languageDirections.SourceLanguage = value;
				}
				OnPropertyChanged("SourceLanguage");
			}
		}

		public Language[] TargetLanguages => LanguageDirections.Select((ILanguageDirection d) => d.TargetLanguage).ToArray();

		public IProjectTemplate ProjectTemplate
		{
			get
			{
				if (_lazyProjectTemplate == null && ProjectRepository.ProjectTemplateGuid != Guid.Empty)
				{
					_lazyProjectTemplate = base.ProjectsProvider.GetProjectTemplate(ProjectRepository.ProjectTemplateGuid);
				}
				return _lazyProjectTemplate;
			}
			set
			{
				ProjectTemplate projectTemplate = (ProjectTemplate)(object)value;
				if (projectTemplate == null)
				{
					ProjectRepository.ProjectTemplateGuid = Guid.Empty;
					return;
				}
				base.Settings = projectTemplate.Settings;
				base.CascadeItem.Update(projectTemplate.CascadeItem);
				base.LanguageResources = projectTemplate.LanguageResources;
				foreach (LanguageDirection languageDirections in base.LanguageDirectionsList)
				{
					languageDirections.InitializeSettingsFromConfiguration((IProjectConfiguration)(object)projectTemplate, copyTms: true);
					ILanguageDirection languageDirection2 = projectTemplate.GetLanguageDirection(languageDirections.SourceLanguage, languageDirections.TargetLanguage);
					if (languageDirection2 != null)
					{
						CopyAutoSuggestDictionaries(languageDirection2, (ILanguageDirection)(object)languageDirections);
					}
				}
				base.TermbaseConfiguration = ((ICopyable<IProjectTermbaseConfiguration>)(object)projectTemplate.TermbaseConfiguration).Copy();
				if (!IsInPlace)
				{
					((IProjectConfigurationRepository)ProjectRepository).SetInitialComplexTaskTemplate(projectTemplate.StartTaskTemplate);
				}
				if (string.IsNullOrWhiteSpace(ProjectOrigin) || !ProjectOrigin.Equals("LC project"))
				{
					SetAnalysisBands(projectTemplate.AnalysisBands);
				}
				ProjectRepository.ProjectTemplateGuid = projectTemplate.Guid;
				ProjectRepository.ReferenceProjectGuid = Guid.Empty;
				_lazyProjectTemplate = value;
				_lazyReferenceProject = null;
				OnPropertyChanged("ProjectTemplate");
			}
		}

		public IProject ReferenceProject
		{
			get
			{
				if (_lazyReferenceProject == null && ProjectRepository.ReferenceProjectGuid != Guid.Empty)
				{
					_lazyReferenceProject = base.ProjectsProvider.GetProject(ProjectRepository.ReferenceProjectGuid);
				}
				return _lazyReferenceProject;
			}
			set
			{
				if (!(value is Project project))
				{
					ProjectRepository.ReferenceProjectGuid = Guid.Empty;
					return;
				}
				string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				IProjectTemplate val2 = (ProjectTemplate = base.ProjectsProvider.CreateNewProjectTemplate(string.Empty, text, (IProject)(object)project));
				ProjectRepository.ProjectTemplateGuid = Guid.Empty;
				ProjectRepository.ReferenceProjectGuid = project.Guid;
				val2.Delete();
				_lazyProjectTemplate = null;
				_lazyReferenceProject = value;
				OnPropertyChanged("ReferenceProject");
			}
		}

		public ProjectStatus Status => EnumConvert.ConvertProjectStatus(_xmlProjectListItem.ProjectInfo.Status);

		public string Description
		{
			get
			{
				lock (GetLockObject())
				{
					return _xmlProjectListItem.ProjectInfo.Description;
				}
			}
			set
			{
				lock (GetLockObject())
				{
					_xmlProjectListItem.ProjectInfo.Description = value;
				}
				OnPropertyChanged("Description");
			}
		}

		public DateTime DueDate
		{
			get
			{
				lock (GetLockObject())
				{
					return _xmlProjectListItem.ProjectInfo.DueDateSpecified ? _xmlProjectListItem.ProjectInfo.DueDate.ToLocalTime() : DateTime.MaxValue;
				}
			}
			set
			{
				lock (GetLockObject())
				{
					if (value != DateTime.MaxValue)
					{
						_xmlProjectListItem.ProjectInfo.DueDateSpecified = true;
						_xmlProjectListItem.ProjectInfo.DueDate = value.ToUniversalTime();
					}
					else
					{
						_xmlProjectListItem.ProjectInfo.DueDateSpecified = false;
						_xmlProjectListItem.ProjectInfo.DueDate = DateTime.MaxValue;
					}
				}
				OnPropertyChanged("DueDate");
			}
		}

		public DateTime StartedAt
		{
			get
			{
				lock (GetLockObject())
				{
					return _xmlProjectListItem.ProjectInfo.StartedAtSpecified ? _xmlProjectListItem.ProjectInfo.StartedAt.ToLocalTime() : DateTime.MinValue;
				}
			}
			set
			{
				lock (GetLockObject())
				{
					if (value == DateTime.MinValue)
					{
						_xmlProjectListItem.ProjectInfo.StartedAtSpecified = false;
						return;
					}
					_xmlProjectListItem.ProjectInfo.StartedAtSpecified = true;
					_xmlProjectListItem.ProjectInfo.StartedAt = value.ToUniversalTime();
				}
			}
		}

		public ICustomer Customer
		{
			get
			{
				lock (GetLockObject())
				{
					return (ICustomer)(object)((_xmlProjectListItem.ProjectInfo.Customer != null) ? new Customer(_xmlProjectListItem.ProjectInfo.Customer) : null);
				}
			}
		}

		public IUser CurrentUser
		{
			get
			{
				if (!IsPublished)
				{
					return base.ProjectsProvider.UserProvider.CurrentUser;
				}
				return GetUserById(PublishProjectOperation.ServerUserName);
			}
		}

		public IUser CreatedBy
		{
			get
			{
				return GetUserById(_xmlProjectListItem.ProjectInfo.CreatedBy);
			}
			set
			{
				_xmlProjectListItem.ProjectInfo.CreatedBy = value.UserId;
			}
		}

		public DateTime CreatedAt => _xmlProjectListItem.ProjectInfo.CreatedAt.ToLocalTime();

		public DateTime CompletedAt
		{
			get
			{
				if (!_xmlProjectListItem.ProjectInfo.CompletedAtSpecified)
				{
					return DateTime.MaxValue;
				}
				return _xmlProjectListItem.ProjectInfo.CompletedAt.ToLocalTime();
			}
		}

		public DateTime ArchivedAt
		{
			get
			{
				if (!_xmlProjectListItem.ProjectInfo.ArchivedAtSpecified)
				{
					return DateTime.MaxValue;
				}
				return _xmlProjectListItem.ProjectInfo.ArchivedAt.ToLocalTime();
			}
		}

		public string ProjectOrigin
		{
			get
			{
				return base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectOrigin;
			}
			set
			{
				base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectOrigin = value;
			}
		}

		public string IconPath
		{
			get
			{
				return base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectIconPath;
			}
			set
			{
				base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectIconPath = value;
			}
		}

		public string AccountId
		{
			get
			{
				return _listItemSettingsBundle.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().AccountId;
			}
			set
			{
				base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().AccountId = value;
			}
		}

		public string TenantName => _listItemSettingsBundle.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().TenantName;

		public string OfflineUserId
		{
			get
			{
				return base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().OfflineUserId;
			}
			set
			{
				base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().OfflineUserId = value;
			}
		}

		public FolderStructureMigrationStatus FolderStructureMigration
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().FolderStructureMigration;
			}
			set
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().FolderStructureMigration = value;
			}
		}

		public ProjectType ProjectType
		{
			get
			{
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				if (IsCloudBased)
				{
					return (ProjectType)11;
				}
				if (IsLCProject)
				{
					if (!IsImported)
					{
						return (ProjectType)8;
					}
					return (ProjectType)12;
				}
				if (!string.IsNullOrWhiteSpace(ProjectOrigin))
				{
					if (!ProjectOrigin.Equals("Offline cloud package"))
					{
						return (ProjectType)10;
					}
					return (ProjectType)9;
				}
				if (IsInPlace)
				{
					return (ProjectType)1;
				}
				if (PublishProjectOperation != null && (int)PublishProjectOperation.Status != 0)
				{
					return (ProjectType)6;
				}
				if (IsImported)
				{
					if (string.IsNullOrEmpty(PackageConvertor))
					{
						return (ProjectType)2;
					}
					return (ProjectType)(PackageConvertor switch
					{
						"Sdl.TranslationStudio.Packaging.Convertors.Tms.TmsPackageConvertor" => 3, 
						"Sdl.TranslationStudio.Packaging.Convertors.TeamWorks.TeamWorksPackageConvertor" => 4, 
						"Sdl.TranslationStudio.Packaging.Convertors.WorldServer.WsPackageConvertor" => 5, 
						_ => 7, 
					});
				}
				return (ProjectType)0;
			}
		}

		public IWordCountStatistics WordCountStatistics => (IWordCountStatistics)(object)new WordCountStatistics(GetTranslatableFiles(SourceLanguage));

		public IAutomaticTask[] AutomaticTasks => TasksList?.FindAll((IScheduledTask task) => task is IAutomaticTask).ConvertAll((Converter<IScheduledTask, IAutomaticTask>)((IScheduledTask task) => (IAutomaticTask)task)).ToArray();

		public IManualTask[] ManualTasks => TasksList?.FindAll((IScheduledTask task) => task is IManualTask).ConvertAll((Converter<IScheduledTask, IManualTask>)((IScheduledTask task) => (IManualTask)task)).ToArray();

		public IList<IProjectPackageCreation> ProjectPackageCreationOperations
		{
			get
			{
				EnsureLoaded();
				return _projectPackageCreations;
			}
		}

		public IList<IProjectPackageImport> ProjectPackageImportOperations
		{
			get
			{
				EnsureLoaded();
				return _projectPackageImports;
			}
		}

		public IList<IReturnPackageCreation> ReturnPackageCreationOperations
		{
			get
			{
				if (!IsImported)
				{
					throw new InvalidOperationException();
				}
				EnsureLoaded();
				return _returnPackageCreations;
			}
		}

		public IList<IReturnPackageImport> ReturnPackageImportOperations
		{
			get
			{
				EnsureLoaded();
				return _returnPackageImports;
			}
		}

		public string PackageConvertor
		{
			get
			{
				return _xmlProjectListItem.ProjectInfo.PackageConvertor;
			}
			set
			{
				_xmlProjectListItem.ProjectInfo.PackageConvertor = value;
			}
		}

		string IRelativePathManager.BasePath => base.PathUtil.NormalizeFullPath(ProjectFileDirectory);

		public virtual IPublishProjectOperation PublishProjectOperation
		{
			get
			{
				return (IPublishProjectOperation)(object)PublishProjectOperationImpl;
			}
			set
			{
				PublishProjectOperationImpl = value as PublishProjectOperation;
			}
		}

		internal PublishProjectOperation PublishProjectOperationImpl
		{
			get
			{
				return _lazyPublishProjectOperation ?? (_lazyPublishProjectOperation = new PublishProjectOperation((IProject)(object)this, base.PathUtil));
			}
			set
			{
				_lazyPublishProjectOperation = value;
			}
		}

		internal DateTime? LastSynchronizationTimestamp
		{
			get
			{
				ProjectSyncSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectSyncSettings>();
				return settingsGroup.LastSynchronizationTimestamp;
			}
			set
			{
				ProjectSyncSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectSyncSettings>();
				settingsGroup.LastSynchronizationTimestamp = value;
			}
		}

		public bool IsPublished
		{
			get
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Invalid comparison between Unknown and I4
				if (PublishProjectOperation != null)
				{
					return (int)PublishProjectOperation.Status == 4;
				}
				return false;
			}
		}

		public bool IsServerAvailable
		{
			get
			{
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Invalid comparison between Unknown and I4
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Invalid comparison between Unknown and I4
				if (!IsPublished)
				{
					return false;
				}
				string absoluteUri = PublishProjectOperation.UnqualifiedServerUri.AbsoluteUri;
				if (!IdentityInfoCache.Default.ContainsKey(absoluteUri))
				{
					return false;
				}
				if (!PublishProjectOperation.OriginalServerUserName.Equals(PublishProjectOperation.ServerUserName, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				ConnectionInfo connectionInfo = IdentityInfoCache.Default.GetConnectionInfo(absoluteUri);
				if ((int)connectionInfo.ConnectionStatus == 1)
				{
					return (int)connectionInfo.AuthenticationStatus == 2;
				}
				return false;
			}
		}

		internal bool IsLoaded => _projectRepository.IsInitialized;

		internal virtual IProjectRepository ProjectRepository
		{
			get
			{
				EnsureLoaded();
				return _projectRepository;
			}
		}

		public override IProjectConfigurationRepository Repository
		{
			get
			{
				EnsureLoaded();
				return (IProjectConfigurationRepository)_projectRepository;
			}
		}

		internal string ProjectFileDirectory => Path.GetDirectoryName(ProjectFilePath);

		internal List<IScheduledTask> TasksList
		{
			get
			{
				EnsureLoaded();
				return _tasksList;
			}
		}

		public IProjectTaskFileManager ProjectTaskFileManager
		{
			get
			{
				EnsureLoaded();
				return _projectTaskFileManager;
			}
		}

		internal List<ComplexTask> ComplexTasksList { get; private set; }

		public string ServerUserName => PublishProjectOperation.ServerUserName;

		public MigrationData MigrationData
		{
			get
			{
				return base.Settings.GetSettingsGroup<MigrationDataSettings>().MigrationData;
			}
			set
			{
				base.Settings.GetSettingsGroup<MigrationDataSettings>().MigrationData = value;
			}
		}

		public event EventHandler<LanguageChangeEventArgs> SourceLanguageChanged;

		public event EventHandler LocalDataFolderChanged;

		internal Project(IProjectsProvider projectsProvider, ProjectListItem xmlProjectListItem, IProjectRepository repository, IProjectOperation operation, IEventAggregator eventAggregator)
			: base(projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			_xmlProjectListItem = xmlProjectListItem;
			_listItemSettingsBundle = LoadProjectListItemSettings(_xmlProjectListItem);
		}

		internal Project(IProjectsProvider projectsProvider, string projectFilePath, Language sourceLanguage, Language[] targetLanguages, IProjectTemplate projectTemplate, IProjectRepository repository, IProjectOperation operation, IEventAggregator eventAggregator)
			: base(projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			InitProject(projectFilePath, sourceLanguage, targetLanguages);
			UpdateGeneralProjectInfoSettings((IObjectWithSettings)(object)projectTemplate);
			ProjectTemplate = projectTemplate;
		}

		internal Project(IProjectsProvider projectsProvider, string projectFilePath, Language sourceLanguage, Language[] targetLanguages, IProject referenceProject, IProjectRepository repository, IUser currentUser, IProjectOperation operation, IEventAggregator eventAggregator)
			: base(projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			((IProjectConfigurationRepository)_projectRepository).AddUser(currentUser);
			InitProject(projectFilePath, sourceLanguage, targetLanguages);
			InitializeProjectTemplate(referenceProject.ProjectTemplate, currentUser);
			UpdateGeneralProjectInfoSettings((IObjectWithSettings)(object)referenceProject);
			ReferenceProject = referenceProject;
		}

		internal Project(IProjectsProvider projectsProvider, string projectFilePath, Language sourceLanguage, Language[] targetLanguages, IProjectRepository repository, IProjectOperation operation, IEventAggregator eventAggregator)
			: base(projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			InitProject(projectFilePath, sourceLanguage, targetLanguages);
		}

		internal Project(IProjectsProvider projectsProvider, string projectFilePath, bool licenseOverrideRequired, IProjectRepository repository, IProjectOperation operation, IEventAggregator eventAggregator)
			: base(projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			LicenseOverrideRequired = licenseOverrideRequired;
			Load(projectFilePath, createProjectListItem: true);
		}

		internal Project(ProjectsProvider projectsProvider, string projectFilePath, IPackageProject packageProject, IProjectRepository repository, IProjectOperation operation, IEventAggregator eventAggregator)
			: base((IProjectsProvider)(object)projectsProvider, projectsProvider.PathUtil)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			_operation = operation;
			_eventAggregator = eventAggregator;
			_projectRepository = repository;
			AddUserToCache(((IProject)packageProject).CreatedBy);
			CreateProjectListItem(base.PathUtil.MakeRelativePath(projectFilePath, base.ProjectsProvider.LocalDataFolder));
			CreateProjectSettingsBundle();
			CreateEmptyIndexes();
		}

		private void InitProject(string projectFilePath, Language sourceLanguage, Language[] targetLanguages)
		{
			CreateProjectSettingsBundle();
			foreach (Language targetLanguage in targetLanguages)
			{
				AddLanguageDirection(sourceLanguage, targetLanguage);
			}
			CreateProjectListItem(base.PathUtil.MakeRelativePath(projectFilePath, base.ProjectsProvider.LocalDataFolder));
			CreateEmptyIndexes();
		}

		private void InitializeProjectTemplate(IProjectTemplate projectTemplate, IUser currentUser)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if (projectTemplate is ProjectTemplate projectTemplate2)
			{
				ProjectTemplateRepository repository = new ProjectTemplateRepository(base.ProjectsProvider.Application, base.PathUtil);
				ProjectTemplate = (IProjectTemplate)(object)new ProjectTemplate(projectTemplate2.ProjectsProvider, projectTemplate2.Description, projectTemplate2.ProjectTemplateFilePath, base.PathUtil, (IProjectTemplateRepository)(object)repository, currentUser, (IDirectory)new DirectoryWrapper());
			}
		}

		private void CreateProjectSettingsBundle()
		{
			ISettingsBundle val = SettingsUtil.CreateSettingsBundle((ISettingsBundle)null);
			base.SettingsBundlesList.AddSettingsBundle(Repository.SettingsBundleGuid, val);
		}

		private void CreateEmptyIndexes()
		{
			_projectPackageCreations = new List<IProjectPackageCreation>();
			_projectPackageImports = new List<IProjectPackageImport>();
			_returnPackageCreations = new List<IReturnPackageCreation>();
			_returnPackageImports = new List<IReturnPackageImport>();
			_tasksList = new List<IScheduledTask>();
			ComplexTasksList = new List<ComplexTask>();
			_projectTaskFileManager = (IProjectTaskFileManager)(object)new ProjectTaskFileManager();
		}

		private void UpdateGeneralProjectInfoSettings(IObjectWithSettings settings)
		{
			if (settings is IProjectTemplate)
			{
				GeneralProjectInfoSettings generalProjectInfo = settings.Settings.GetSettingsGroup<GeneralProjectInfoSettings>();
				DueDate = generalProjectInfo.DueDate?.Value ?? DateTime.MaxValue;
				ICustomer value = base.ProjectsProvider.CustomerProvider.Customers?.FirstOrDefault((ICustomer a) => string.Compare(a.Name, generalProjectInfo.CustomerId?.Value, StringComparison.InvariantCultureIgnoreCase) == 0);
				ChangeCustomer(value);
				return;
			}
			IProject projectReference = (IProject)(object)((settings is IProject) ? settings : null);
			if (projectReference != null)
			{
				DueDate = projectReference.DueDate;
				ICustomer value2 = base.ProjectsProvider.CustomerProvider.Customers?.FirstOrDefault(delegate(ICustomer a)
				{
					string name = a.Name;
					ICustomer customer = projectReference.Customer;
					return string.Compare(name, (customer != null) ? customer.Name : null, StringComparison.InvariantCultureIgnoreCase) == 0;
				});
				ChangeCustomer(value2);
			}
		}

		protected virtual object GetLockObject()
		{
			return base.ProjectsProvider.SyncRoot;
		}

		public void UpdateStatus(ProjectStatus updatedProjectStatus)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			lock (GetLockObject())
			{
				_xmlProjectListItem.ProjectInfo.Status = EnumConvert.ConvertProjectStatus(updatedProjectStatus);
			}
		}

		public void ChangeProjectName(string value)
		{
			if (Name != value)
			{
				lock (GetLockObject())
				{
					string text = base.PathUtil.MakeAbsolutePath(_xmlProjectListItem.ProjectFilePath, base.ProjectsProvider.LocalDataFolder);
					string localDataFolder = LocalDataFolder;
					Sdl.ProjectApi.Implementation.ProjectsProvider.ValidateProjectName(value);
					string text2 = Path.Combine(Path.GetDirectoryName(text), Sdl.ProjectApi.Implementation.ProjectsProvider.GetProjectFileName(value));
					if (File.Exists(text) && !object.Equals(text, text2))
					{
						File.Move(text, text2);
					}
					_xmlProjectListItem.ProjectInfo.Name = value;
					_xmlProjectListItem.ProjectFilePath = base.PathUtil.MakeRelativePath(text2, base.ProjectsProvider.LocalDataFolder);
					ProjectRepository.ProjectInfo.Name = value;
					if (IsInPlace && Directory.Exists(localDataFolder) && !object.Equals(localDataFolder, LocalDataFolder))
					{
						Directory.Move(localDataFolder, LocalDataFolder);
					}
				}
			}
			OnPropertyChanged("Name");
		}

		public T GetSettingsGroup<T>() where T : ISettingsGroup, new()
		{
			if (IsLoaded)
			{
				return base.Settings.GetSettingsGroup<T>();
			}
			return _listItemSettingsBundle.GetSettingsGroup<T>();
		}

		public void SaveAs(string newProjectFilePath, string oldTranslatedFilePath = null)
		{
			if (string.IsNullOrEmpty(newProjectFilePath))
			{
				throw new ArgumentNullException("newProjectFilePath");
			}
			if (!IsInPlace)
			{
				throw new NotSupportedException("This method is not supported for standard projects.");
			}
			lock (GetLockObject())
			{
				if (!object.Equals(ProjectFilePath, newProjectFilePath))
				{
					string fileName = Path.GetFileName(newProjectFilePath);
					string text = fileName.Substring(0, fileName.Length - FileTypes.ProjectFileExtension.Length);
					string localDataFolder = GetLocalDataFolder(newProjectFilePath, text);
					bool flag = !string.Equals(text, Name, StringComparison.InvariantCulture);
					if (Directory.Exists(LocalDataFolder))
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(LocalDataFolder);
						DirectoryInfo diDestination = new DirectoryInfo(localDataFolder);
						Util.CopyDirectory(directoryInfo, diDestination, "*", "*", Overwrite: true, -1);
						if (!File.Exists(ProjectFilePath))
						{
							directoryInfo.Delete(recursive: true);
						}
					}
					if (File.Exists(ProjectFilePath))
					{
						Util.CopyFile(ProjectFilePath, newProjectFilePath);
					}
					IProjectFile[] projectFiles = GetProjectFiles(SourceLanguage);
					IProjectFile[] array = projectFiles;
					for (int i = 0; i < array.Length; i++)
					{
						LanguageFile languageFile = (LanguageFile)(object)array[i];
						if (languageFile?.LocalFilePath != oldTranslatedFilePath)
						{
							languageFile?.OnInPlaceProjectFilePathChanged(newProjectFilePath, base.PathUtil);
						}
					}
					if (flag)
					{
						_xmlProjectListItem.ProjectInfo.Name = text;
						ProjectRepository.ProjectInfo.Name = text;
						OnPropertyChanged("Name");
					}
					_xmlProjectListItem.ProjectFilePath = base.PathUtil.MakeRelativePath(newProjectFilePath, base.ProjectsProvider.LocalDataFolder);
					this.LocalDataFolderChanged?.Invoke(this, EventArgs.Empty);
				}
				Save();
			}
		}

		private string GetLocalDataFolder(string projectFilePath, string projectName)
		{
			string pathValue = Path.Combine(Path.GetDirectoryName(projectFilePath), $"{projectName}.ProjectFiles");
			Uri uri = new UriBuilder("file", null, 0, pathValue).Uri;
			return uri.LocalPath;
		}

		public ILanguageDirection GetLanguageDirection(Language targetLanguage)
		{
			return base.LanguageDirectionsList.Cast<ILanguageDirection>().FirstOrDefault((ILanguageDirection ld) => ((object)ld.TargetLanguage).Equals((object)targetLanguage));
		}

		protected override void OnTargetLanguageChanged(object sender, LanguageDirectionChangeEventArgs e)
		{
			ProjectRepository.UpdateSourceLanguageForFiles(((LanguageBase)e.OldLanguage).IsoAbbreviation, ((LanguageBase)e.LanguageDirection.TargetLanguage).IsoAbbreviation);
			base.OnTargetLanguageChanged(sender, e);
		}

		public void ChangeCustomer(ICustomer value)
		{
			lock (GetLockObject())
			{
				if (value != null)
				{
					ICustomer val = base.ProjectsProvider.CustomerProvider.GetCustomer(value.Guid);
					if (val == null)
					{
						val = base.ProjectsProvider.CustomerProvider.GetCustomerByName(value.Name);
						if (val == null)
						{
							val = base.ProjectsProvider.CustomerProvider.AddCustomer(value.Guid, value.Name, value.Email);
						}
						else
						{
							val.Email = value.Email;
						}
					}
					_xmlProjectListItem.ProjectInfo.Customer = new Sdl.ProjectApi.Implementation.Xml.Customer
					{
						Guid = val.Guid,
						Name = val.Name,
						Email = val.Email
					};
				}
				else
				{
					_xmlProjectListItem.ProjectInfo.Customer = null;
				}
			}
			OnPropertyChanged("Customer");
		}

		internal void ValidateProjectLocalDataFolder(string projectLocalDataFolder)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (projectLocalDataFolder.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				throw new InvalidLocalDataFolderException(string.Format(ErrorMessages.InvalidLocalDataFolderException_Project_InvalidCharacters, new string(Path.GetInvalidPathChars())));
			}
			if (!object.Equals(LocalDataFolder, projectLocalDataFolder) && _directoryWrapper.DirectoryContainsFiles(projectLocalDataFolder))
			{
				throw new InvalidLocalDataFolderException(ErrorMessages.InvalidLocalDataFolderException_Project_ShouldBeEmpty);
			}
		}

		public override void Check()
		{
			EnsureLoaded();
		}

		public override void Save()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				lock (GetLockObject())
				{
					Directory.CreateDirectory(Path.GetDirectoryName(ProjectFilePath));
					OnBeforeSerialization();
					UpdateProjectListItemSettings(_listItemSettingsBundle);
					ProjectRepository.Save(ProjectFilePath);
				}
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Project_ErrorSerializing, _xmlProjectListItem.ProjectInfo.Name, ProjectFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		public override void DiscardCachedData()
		{
			if (File.Exists(ProjectFilePath))
			{
				OnBeforeCachedDataDiscarded();
				base.DiscardCachedData();
				((IProjectConfigurationRepository)ProjectRepository).DiscardData();
				_tasksList = null;
				ComplexTasksList = null;
				_projectTaskFileManager = null;
				_lazyProjectTemplate = null;
				_lazyReferenceProject = null;
				_projectPackageCreations = null;
				_projectPackageImports = null;
				_returnPackageCreations = null;
				_returnPackageImports = null;
			}
		}

		public IAutomaticTask Start()
		{
			ProjectRepository.MarkProjectAsStarted();
			if (base.StartTaskTemplate != null)
			{
				IAutomaticTask result = (IAutomaticTask)(object)CreateNewAutomaticTask((ITaskTemplate)(object)base.StartTaskTemplate, GetProjectFiles(SourceLanguage), isStartProjectTask: true);
				OnPropertyChanged("StartedAt");
				return result;
			}
			return null;
		}

		public void Complete()
		{
			MarkAsCompleted(DateTime.UtcNow);
		}

		internal void MarkAsCompleted(DateTime completedAt)
		{
			lock (GetLockObject())
			{
				_xmlProjectListItem.ProjectInfo.Status = ProjectStatus.Completed;
				_xmlProjectListItem.ProjectInfo.CompletedAt = completedAt;
				_xmlProjectListItem.ProjectInfo.CompletedAtSpecified = true;
			}
			OnPropertyChanged("CompletedAt");
		}

		public void Reactivate()
		{
			lock (GetLockObject())
			{
				_xmlProjectListItem.ProjectInfo.Status = ProjectStatus.Started;
				_xmlProjectListItem.ProjectInfo.CompletedAt = DateTime.UtcNow;
				_xmlProjectListItem.ProjectInfo.CompletedAtSpecified = false;
			}
		}

		public void Archive()
		{
			base.ProjectsProvider.RemoveProject((IProject)(object)this);
			lock (GetLockObject())
			{
				_xmlProjectListItem.ProjectInfo.Status = ProjectStatus.Archived;
				_xmlProjectListItem.ProjectInfo.ArchivedAt = DateTime.UtcNow;
				_xmlProjectListItem.ProjectInfo.ArchivedAtSpecified = true;
			}
			OnPropertyChanged("ArchivedAt");
		}

		private bool IsWhiteSpaceFolder()
		{
			string[] source = ProjectFileDirectory.Split('\\');
			return string.IsNullOrWhiteSpace(source.Last());
		}

		public void Delete()
		{
			base.ProjectsProvider.RemoveProject((IProject)(object)this);
			if (!IsWhiteSpaceFolder() && Directory.Exists(LocalDataFolder))
			{
				try
				{
					Directory.Delete(LocalDataFolder, recursive: true);
				}
				catch (IOException ex)
				{
					ILogger val = LogProvider.GetLoggerFactory().CreateLogger(GetType().FullName);
					LoggerExtensions.LogError(val, (Exception)ex, $"Failed to delete directory {LocalDataFolder}.", Array.Empty<object>());
					ITelemetryService val2 = default(ITelemetryService);
					if (GlobalServices.Context.TryGetService<ITelemetryService>(ref val2))
					{
						val2.TrackException((Exception)ex);
					}
				}
			}
			if (IsInPlace && File.Exists(ProjectFilePath))
			{
				File.Delete(ProjectFilePath);
			}
			_isDeleted = true;
		}

		public ITranslatableFile[] GetTranslatableFiles(Language language)
		{
			List<IProjectFile> projectFiles = ProjectRepository.GetProjectFiles((IProject)(object)this, language);
			return (from f in projectFiles
				where (int)f.FileRole == 1 || (int)f.FileRole == 0
				select (ITranslatableFile)(object)((f is ITranslatableFile) ? f : null)).ToArray();
		}

		public ITranslatableFile[] GetTranslatableFilesForTask(ITaskTemplate template)
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			List<ITranslatableFile> list = (from f in ProjectRepository.GetProjectFiles((IProject)(object)this)
				where (int)f.FileRole == 1 || (int)f.FileRole == 0
				select (ITranslatableFile)(object)((f is ITranslatableFile) ? f : null)).ToList();
			List<ITranslatableFile> list2 = new List<ITranslatableFile>();
			foreach (ITranslatableFile item in list)
			{
				if (template.SupportsFileType(item.TaskFileType) && (!((ILanguageFile)item).IsSource || item.TargetLanguageFiles.Length == 0 || StartsWithSplitTask(template)))
				{
					ITaskTemplateFileFilter val = (ITaskTemplateFileFilter)(object)((template is ITaskTemplateFileFilter) ? template : null);
					if (val == null)
					{
						list2.Add(item);
					}
					else if (val.SupportsFile(item))
					{
						list2.Add(item);
					}
				}
			}
			return list2.ToArray();
		}

		private bool StartsWithSplitTask(ITaskTemplate template)
		{
			IComplexTaskTemplate val = (IComplexTaskTemplate)(object)((template is IComplexTaskTemplate) ? template : null);
			if (val != null)
			{
				template = val.SubTaskTemplates[0];
			}
			return template.Id == "Sdl.ProjectApi.AutomaticTasks.Split";
		}

		public ITranslatableFile[] GetTranslatableFiles()
		{
			return ProjectRepository.GetTranslatableTargetOrSingleDocumentFiles((IProject)(object)this).ToArray();
		}

		public IProjectFile[] GetAllProjectFiles()
		{
			return ProjectRepository.GetProjectFiles((IProject)(object)this, allFiles: true).ToArray();
		}

		public IProjectFile[] GetProjectFiles(Predicate<IProjectFile> match)
		{
			List<IProjectFile> projectFiles = ProjectRepository.GetProjectFiles((IProject)(object)this);
			return projectFiles.Where((IProjectFile f) => match(f)).ToArray();
		}

		public ILocalizableFile[] GetLocalizableFiles(Language language)
		{
			List<IProjectFile> projectFiles = ProjectRepository.GetProjectFiles((IProject)(object)this, language);
			return (from f in projectFiles
				where (int)f.FileRole == 3 || (int)f.FileRole == 1
				select (ILocalizableFile)(object)((f is ILocalizableFile) ? f : null)).ToArray();
		}

		public IReferenceFile[] GetReferenceFiles(Language language)
		{
			List<IProjectFile> source = ProjectRepository.GetProjectFiles((IProject)(object)this, language).ToList();
			return (from f in source
				where (int)f.FileRole == 2
				select (IReferenceFile)(object)((f is IReferenceFile) ? f : null)).ToArray();
		}

		public IReferenceFile[] GetReferenceFiles()
		{
			List<IProjectFile> source = ProjectRepository.GetProjectFiles((IProject)(object)this, allFiles: true).ToList();
			return (from f in source
				where (int)f.FileRole == 2
				select (IReferenceFile)(object)((f is IReferenceFile) ? f : null)).ToArray();
		}

		public IProjectFile[] GetProjectFiles(Language language)
		{
			return ProjectRepository.GetProjectFiles((IProject)(object)this, language).ToArray();
		}

		public List<IProjectFile> GetPagedProjectFiles(Language language, int pageSize, int skip)
		{
			return ProjectRepository.GetPagedProjectFiles((IProject)(object)this, language, pageSize, skip);
		}

		public ILanguageFile GetSourceLanguageFile(ILanguageFile targetLanguageFile)
		{
			return ProjectRepository.GetSourceLanguageFile((IProject)(object)this, targetLanguageFile);
		}

		public ITranslatableFile GetTranslatableFile(string filenameInProject, Language language)
		{
			string fileName = Path.GetFileName(filenameInProject);
			string folderName = base.PathUtil.NormalizeFolder(Path.GetDirectoryName(filenameInProject));
			return ProjectRepository.GetTranslatableFile((IProject)(object)this, fileName, folderName, language);
		}

		public IProjectFile GetProjectFile(Guid projectFileGuid)
		{
			return ProjectRepository.GetProjectFile((IProject)(object)this, projectFileGuid);
		}

		internal IMergedTranslatableFile GetMergedTranslatableFile(Guid projectFileGuid)
		{
			return ProjectRepository.GetMergedTranslatableFile((IProject)(object)this, projectFileGuid);
		}

		internal List<IMergedTranslatableFile> GetMergedTranslatableFileHistory(Guid projectFileGuid)
		{
			return ProjectRepository.GetMergedTranslatableFileHistory((IProject)(object)this, projectFileGuid);
		}

		public ITaskFile GetTaskFile(Guid taskFileGuid)
		{
			return ProjectTaskFileManager.GetTaskFile(taskFileGuid);
		}

		public IProjectFile GetProjectFile(string filenameInProject, Language language)
		{
			string fileName = Path.GetFileName(filenameInProject);
			string folderName = base.PathUtil.NormalizeFolder(Path.GetDirectoryName(filenameInProject));
			return ProjectRepository.GetProjectFile((IProject)(object)this, fileName, folderName, language);
		}

		internal ITranslatableFile GetTranslatableFile(Guid languageFileGuid)
		{
			return ProjectRepository.GetTranslatableFile((IProject)(object)this, languageFileGuid);
		}

		public ITranslatableFile AddTranslatableFile(string fileToAdd, string folderInProject)
		{
			IProjectFile obj = ProjectRepository.AddProjectFile((IProject)(object)this, fileToAdd, folderInProject, SourceLanguage, (FileRole)0);
			return (ITranslatableFile)(object)((obj is ITranslatableFile) ? obj : null);
		}

		public ITranslatableFile AddTranslatableFile(string fileToAdd, string folderInProject, Language language)
		{
			IProjectFile obj = ProjectRepository.AddProjectFile((IProject)(object)this, fileToAdd, folderInProject, language, (FileRole)0);
			return (ITranslatableFile)(object)((obj is ITranslatableFile) ? obj : null);
		}

		public IMergedTranslatableFile CreateMergedTranslatableFile(string mergedFileName, string folderInProject, ITranslatableFile[] childFiles)
		{
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Invalid comparison between Unknown and I4
			if (string.IsNullOrEmpty(mergedFileName))
			{
				throw new ArgumentNullException("mergedFileName");
			}
			if (folderInProject == null)
			{
				throw new ArgumentNullException("folderInProject");
			}
			if (childFiles == null)
			{
				throw new ArgumentNullException("childFiles");
			}
			if (childFiles.Length < 2)
			{
				throw new ArgumentException(ErrorMessages.Project_CreateMergedTranslatableFile_A_merged_file_should_consist_of_at_least_2_child_files_, "childFiles");
			}
			Language val = null;
			foreach (ITranslatableFile val2 in childFiles)
			{
				if (val == null)
				{
					val = ((IProjectFile)val2).Language;
				}
				else if (!((object)val).Equals((object)((IProjectFile)val2).Language))
				{
					throw new ArgumentException(ErrorMessages.Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_same_language, "childFiles");
				}
				if ((int)((IProjectFile)val2).FileRole != 1)
				{
					throw new ArgumentException(ErrorMessages.Project_CreateMergedTranslatableFile_A_merged_file_should_have_the_file_role_translatable, "childFiles");
				}
			}
			folderInProject = base.PathUtil.NormalizeFolder(folderInProject);
			IProjectRepository projectRepository = ProjectRepository;
			string folderInProject2 = folderInProject;
			Language language = val;
			FileTypeDefinitionId fileTypeDefinitionId = base.FileTypeConfiguration.FilterManager.DefaultBilingualFileTypeDefinition.FileTypeInformation.FileTypeDefinitionId;
			return projectRepository.CreateMergedTranslatableFile((IProject)(object)this, mergedFileName, folderInProject2, language, ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id, childFiles);
		}

		public ILocalizableFile AddLocalizableNotTranslatableFile(string fileToAdd, string folderInProject)
		{
			IProjectFile obj = ProjectRepository.AddProjectFile((IProject)(object)this, fileToAdd, folderInProject, SourceLanguage, (FileRole)3);
			return (ILocalizableFile)(object)((obj is ILocalizableFile) ? obj : null);
		}

		public IBilingualReferenceFile AddBilingualReferenceFile(string fileToAdd, ITranslatableFile translatableFile, Language targetLanguage, bool isReviewed)
		{
			return (IBilingualReferenceFile)(object)((TranslatableFile)(object)translatableFile).AddPreviousBilingualFile(fileToAdd, isReviewed, targetLanguage);
		}

		public IReferenceFile AddReferenceFile(string fileToAdd, string folderInProject)
		{
			IProjectFile obj = ProjectRepository.AddProjectFile((IProject)(object)this, fileToAdd, folderInProject, SourceLanguage, (FileRole)2);
			return (IReferenceFile)(object)((obj is IReferenceFile) ? obj : null);
		}

		public void DeleteFilesAndDependencies(string fileNameInProject)
		{
			string fileName = Path.GetFileName(fileNameInProject);
			string text = base.PathUtil.NormalizeFolder(Path.GetDirectoryName(fileNameInProject));
			List<IProjectFile> projectFiles = ProjectRepository.GetProjectFiles((IProject)(object)this);
			foreach (IProjectFile item in projectFiles)
			{
				if (item.Filename == fileName && item.PathInProject == text)
				{
					item.RemoveFromProject(true);
					break;
				}
			}
		}

		public IProjectFile[] SplitIntoTargetLanguage(IProjectFile[] sourceLanguageFiles, Language targetLanguage)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			foreach (IProjectFile sourceLanguageFile in sourceLanguageFiles)
			{
				if (SplitIntoTargetLanguage(sourceLanguageFile, targetLanguage, out var targetLanguageFile))
				{
					list.Add(targetLanguageFile);
				}
			}
			return list.ToArray();
		}

		public IProjectFile[] SplitIntoTargetLanguages(IProjectFile[] sourceLanguageFiles)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			Language[] targetLanguages = TargetLanguages;
			Language[] array = targetLanguages;
			foreach (Language targetLanguage in array)
			{
				list.AddRange(SplitIntoTargetLanguage(sourceLanguageFiles, targetLanguage));
			}
			return list.ToArray();
		}

		public IProjectFile[] SplitIntoTargetLanguages(IProjectFile[] sourceLanguageFiles, SplitIntoTargetLanguagesProgressHandler progressHandler)
		{
			if (progressHandler != null)
			{
				progressHandler.Invoke(0);
			}
			IProjectFile[] array = (IProjectFile[])sourceLanguageFiles.Clone();
			Array.Sort(array, new FileRoleComparer());
			if (progressHandler != null)
			{
				progressHandler.Invoke(0);
			}
			List<IProjectFile> list = new List<IProjectFile>();
			int num = 0;
			IProjectFile[] array2 = array;
			foreach (IProjectFile val in array2)
			{
				num++;
				if (progressHandler != null)
				{
					progressHandler.Invoke(num);
				}
				Language[] array3 = null;
				ILocalizableFile val2 = (ILocalizableFile)(object)((val is ILocalizableFile) ? val : null);
				if (val2 != null)
				{
					array3 = ((ILanguageFile)val2).SpecificTargetLanguages;
				}
				if (array3 == null)
				{
					array3 = TargetLanguages;
				}
				if (progressHandler != null)
				{
					progressHandler.Invoke(num);
				}
				Language[] array4 = array3;
				foreach (Language targetLanguage in array4)
				{
					if (progressHandler != null)
					{
						progressHandler.Invoke(num);
					}
					if (SplitIntoTargetLanguage(val, targetLanguage, out var targetLanguageFile))
					{
						list.Add(targetLanguageFile);
					}
				}
			}
			return list.ToArray();
		}

		public override void SetAnalysisBands(int[] minimumMatchValues)
		{
			base.SetAnalysisBands(minimumMatchValues);
			if (!IsLCProject)
			{
				ILanguageDirection[] languageDirections = LanguageDirections;
				for (int i = 0; i < languageDirections.Length; i++)
				{
					LanguageDirection languageDirection = (LanguageDirection)(object)languageDirections[i];
					languageDirection.NotifyAnalysisStatisticsChanged();
				}
				ProjectRepository.ResetAnalysisStatics((IProject)(object)this);
			}
		}

		public IAnalysisStatistics GetAnalysisStatistics(ILanguageDirection languageDirection)
		{
			return (IAnalysisStatistics)(object)((LanguageDirection)(object)languageDirection).AnalysisStatistics;
		}

		public IConfirmationStatistics GetConfirmationStatistics(ILanguageDirection languageDirection)
		{
			return (IConfirmationStatistics)(object)((LanguageDirection)(object)languageDirection).ConfirmationStatistics;
		}

		internal void NotifyWordCountStatisticsChanged()
		{
			ProjectRepository.ResetWordCountStatistics();
		}

		internal void NotifyAnalysisStatisticsChanged(ILanguageDirection languageDirection)
		{
			NotifyWordCountStatisticsChanged();
			((LanguageDirection)(object)languageDirection).NotifyAnalysisStatisticsChanged();
		}

		public IScheduledTask GetTask(TaskId taskId)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Guid[] array = ((TaskId)(ref taskId)).ToGuidArray();
			if (array.Length == 1)
			{
				return (IScheduledTask)(object)GetTask<ScheduledTask>(array[0]);
			}
			return (IScheduledTask)(object)ComplexTasksList.FirstOrDefault(delegate(ComplexTask complexTask)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				TaskId id = complexTask.Id;
				return ((object)(TaskId)(ref id)).Equals((object)taskId);
			});
		}

		public IManualCollaborativeTask CreateNewManualTask(IManualTaskTemplate template)
		{
			return CreateNewManualTask(template, (IProjectFile[])(object)new IProjectFile[0]);
		}

		public IManualCollaborativeTask CreateNewManualTask(IManualTaskTemplate template, IProjectFile[] files)
		{
			return CreateNewManualTask(template, files, Guid.NewGuid(), ((ITaskTemplate)template).Name, ((ITaskTemplate)template).Description, DateTime.MaxValue, CurrentUser, DateTime.Now, null, null, null);
		}

		protected virtual IManualCollaborativeTask CreateNewManualTask(IManualTaskTemplate template, IProjectFile[] files, Guid taskId, string taskName, string taskDescription, DateTime dueDate, IUser createdBy, DateTime createdAt, IUser assignedBy, IUser assignedTo, string comment)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Invalid comparison between Unknown and I4
			IManualCollaborativeTask val = ProjectRepository.CreateNewManualTask((IProject)(object)this, template, taskId, taskName, taskDescription, dueDate, createdBy, createdAt);
			foreach (IProjectFile val2 in files)
			{
				((ICollaborativeTask<IManualTaskFile>)(object)val).AddFile(val2, (FilePurpose)(((int)val2.FileRole != 2) ? 1 : 2));
			}
			if (assignedTo != null)
			{
				val.Assign(assignedBy, assignedTo, comment);
			}
			TasksList.Add((IScheduledTask)(object)val);
			base.ProjectsProviderImp.AddOrUpdateManualTaskIndexEntry((IManualTask)(object)val);
			return val;
		}

		public void RemoveManualTask(IManualCollaborativeTask manualTask)
		{
			if (manualTask == null)
			{
				throw new ArgumentNullException("manualTask");
			}
			if (manualTask is ManualTask)
			{
				base.ProjectsProviderImp.RemoveManualTaskIndexEntry((ManualTask)(object)manualTask);
				ProjectRepository.RemoveTask((IScheduledTask)(object)(ManualTask)(object)manualTask);
			}
			if (manualTask is ScheduledTask)
			{
				TasksList.Remove((IScheduledTask)(object)(ScheduledTask)(object)manualTask);
			}
			IManualTaskFile[] files = ((IManualTask)manualTask).Files;
			foreach (IManualTaskFile val in files)
			{
				((ICollaborativeTask<IManualTaskFile>)(object)manualTask).RemoveFile(val);
			}
		}

		public IAutomaticCollaborativeTask CreateNewAutomaticTask(ITaskTemplate template)
		{
			return CreateNewAutomaticTask(template, (IProjectFile[])(object)new IProjectFile[0]);
		}

		public IAutomaticCollaborativeTask CreateNewAutomaticTask(ITaskTemplate template, IProjectFile[] files)
		{
			return CreateNewAutomaticTask(template, files, isStartProjectTask: false);
		}

		public void AddAutomaticTasks(IEnumerable<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasks)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask in xmlAutomaticTasks)
			{
				AutomaticTask item = ProjectRepository.AddAutomaticTasks((IProject)(object)this, xmlAutomaticTask);
				TasksList.Add((IScheduledTask)(object)item);
				ComplexTask item2 = new ComplexTask((IProject)(object)this, new List<AutomaticTask> { item });
				ComplexTasksList.Add(item2);
			}
		}

		internal IAutomaticCollaborativeTask CreateNewAutomaticTask(ITaskTemplate template, IProjectFile[] files, bool isStartProjectTask)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Invalid comparison between Unknown and I4
			if (template is IComplexTaskTemplate)
			{
				return (IAutomaticCollaborativeTask)(object)CreateComplexTask(template, files, isStartProjectTask);
			}
			AutomaticTask automaticTask = ProjectRepository.AddAutomaticTasks((IProject)(object)this, CurrentUser, template.Id);
			automaticTask.ForceProcessing = isStartProjectTask;
			foreach (IProjectFile item in files.OrderBy((IProjectFile f) => f.LocalFilePath))
			{
				automaticTask.AddFile(item, (FilePurpose)(((int)item.FileRole != 2) ? 1 : 2));
			}
			TasksList.Add((IScheduledTask)(object)automaticTask);
			return (IAutomaticCollaborativeTask)(object)automaticTask;
		}

		private ComplexTask CreateComplexTask(ITaskTemplate template, IProjectFile[] files, bool isStartProjectTask)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Invalid comparison between Unknown and I4
			IComplexTaskTemplate val = (IComplexTaskTemplate)template;
			Guid previousTaskGuid = Guid.Empty;
			ITaskTemplate[][] array = GroupTaskTemplates(val.SubTaskTemplates);
			List<AutomaticTask> list = new List<AutomaticTask>();
			ITaskTemplate[][] array2 = array;
			foreach (ITaskTemplate[] subTaskTemplates in array2)
			{
				Guid guid = Guid.NewGuid();
				AutomaticTask item = ProjectRepository.AddAutomaticTasks((IProject)(object)this, guid, previousTaskGuid, subTaskTemplates, CurrentUser, ((ITaskTemplate)val).Id);
				previousTaskGuid = guid;
				TasksList.Add((IScheduledTask)(object)item);
				list.Add(item);
			}
			ComplexTask complexTask = new ComplexTask((IProject)(object)this, list)
			{
				ForceProcessing = isStartProjectTask
			};
			foreach (IProjectFile item2 in files.OrderBy((IProjectFile f) => f.LocalFilePath))
			{
				complexTask.AddFile(item2, (FilePurpose)(((int)item2.FileRole != 2) ? 1 : 2));
			}
			ComplexTasksList.Add(complexTask);
			return complexTask;
		}

		private ITaskTemplate[][] GroupTaskTemplates(IEnumerable<ITaskTemplate> templates)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			List<ITaskTemplate[]> list = new List<ITaskTemplate[]>();
			List<ITaskTemplate> list2 = new List<ITaskTemplate>(1);
			foreach (ITaskTemplate template in templates)
			{
				IAutomaticTaskTemplate val = (IAutomaticTaskTemplate)(object)((template is IAutomaticTaskTemplate) ? template : null);
				if (val != null)
				{
					if ((int)val.TaskType == 0)
					{
						if (list2.Count > 0)
						{
							list.Add(list2.ToArray());
							list2 = new List<ITaskTemplate>(1);
						}
						list.Add((ITaskTemplate[])(object)new ITaskTemplate[1] { (ITaskTemplate)val });
					}
					else
					{
						if (list2.Count > 0 && list2[list2.Count - 1] is IAutomaticTaskTemplate && ((IAutomaticTaskTemplate)list2[list2.Count - 1]).AllowMultiThreading != val.AllowMultiThreading)
						{
							list.Add(list2.ToArray());
							list2 = new List<ITaskTemplate>(1);
						}
						list2.Add((ITaskTemplate)(object)val);
					}
				}
				else
				{
					if (list2.Count > 0)
					{
						list.Add(list2.ToArray());
						list2 = new List<ITaskTemplate>(1);
					}
					list.Add((ITaskTemplate[])(object)new ITaskTemplate[1] { template });
				}
			}
			if (list2.Count > 0)
			{
				list.Add(list2.ToArray());
			}
			return list.ToArray();
		}

		protected virtual void EnsureLoaded()
		{
			lock (GetLockObject())
			{
				if (!_projectRepository.IsInitialized)
				{
					Load();
				}
			}
		}

		string IRelativePathManager.MakeRelativePath(string tmFilePath)
		{
			return base.PathUtil.MakeRelativePath((IProject)(object)this, tmFilePath, false);
		}

		string IRelativePathManager.MakeAbsolutePath(string tmFilePath)
		{
			return base.PathUtil.MakeAbsolutePath((IProject)(object)this, tmFilePath, false);
		}

		public IManualTask AddManualTask(IManualTask task, IPackageOperationMessageReporter messageReporter)
		{
			ITaskFile[] files = (ITaskFile[])(object)task.Files;
			return AddManualTask(task, files, messageReporter);
		}

		public IManualTask AddManualTask(IManualTask task, ITaskFile[] taskFiles, IPackageOperationMessageReporter messageReporter)
		{
			return AddManualTask(task, taskFiles, task.AssignedTo, messageReporter);
		}

		public virtual IManualTask AddManualTask(IManualTask task, ITaskFile[] taskFiles, IUser assignTo, IPackageOperationMessageReporter messageReporter)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Invalid comparison between Unknown and I4
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			if ((int)((IScheduledTask)task).Status != 2 && (int)((IScheduledTask)task).Status != 9)
			{
				throw new InvalidOperationException(StringResources.AddManualTask_AssignedAndCompletedCheck);
			}
			TaskId id = ((ITaskBase)task).Id;
			Guid guid = ((TaskId)(ref id)).ToGuidArray()[0];
			List<IProjectFile> files = new List<IProjectFile>();
			IProject project = ((ITaskFile)task.Files[0]).ProjectFile.Project;
			AddManualTaskFiles(taskFiles, messageReporter, files);
			CopyTasksHistory(task, project, files);
			ManualTask task2 = GetTask<ManualTask>(guid);
			if (task2 == null)
			{
				IManualTaskTemplate template = (IManualTaskTemplate)((ITaskBase)task).TaskTemplates[0];
				id = ((ITaskBase)task).Id;
				ManualTask manualTask = (ManualTask)(object)AddManualTaskHistory(template, ((TaskId)(ref id)).ToGuidArray()[0], ((ITaskBase)task).Name, ((ITaskBase)task).Description, ((IScheduledTask)task).ExternalId, ((ITaskBase)task).CreatedAt, ((ITaskBase)task).CreatedBy, task.DueDate, ((IScheduledTask)task).StartedAt, ((IScheduledTask)task).CompletedAt, ((IScheduledTask)task).PercentComplete, ((IScheduledTask)task).Status, task.AssignedBy, assignTo, ((IScheduledTask)task).Comment);
				CopyTaskFiles(files, (IScheduledTask)(object)task, manualTask);
				return (IManualTask)(object)manualTask;
			}
			UpdateManualTask(task, task2);
			foreach (ITaskFile val in taskFiles)
			{
				AddOrUpdateTaskFile(task2, val.ProjectFile, val);
			}
			return (IManualTask)(object)task2;
		}

		private void AddManualTaskFiles(ITaskFile[] taskFiles, IPackageOperationMessageReporter messageReporter, List<IProjectFile> files)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Invalid comparison between Unknown and I4
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Invalid comparison between Unknown and I4
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			for (int i = 0; i < taskFiles.Length; i++)
			{
				ITaskFile val = taskFiles[i];
				messageReporter.ReportCurrentOperation(string.Format(StringResources.AddTranslatableFile_Status, i + 1, taskFiles.Length));
				if ((int)val.ProjectFile.FileRole == 1)
				{
					ITranslatableFile val2 = (ITranslatableFile)val.ProjectFile;
					ISettingsGroup settingsGroup = ((IObjectWithSettings)val2).Settings.GetSettingsGroup("FileCustomAttributeSettings");
					ITranslatableFile val3 = AddTranslatableFile(val2, messageReporter);
					ISettingsGroup settingsGroup2 = ((IObjectWithSettings)val3).Settings.GetSettingsGroup("FileCustomAttributeSettings");
					settingsGroup2.ImportSettings(settingsGroup);
					files.Add((IProjectFile)(object)val3);
				}
				else if ((int)val.ProjectFile.FileRole == 3)
				{
					ILocalizableFile item = AddLocalizableFile((ILocalizableFile)val.ProjectFile, messageReporter);
					files.Add((IProjectFile)(object)item);
				}
				else if ((int)val.ProjectFile.FileRole == 2)
				{
					IReferenceFile item2 = AddReferenceFile((IReferenceFile)val.ProjectFile, messageReporter);
					files.Add((IProjectFile)(object)item2);
				}
			}
		}

		private void AddOrUpdateTaskFile(ScheduledTask existingTask, IProjectFile projectFile, ITaskFile taskFile)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			TaskFile taskFile2 = (TaskFile)(object)existingTask.Files.SingleOrDefault((ITaskFile pkgFile) => pkgFile.ProjectFile.Guid == taskFile.ProjectFile.Guid);
			if (taskFile2 == null)
			{
				ITaskFile parentTaskFile = null;
				ITaskFile parentTaskFile2 = taskFile.ParentTaskFile;
				if (parentTaskFile2 != null)
				{
					parentTaskFile = GetTaskFile(parentTaskFile2.Id);
				}
				taskFile2 = (TaskFile)(object)existingTask.AddFile(projectFile, taskFile.Purpose, parentTaskFile, taskFile.Id);
			}
			if (((ITaskEntity)taskFile).IsComplete)
			{
				taskFile2.SetCompleted();
			}
		}

		private void CopyTasksHistory(IManualTask orginalManualTask, IProject project, List<IProjectFile> files)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			IPackageProject val = (IPackageProject)(object)((project is IPackageProject) ? project : null);
			if (val != null && (int)val.PackageType == 0)
			{
				CopyAutomaticTaskHistory(orginalManualTask, project.AutomaticTasks, files);
			}
			CopyManualTaskHistory(orginalManualTask, files);
		}

		private void CopyManualTaskHistory(IManualTask orginalManualTask, List<IProjectFile> files)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			TaskId id = ((ITaskBase)orginalManualTask).Id;
			ManualTask manualTask = GetManualTask(((TaskId)(ref id)).ToGuidArray()[0]);
			if (manualTask == null)
			{
				IManualTaskTemplate template = (IManualTaskTemplate)((ITaskBase)orginalManualTask).TaskTemplates[0];
				id = ((ITaskBase)orginalManualTask).Id;
				manualTask = (ManualTask)(object)AddManualTaskHistory(template, ((TaskId)(ref id)).ToGuidArray()[0], ((ITaskBase)orginalManualTask).Name, ((ITaskBase)orginalManualTask).Description, ((IScheduledTask)orginalManualTask).ExternalId, ((ITaskBase)orginalManualTask).CreatedAt, ((ITaskBase)orginalManualTask).CreatedBy, orginalManualTask.DueDate, ((IScheduledTask)orginalManualTask).StartedAt, ((IScheduledTask)orginalManualTask).CompletedAt, ((IScheduledTask)orginalManualTask).PercentComplete, ((IScheduledTask)orginalManualTask).Status, orginalManualTask.AssignedBy, orginalManualTask.AssignedTo, ((IScheduledTask)orginalManualTask).Comment);
				CopyTaskFiles(files, (IScheduledTask)(object)orginalManualTask, manualTask);
			}
			else
			{
				CopyTaskFiles(files, (IScheduledTask)(object)orginalManualTask, manualTask);
				UpdateManualTask(orginalManualTask, manualTask);
			}
		}

		private void UpdateManualTask(IManualTask sourceTask, ManualTask targetTask)
		{
			targetTask.UpdateFromManualTask(sourceTask);
			if (sourceTask.AssignedBy != null)
			{
				AddUserToCache(sourceTask.AssignedBy);
			}
			if (sourceTask.AssignedTo != null)
			{
				AddUserToCache(sourceTask.AssignedTo);
			}
		}

		private void CopyAutomaticTaskHistory(IManualTask orginalManualTask, IEnumerable<IAutomaticTask> automaticTasks, List<IProjectFile> files)
		{
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Invalid comparison between Unknown and I4
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			IEnumerable<IAutomaticTask> enumerable = from t in (from task in automaticTasks
					from file in ((IScheduledTask)task).Files
					join pf in files on file.ProjectFile.Guid equals pf.Guid
					join orgFile in orginalManualTask.Files on pf.Guid equals ((ITaskFile)orgFile).ProjectFile.Guid
					where (int)((ITaskFile)orgFile).Purpose == 1
					select task).Distinct()
				select (t);
			foreach (IAutomaticTask item in enumerable)
			{
				TaskId id = ((ITaskBase)item).Id;
				AutomaticTask automaticTask = GetAutomaticTask(((TaskId)(ref id)).ToGuidArray()[0]);
				if (automaticTask == null)
				{
					automaticTask = AddAutomaticTaskHistory(item);
					CopyTaskFiles(files, (IScheduledTask)(object)item, automaticTask);
					automaticTask.Status = ((IScheduledTask)item).Status;
					continue;
				}
				automaticTask.Status = (TaskStatus)1;
				CopyTaskFiles(files, (IScheduledTask)(object)item, automaticTask);
				automaticTask.Status = ((IScheduledTask)item).Status;
				if ((int)((IScheduledTask)item).Status == 9)
				{
					automaticTask.XmlTask.CompletedAtSpecified = true;
					automaticTask.XmlTask.CompletedAt = ((IScheduledTask)item).CompletedAt.ToUniversalTime();
				}
				automaticTask.XmlTask.PercentComplete = ((IScheduledTask)item).PercentComplete;
			}
		}

		private void CopyTaskFiles(IEnumerable<IProjectFile> files, IScheduledTask fromTask, ScheduledTask toTask)
		{
			var enumerable = from taskFile in fromTask.Files
				join pf in files on taskFile.ProjectFile.Guid equals pf.Guid
				select new { taskFile, pf };
			foreach (var item in enumerable)
			{
				AddOrUpdateTaskFile(toTask, item.pf, item.taskFile);
			}
		}

		private IManualCollaborativeTask AddManualTaskHistory(IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, string externalId, DateTime createdAt, IUser createdBy, DateTime dueDate, DateTime startedAt, DateTime completedAt, int percentComplete, TaskStatus taskStatus, IUser assignedBy, IUser assignedTo, string comment)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			IManualCollaborativeTask val = ProjectRepository.CreateNewManualTask((IProject)(object)this, template, taskId, taskName, taskDescription, externalId, startedAt, dueDate, createdBy, createdAt, completedAt, percentComplete);
			if (assignedTo != null)
			{
				val.Assign(assignedBy, assignedTo, comment);
			}
			((IManualTask)val).SetStatus(taskStatus);
			TasksList.Add((IScheduledTask)(object)val);
			base.ProjectsProviderImp.AddOrUpdateManualTaskIndexEntry((IManualTask)(object)val);
			return val;
		}

		private AutomaticTask AddAutomaticTaskHistory(IAutomaticTask task)
		{
			AutomaticTask automaticTask = ProjectRepository.AddAutomaticTasks((IProject)(object)this, task);
			TasksList.Add((IScheduledTask)(object)automaticTask);
			return automaticTask;
		}

		private void CopyStatistics(ITranslatableFile originalTransFile, ITranslatableFile transFile)
		{
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Invalid comparison between Unknown and I4
			transFile.AnalysisStatistics.Exact.Assign(originalTransFile.AnalysisStatistics.Exact);
			transFile.AnalysisStatistics.InContextExact.Assign(originalTransFile.AnalysisStatistics.InContextExact);
			transFile.AnalysisStatistics.New.Assign(originalTransFile.AnalysisStatistics.New);
			transFile.AnalysisStatistics.Perfect.Assign(originalTransFile.AnalysisStatistics.Perfect);
			((IWordCountStatistics)transFile.AnalysisStatistics).Repetitions.Assign(((IWordCountStatistics)originalTransFile.AnalysisStatistics).Repetitions);
			((IWordCountStatistics)transFile.AnalysisStatistics).Total.Assign(((IWordCountStatistics)originalTransFile.AnalysisStatistics).Total);
			transFile.AnalysisStatistics.Locked.Assign(originalTransFile.AnalysisStatistics.Locked);
			IProject project = ((IProjectFile)originalTransFile).Project;
			IProject obj = ((project is IPackageProject) ? project : null);
			if (obj == null || (int)((IPackageProject)obj).PackageType != 1)
			{
				for (int i = 0; i < originalTransFile.AnalysisStatistics.Fuzzy.Length; i++)
				{
					((ICountData)transFile.AnalysisStatistics.Fuzzy[i]).Assign((ICountData)(object)originalTransFile.AnalysisStatistics.Fuzzy[i]);
				}
			}
			transFile.AnalysisStatistics.UpdateAll();
			if (!((ILanguageFile)transFile).IsSource)
			{
				transFile.ConfirmationStatistics[(ConfirmationLevel)0].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)0]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)1].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)1]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)2].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)2]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)3].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)3]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)4].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)4]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)5].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)5]);
				transFile.ConfirmationStatistics[(ConfirmationLevel)6].Assign(originalTransFile.ConfirmationStatistics[(ConfirmationLevel)6]);
				transFile.ConfirmationStatistics.Update();
			}
		}

		public void AddLanguageDirection(ILanguageDirection languageDirection, bool copyMainTms, bool copyProjectTms, bool copyAutoSuggestDictionaries, bool removeMtProviders)
		{
			if (GetLanguageDirection(languageDirection.SourceLanguage, languageDirection.TargetLanguage) == null)
			{
				LanguageDirection languageDirection2 = (LanguageDirection)(object)AddLanguageDirection(languageDirection.SourceLanguage, languageDirection.TargetLanguage);
				languageDirection2.InitializeSettingsFromConfiguration(languageDirection.Configuration, copyTms: false);
				CopyTranslationProviderCascade(languageDirection.CascadeItem, languageDirection2.CascadeItem, copyMainTms, copyProjectTms, removeMtProviders, (ILanguageDirection)(object)languageDirection2);
				CopyAutoSuggestDictionaries(languageDirection, (ILanguageDirection)(object)languageDirection2, copyAutoSuggestDictionaries);
			}
		}

		private void AdjustProjectTranslationMemoriesUri(string oldLocalDataFolder, string newLocalDataFolder)
		{
			ILanguageDirection[] languageDirections = LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				AdjustProjectTMUriForCascadeEntry(oldLocalDataFolder, newLocalDataFolder, val.CascadeItem.CascadeEntryItems);
			}
			AdjustProjectTMUriForCascadeEntry(oldLocalDataFolder, newLocalDataFolder, base.CascadeItem.CascadeEntryItems);
		}

		private static void AdjustProjectTMUriForCascadeEntry(string oldLocalDataFolder, string newLocalDataFolder, IList<ProjectCascadeEntryItem> cascadeEntries)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			foreach (ProjectCascadeEntryItem cascadeEntry in cascadeEntries)
			{
				foreach (ITranslationProviderItem projectTranslationProviderItem in cascadeEntry.ProjectTranslationProviderItems)
				{
					if (projectTranslationProviderItem.IsFileBasedTranslationMemory())
					{
						TranslationProviderUriBuilder val = new TranslationProviderUriBuilder(projectTranslationProviderItem.Uri);
						string text = val.Resource.Replace(oldLocalDataFolder, string.Empty);
						string path = (text.StartsWith("\\") ? text.Substring(1) : text);
						string text2 = Path.Combine(newLocalDataFolder, path);
						projectTranslationProviderItem.Uri = FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(text2);
					}
				}
			}
		}

		public void CopyTranslationProviderCascade(ProjectCascadeItem fromCascade, ProjectCascadeItem toCascade, bool copyMainTms, bool copyProjectTms, bool removeMtProviders, ILanguageDirection toLanguageDirection)
		{
			toCascade.StopSearchingWhenResultsFound = fromCascade.StopSearchingWhenResultsFound;
			toCascade.OverrideParent = fromCascade.OverrideParent;
			if (!fromCascade.OverrideParent)
			{
				return;
			}
			foreach (ProjectCascadeEntryItem cascadeEntryItem in fromCascade.CascadeEntryItems)
			{
				CopyTranslationProviderCascadeEntry(cascadeEntryItem, toCascade, copyMainTms, copyProjectTms, toLanguageDirection);
			}
		}

		internal ProjectCascadeEntryItem CopyTranslationProviderCascadeEntry(ProjectCascadeEntryItem fromEntry, ProjectCascadeItem toCascade, bool copyMainTm, bool copyProjectTm, ILanguageDirection toLanguageDirection)
		{
			ProjectCascadeEntryItem val = fromEntry.Copy();
			toCascade.CascadeEntryItems.Add(val);
			TranslationMemoryInfoFactory translationMemoryInfoFactory = new TranslationMemoryInfoFactory();
			ITranslationMemoryInfo translationMemoryInfo = translationMemoryInfoFactory.Create(val.MainTranslationProviderItem.Uri);
			CopyableTranslationProviderItem copyableTranslationProviderItem = new CopyableTranslationProviderItem(val.MainTranslationProviderItem, (IProject)(object)this, translationMemoryInfo, toLanguageDirection);
			if (copyableTranslationProviderItem.CanBeCopied)
			{
				if (copyMainTm)
				{
					copyableTranslationProviderItem.CopyTranslationProvider();
				}
				else
				{
					val.MainTranslationProviderItem.Enabled = false;
				}
			}
			for (int num = val.ProjectTranslationProviderItems.Count - 1; num >= 0; num--)
			{
				ITranslationMemoryInfo translationMemoryInfo2 = translationMemoryInfoFactory.Create(val.ProjectTranslationProviderItems[num].Uri);
				CopyableTranslationProviderItem copyableTranslationProviderItem2 = new CopyableTranslationProviderItem(val.ProjectTranslationProviderItems[num], (IProject)(object)this, translationMemoryInfo2, toLanguageDirection);
				if (copyableTranslationProviderItem2.CanBeCopied)
				{
					if (copyProjectTm)
					{
						copyableTranslationProviderItem2.CopyTranslationProvider();
					}
					else
					{
						val.ProjectTranslationProviderItems.RemoveAt(num);
					}
				}
			}
			return val;
		}

		internal void CopyAutoSuggestDictionaries(ILanguageDirection fromLanguageDirection, ILanguageDirection toLanguageDirection, bool copyAutoSuggestDictionaries)
		{
			((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Clear();
			if (!copyAutoSuggestDictionaries)
			{
				return;
			}
			foreach (IAutoSuggestDictionary item2 in (IEnumerable<IAutoSuggestDictionary>)fromLanguageDirection.AutoSuggestDictionaries)
			{
				string filePath = item2.FilePath;
				string fileName = Path.GetFileName(filePath);
				string path = Path.Combine(ProjectFileDirectory, "AutoSuggest Dictionaries");
				string text = Path.Combine(path, fileName);
				Util.CopyFile(filePath, text);
				IAutoSuggestDictionary item = (IAutoSuggestDictionary)(object)new AutoSuggestDictionary(text);
				((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Add(item);
			}
		}

		public ITranslatableFile AddTranslatableFile(ITranslatableFile translatableFile, IPackageOperationMessageReporter messageReporter)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			if (((ILanguageFile)translatableFile).IsSource)
			{
				return AddTranslatableFile(translatableFile, null, messageReporter);
			}
			IProjectFile val = null;
			if (translatableFile.SourceLanguageFile != null)
			{
				val = GetProjectFile(((IProjectFile)translatableFile.SourceLanguageFile).Guid);
				if (val == null)
				{
					val = (IProjectFile)(object)AddTranslatableFile(translatableFile.SourceLanguageFile, messageReporter);
				}
			}
			return AddTranslatableFile(translatableFile, (ITranslatableFile)val, messageReporter);
		}

		public ITranslatableFile AddTranslatableFile(ITranslatableFile translatableFile, ITranslatableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			IProjectFile projectFile = GetProjectFile(((IProjectFile)translatableFile).Guid);
			ITranslatableFile val;
			bool flag;
			bool flag2;
			if (projectFile == null)
			{
				val = ProjectRepository.AddTranslatableFile((IProject)(object)this, translatableFile, referenceFile, messageReporter);
				flag = true;
				flag2 = true;
			}
			else
			{
				val = (ITranslatableFile)(object)(projectFile as TranslatableFile);
				if (val == null)
				{
					throw new ArgumentException(StringResources.AddTranslatableFile_InvalidOriginalFile);
				}
				flag = TryUploadNewVersion(((IProjectFile)translatableFile).LocalFilePath, (ILocalizableFile)(object)val, messageReporter);
				flag2 = false;
			}
			if (flag)
			{
				CopyStatistics(translatableFile, val);
			}
			if (flag2)
			{
				((IObjectWithSettings)val).Settings = ((IObjectWithSettings)translatableFile).Settings;
				((AbstractSettingsGroupBase)((IObjectWithSettings)val).Settings.GetSettingsGroup<LanguageFileServerStateSettings>()).Reset();
			}
			return val;
		}

		public ITranslatableFile AddTranslatableFile(string fileToAdd, string folderInProject, Language language, Guid id, string filterDefinitionId, ITranslatableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			IProjectFile projectFile = GetProjectFile(id);
			ITranslatableFile val;
			if (projectFile == null)
			{
				val = ProjectRepository.AddTranslatableFile((IProject)(object)this, fileToAdd, folderInProject, language, id, filterDefinitionId, referenceFile);
			}
			else
			{
				val = (ITranslatableFile)(object)((projectFile is ITranslatableFile) ? projectFile : null);
				if (val == null)
				{
					throw new ArgumentException("Failed to update translatable file, because the original file is not translatable.");
				}
				TryUploadNewVersion(fileToAdd, (ILocalizableFile)(object)val, messageReporter);
			}
			return val;
		}

		private bool TryUploadNewVersion(string fileToUpload, ILocalizableFile toLocalizableFile, IPackageOperationMessageReporter messageReporter)
		{
			if (File.GetLastWriteTimeUtc(((IProjectFile)toLocalizableFile).LocalFilePath) <= File.GetLastWriteTimeUtc(fileToUpload))
			{
				((ILanguageFile)toLocalizableFile).UploadNewVersion(fileToUpload, string.Empty);
				return true;
			}
			if (_overwriteFileHelper.ShouldOverwriteFile(new ShouldOverwriteLocalizableFileQuestion((IProject)(object)this, toLocalizableFile)))
			{
				((ILanguageFile)toLocalizableFile).UploadNewVersion(fileToUpload, string.Empty);
				if (messageReporter != null)
				{
					messageReporter.ReportMessage(StringResources.PackageImport_Source, string.Format(StringResources.FileOverwritten, Path.GetFileName(((IProjectFile)toLocalizableFile).LocalFilePath)), (MessageLevel)0, (IProjectFile)(object)toLocalizableFile);
				}
				return true;
			}
			if (messageReporter != null)
			{
				messageReporter.ReportMessage(StringResources.PackageImport_Source, string.Format(StringResources.FileNotOverwritten, Path.GetFileName(((IProjectFile)toLocalizableFile).LocalFilePath)), (MessageLevel)0, (IProjectFile)(object)toLocalizableFile);
			}
			return false;
		}

		public ILocalizableFile AddLocalizableFile(ILocalizableFile localizableFile, IPackageOperationMessageReporter messageReporter)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			if (((ILanguageFile)localizableFile).IsSource)
			{
				return AddLocalizableFile(localizableFile, null, messageReporter);
			}
			IProjectFile val = null;
			if (((ILanguageFile)localizableFile).SourceLanguageFile != null)
			{
				val = GetProjectFile(((IProjectFile)((ILanguageFile)localizableFile).SourceLanguageFile).Guid);
				if (val == null)
				{
					val = (IProjectFile)(object)AddLocalizableFile((ILocalizableFile)((ILanguageFile)localizableFile).SourceLanguageFile, messageReporter);
				}
			}
			return AddLocalizableFile(localizableFile, (ILocalizableFile)val, messageReporter);
		}

		private ILocalizableFile AddLocalizableFile(ILocalizableFile localizableFile, ILocalizableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			IProjectFile projectFile = GetProjectFile(((IProjectFile)localizableFile).Guid);
			ILocalizableFile val;
			if (projectFile == null)
			{
				val = ProjectRepository.AddLocalizableFile((IProject)(object)this, localizableFile, referenceFile);
			}
			else
			{
				val = (ILocalizableFile)(object)((projectFile is ILocalizableFile) ? projectFile : null);
				if (val == null && messageReporter != null)
				{
					messageReporter.ReportMessage(StringResources.PackageImport_Source, "Failed to update localizable file, because the original file is not localizable.", (MessageLevel)2, (IProjectFile)(object)localizableFile);
				}
				TryUploadNewVersion(((IProjectFile)localizableFile).LocalFilePath, val, messageReporter);
			}
			return val;
		}

		public IReferenceFile AddReferenceFile(IReferenceFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			if (((ILanguageFile)referenceFile).IsSource)
			{
				return AddReferenceFile(referenceFile, (IReferenceFile)null);
			}
			IProjectFile val = null;
			if (((ILanguageFile)referenceFile).SourceLanguageFile != null)
			{
				val = GetProjectFile(((IProjectFile)((ILanguageFile)referenceFile).SourceLanguageFile).Guid);
				if (val == null)
				{
					val = (IProjectFile)(object)AddReferenceFile((IReferenceFile)((ILanguageFile)referenceFile).SourceLanguageFile, messageReporter);
				}
			}
			return this.AddReferenceFile(referenceFile, (IReferenceFile)val);
		}

		public IReferenceFile AddReferenceFile(IReferenceFile referenceFile, IReferenceFile parentReferenceFile)
		{
			return ProjectRepository.AddReferenceFile((IProject)(object)this, referenceFile, parentReferenceFile);
		}

		internal ICommuteClient CreateCommuteClient()
		{
			if (!IsPublished)
			{
				throw new InvalidOperationException("You can only create a commute client for published projects.");
			}
			return base.ProjectsProvider.Application.CommuteClientManager.CreateCommuteClient(PublishProjectOperation.ServerUri);
		}

		public ILocalizableFile AddLocalizableFile(string fileToAdd, string folderInProject, Language language, Guid id, ILocalizableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			IProjectFile projectFile = GetProjectFile(id);
			ILocalizableFile val;
			if (projectFile == null)
			{
				val = ProjectRepository.AddLocalizableFile((IProject)(object)this, fileToAdd, folderInProject, language, id, referenceFile);
			}
			else
			{
				val = (ILocalizableFile)(object)((projectFile is ILocalizableFile) ? projectFile : null);
				if (val == null)
				{
					throw new ArgumentException("Failed to update localizable file, because the original file is not localizable.");
				}
				TryUploadNewVersion(fileToAdd, val, messageReporter);
			}
			return val;
		}

		public IUser[] GetProjectUsers()
		{
			return ProjectRepository.GetUsers().ToArray();
		}

		public void RefreshGuids()
		{
			lock (GetLockObject())
			{
				ManifestIdsReplacer manifestIdsReplacer = new ManifestIdsReplacer(Guid.NewGuid());
				manifestIdsReplacer.ReplaceIds(ProjectFilePath);
				DiscardCachedData();
				Load(ProjectFilePath, createProjectListItem: true);
			}
		}

		public ProjectListItem CreateProjectListItem()
		{
			if (IsLoaded)
			{
				ProjectListItem projectListItem = new ProjectListItem
				{
					ProjectFilePath = _xmlProjectListItem.ProjectFilePath,
					Guid = ProjectRepository.ProjectGuid,
					ProjectInfo = ProjectRepository.ProjectInfo
				};
				UpdateProjectListItemSettings(_listItemSettingsBundle);
				SaveProjectListItemSettings(projectListItem, _listItemSettingsBundle);
				_xmlProjectListItem = projectListItem;
			}
			else
			{
				SaveProjectListItemSettings(_xmlProjectListItem, _listItemSettingsBundle);
			}
			return _xmlProjectListItem;
		}

		private void CreateProjectListItem(string projectFilePath)
		{
			_xmlProjectListItem = new ProjectListItem
			{
				Guid = ProjectRepository.ProjectGuid,
				ProjectFilePath = projectFilePath,
				ProjectInfo = ProjectRepository.ProjectInfo
			};
			_listItemSettingsBundle = SettingsUtil.CreateSettingsBundle((ISettingsBundle)null);
			UpdateProjectListItemSettings(_listItemSettingsBundle);
		}

		private static ISettingsBundle LoadProjectListItemSettings(ProjectListItem xmlProjectListItem)
		{
			SettingsBundle settingsBundle = xmlProjectListItem.SettingsBundle;
			if (settingsBundle != null)
			{
				return settingsBundle.LoadSettingsBundle(null);
			}
			return SettingsUtil.CreateSettingsBundle((ISettingsBundle)null);
		}

		public void UnArchive()
		{
			lock (GetLockObject())
			{
				_xmlProjectListItem.ProjectInfo.ArchivedAtSpecified = false;
				_xmlProjectListItem.ProjectInfo.ArchivedAt = DateTime.MaxValue;
				_xmlProjectListItem.ProjectInfo.Status = ((!_xmlProjectListItem.ProjectInfo.CompletedAtSpecified) ? ProjectStatus.Started : ProjectStatus.Completed);
			}
		}

		private static void SaveProjectListItemSettings(ProjectListItem projectListItem, ISettingsBundle settingsBundle)
		{
			if (settingsBundle.IsEmpty)
			{
				if (projectListItem.SettingsBundle != null)
				{
					projectListItem.SettingsBundle = null;
				}
				return;
			}
			if (projectListItem.SettingsBundle == null)
			{
				projectListItem.SettingsBundle = new SettingsBundle();
			}
			projectListItem.SettingsBundle.SaveSettingsBundle(settingsBundle);
		}

		private void UpdateProjectListItemSettings(ISettingsBundle settingsBundle)
		{
			if (!(Repository is OnlineProjectRepository))
			{
				Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings settingsGroup = base.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>();
				Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings settingsGroup2 = settingsBundle.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>();
				PublishProjectOperationSettings settingsGroup3 = base.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
				PublishProjectOperationSettings settingsGroup4 = settingsBundle.GetSettingsGroup<PublishProjectOperationSettings>();
				((AbstractSettingsGroupBase)settingsGroup2).ImportSettings((ISettingsGroup)(object)settingsGroup);
				((AbstractSettingsGroupBase)settingsGroup4).ImportSettings((ISettingsGroup)(object)settingsGroup3);
				TranslationManagementSystemInfoSettings settingsGroup5 = base.Settings.GetSettingsGroup<TranslationManagementSystemInfoSettings>();
				TranslationManagementSystemInfoSettings settingsGroup6 = settingsBundle.GetSettingsGroup<TranslationManagementSystemInfoSettings>();
				((AbstractSettingsGroupBase)settingsGroup6).ImportSettings((ISettingsGroup)(object)settingsGroup5);
				SecureProjectSettings settingsGroup7 = base.Settings.GetSettingsGroup<SecureProjectSettings>();
				SecureProjectSettings settingsGroup8 = settingsBundle.GetSettingsGroup<SecureProjectSettings>();
				((AbstractSettingsGroupBase)settingsGroup8).ImportSettings((ISettingsGroup)(object)settingsGroup7);
			}
		}

		private void OnProjectLoaded()
		{
			DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(ProjectFilePath);
			PublishProjectOperationSettings settingsGroup = _listItemSettingsBundle.GetSettingsGroup<PublishProjectOperationSettings>();
			TranslationManagementSystemInfoSettings settingsGroup2 = _listItemSettingsBundle.GetSettingsGroup<TranslationManagementSystemInfoSettings>();
			SecureProjectSettings settingsGroup3 = _listItemSettingsBundle.GetSettingsGroup<SecureProjectSettings>();
			if (settingsGroup.LastSyncedAt.Value > lastWriteTimeUtc)
			{
				PublishProjectOperationSettings settingsGroup4 = base.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
				((AbstractSettingsGroupBase)settingsGroup4).ImportSettings((ISettingsGroup)(object)settingsGroup);
				TranslationManagementSystemInfoSettings settingsGroup5 = base.Settings.GetSettingsGroup<TranslationManagementSystemInfoSettings>();
				((AbstractSettingsGroupBase)settingsGroup5).ImportSettings((ISettingsGroup)(object)settingsGroup2);
				SecureProjectSettings settingsGroup6 = base.Settings.GetSettingsGroup<SecureProjectSettings>();
				((AbstractSettingsGroupBase)settingsGroup6).ImportSettings((ISettingsGroup)(object)settingsGroup3);
			}
			else if (lastWriteTimeUtc > settingsGroup.LastSyncedAt.Value)
			{
				PublishProjectOperationSettings settingsGroup7 = base.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
				((AbstractSettingsGroupBase)settingsGroup).ImportSettings((ISettingsGroup)(object)settingsGroup7);
				TranslationManagementSystemInfoSettings settingsGroup8 = base.Settings.GetSettingsGroup<TranslationManagementSystemInfoSettings>();
				((AbstractSettingsGroupBase)settingsGroup2).ImportSettings((ISettingsGroup)(object)settingsGroup8);
				SecureProjectSettings settingsGroup9 = base.Settings.GetSettingsGroup<SecureProjectSettings>();
				((AbstractSettingsGroupBase)settingsGroup3).ImportSettings((ISettingsGroup)(object)settingsGroup9);
			}
			_xmlProjectListItem.ProjectInfo = ProjectRepository.ProjectInfo;
		}

		private void OnBeforeCachedDataDiscarded()
		{
			if (IsLoaded)
			{
				UpdateProjectListItemSettings(_listItemSettingsBundle);
				SaveProjectListItemSettings(_xmlProjectListItem, _listItemSettingsBundle);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Project project)
			{
				return project.Guid == Guid;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}

		private void Load()
		{
			Load(ProjectFilePath, createProjectListItem: false);
		}

		private void Load(string projectFilePath, bool createProjectListItem)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			_projectRepository.Load(projectFilePath);
			if (createProjectListItem)
			{
				CreateProjectListItem(base.PathUtil.MakeRelativePath(projectFilePath, base.ProjectsProvider.LocalDataFolder));
			}
			else
			{
				OnProjectLoaded();
			}
			try
			{
				LoadTasksList();
				LoadComplexTasksList();
				LoadPackageOperations();
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Project_ErrorLoading, _projectRepository.ProjectInfo.Name, ex.Message), ex);
			}
		}

		private void LoadPackageOperations()
		{
			_projectPackageCreations = ProjectRepository.LoadProjectPackageCreations((IProject)(object)this, base.ProjectsProvider.PackageInitializer);
			_projectPackageImports = ProjectRepository.LoadProjectPackageImports((IProject)(object)this);
			_returnPackageCreations = ProjectRepository.LoadReturnPackageCreations((IProject)(object)this, base.ProjectsProvider.PackageInitializer);
			_returnPackageImports = ProjectRepository.LoadReturnPackageImports((IProject)(object)this, base.ProjectsProvider.PackageInitializer);
		}

		public string GetProjectLanguageDirectory(Language language)
		{
			if (!IsInPlace)
			{
				return Path.Combine(LocalDataFolder, ((LanguageBase)language).IsoAbbreviation);
			}
			return Path.GetDirectoryName(ProjectFilePath);
		}

		internal string GetProjectReportsDirectory()
		{
			return Path.Combine(LocalDataFolder, "Reports");
		}

		internal string GetNextReportFilePath(string reportName, ILanguageDirection languageDirection)
		{
			int num = 0;
			string projectReportsDirectory = GetProjectReportsDirectory();
			string path;
			do
			{
				path = ((languageDirection != null) ? (reportName + " " + ((LanguageBase)languageDirection.SourceLanguage).IsoAbbreviation + "_" + ((LanguageBase)languageDirection.TargetLanguage).IsoAbbreviation + ((num > 0) ? ("(" + num + ")") : "") + ".xml") : (reportName + " " + ((num > 0) ? ("(" + num + ")") : "") + ".xml"));
				path = Path.Combine(projectReportsDirectory, path);
				num++;
			}
			while (File.Exists(path));
			return path;
		}

		public void RemoveLanguageFile(ILanguageFile languageFile)
		{
			ProjectRepository.RemoveLanguageFile(languageFile);
		}

		private void LoadTasksList()
		{
			_projectTaskFileManager = (IProjectTaskFileManager)(object)new ProjectTaskFileManager();
			_tasksList = ProjectRepository.GetTasks((IProject)(object)this);
		}

		private void LoadComplexTasksList()
		{
			ComplexTasksList = new List<ComplexTask>();
			for (int i = 0; i < TasksList.Count; i++)
			{
				if (!(TasksList[i] is AutomaticTask automaticTask) || string.IsNullOrEmpty(automaticTask.XmlTask.ComplexTaskTemplateId))
				{
					continue;
				}
				List<AutomaticTask> list = new List<AutomaticTask> { automaticTask };
				AutomaticTask automaticTask2 = automaticTask;
				for (; i + 1 < TasksList.Count && TasksList[i + 1] is AutomaticTask automaticTask3; i++)
				{
					if (!(automaticTask3.XmlTask.PredecessorTaskGuid == automaticTask2.XmlTask.Guid))
					{
						break;
					}
					list.Add(automaticTask3);
					automaticTask2 = automaticTask3;
				}
				ComplexTasksList.Add(new ComplexTask((IProject)(object)this, list));
			}
		}

		internal AutomaticTask GetAutomaticTask(Guid guid)
		{
			return GetTask<AutomaticTask>(guid);
		}

		internal ManualTask GetManualTask(Guid guid)
		{
			return GetTask<ManualTask>(guid);
		}

		private T GetTask<T>(Guid guid) where T : ScheduledTask
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			foreach (ScheduledTask tasks in TasksList)
			{
				TaskId id = tasks.Id;
				if (((TaskId)(ref id)).ToGuidArray()[0] == guid)
				{
					if (!(tasks is T result))
					{
						throw new ArgumentException($"The specified id represents a task of a different type (expected {typeof(T)}).");
					}
					return result;
				}
			}
			return null;
		}

		private bool SplitIntoTargetLanguage(IProjectFile sourceLanguageFile, Language targetLanguage, out IProjectFile targetLanguageFile)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			LanguageFile languageFile = (LanguageFile)(object)sourceLanguageFile;
			MergedTranslatableFile languageFile2;
			if (sourceLanguageFile is TranslatableFile translatableFile && (languageFile2 = (MergedTranslatableFile)(object)translatableFile.MergedFile) != null)
			{
				IProjectFile targetLanguageFile2;
				bool result = ProjectRepository.SplitFileIntoTargetLanguage((IProject)(object)this, (ILanguageFile)(object)languageFile2, targetLanguage, out targetLanguageFile2);
				IMergedTranslatableFile val = (IMergedTranslatableFile)targetLanguageFile2;
				targetLanguageFile = (IProjectFile)(object)Array.Find(val.ChildFiles, (ITranslatableFile tf) => ((IProjectFile)tf.SourceLanguageFile).Guid.Equals(sourceLanguageFile.Guid));
				return result;
			}
			return ProjectRepository.SplitFileIntoTargetLanguage((IProject)(object)this, (ILanguageFile)(object)languageFile, targetLanguage, out targetLanguageFile);
		}

		public void AddProjectPackageCreationOperation(IProjectPackageCreation c)
		{
			ProjectRepository.AddProjectPackageCreationOperation(c);
			_projectPackageCreations.Add(c);
		}

		public void AddProjectPackageImportOperation(IProjectPackageImport i)
		{
			ProjectRepository.AddProjectPackageImportOperation(i);
			_projectPackageImports.Add(i);
		}

		public void AddReturnPackageCreationOperation(IReturnPackageCreation c)
		{
			ProjectRepository.AddReturnPackageCreationOperation(c);
			_returnPackageCreations.Add(c);
		}

		public void AddReturnPackageImportOperation(IReturnPackageImport i)
		{
			ProjectRepository.AddReturnPackageImportOperation(i);
			_returnPackageImports.Add(i);
		}

		protected override void OnSourceLanguageChanged(object sender, LanguageDirectionChangeEventArgs e)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			base.OnSourceLanguageChanged(sender, e);
			this.SourceLanguageChanged?.Invoke(this, new LanguageChangeEventArgs(e.OldLanguage, e.LanguageDirection.SourceLanguage));
		}

		public override string ToString()
		{
			return Name;
		}

		public IReturnPackageCreation CreateReturnPackage(ITaskFile[] files, string packageName, string comment)
		{
			if (files == null || files.Length == 0)
			{
				throw new ArgumentNullException("files");
			}
			if (string.IsNullOrEmpty(packageName))
			{
				throw new ArgumentNullException("packageName");
			}
			return (IReturnPackageCreation)(object)new ReturnPackageCreation((IProject)(object)this, files, packageName, comment, base.ProjectsProvider.PackageInitializer, base.PathUtil);
		}

		public bool AllowsOperation(string operationId)
		{
			return _operation.IsAllowed((IProject)(object)this, operationId);
		}

		public IProjectOperationResult ExecuteOperation(string operationId, object[] args)
		{
			return _operation.Execute((IProject)(object)this, operationId, args);
		}

		public async Task<IProjectOperationResult> ExecuteOperationAsync(string operationId, object[] args)
		{
			return await _operation.ExecuteAsync((IProject)(object)this, operationId, args);
		}

		public void ForceReload()
		{
			DiscardCachedData();
			lock (GetLockObject())
			{
				Load();
			}
			_eventAggregator.Publish<ProjectForcedReloadEvent>(new ProjectForcedReloadEvent
			{
				ProjectGuid = Guid
			});
		}

		public void Unload()
		{
			base.ProjectsProvider.RemoveProject((IProject)(object)this);
		}

		public IReadOnlyList<IProjectFile> GetProjectFiles(IEnumerable<Guid> projectFileGuids)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			if (projectFileGuids == null || projectFileGuids.Count() == 0)
			{
				return list;
			}
			foreach (Guid projectFileGuid in projectFileGuids)
			{
				IProjectFile projectFile = GetProjectFile(projectFileGuid);
				if (projectFile == null)
				{
					Guid guid = projectFileGuid;
					throw new ArgumentException("Could not find the file with id: " + guid.ToString());
				}
				list.Add(projectFile);
			}
			return list;
		}
	}
}
