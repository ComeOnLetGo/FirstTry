using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Extensions.Logging;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.Desktop.Platform.PluginManagement;
using Sdl.Desktop.Platform.Services;
using Sdl.Platform.Interfaces.Telemetry;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.LanguageCloud;
using Sdl.ProjectApi.Implementation.ProjectSettings;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Interfaces;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectsProvider : IProjectsProvider
	{
		private readonly object _projectTemplatesLock = new object();

		private IWorkflow _lazyDefaultWorkflow;

		private List<IProjectTemplate> _lazyProjectTemplatesList;

		private List<IProject> _lazyProjectsList;

		private readonly ILogger _log;

		private IProjectPackageInitializer _packageInitializer;

		private readonly IProjectsProviderRepository _projectsProviderRepository;

		private readonly IProjectTemplatesProviderRepository _projectTemplatesProviderRepository;

		private readonly IProjectTemplateRepositoryFactory _projectTemplateRepositoryFactory;

		private readonly IWorkflowProviderRepository _workflowProviderRepository;

		private readonly IEventAggregator _eventAggregator;

		private readonly IProjectOperation _projectOperation;

		private readonly ITelemetryService _telemetryService;

		private readonly IDirectory _directoryWrapper;

		private readonly ILoggerFactory _loggerFactory;

		public IApplication Application { get; }

		public ICustomerProvider CustomerProvider { get; }

		public IUserProvider UserProvider { get; }

		public IProjectPathUtil PathUtil { get; }

		public string LocalDataFolder { get; }

		public IProject[] MyProjects
		{
			get
			{
				lock (SyncRoot)
				{
					return ProjectsList.ToArray();
				}
			}
		}

		public IProject[] ActiveProjects
		{
			get
			{
				lock (SyncRoot)
				{
					return ProjectsList.Where((IProject project) => (int)project.Status == 1 || (int)project.Status == 2).Cast<IProject>().ToArray();
				}
			}
		}

		public IProjectTemplate[] ProjectTemplates
		{
			get
			{
				lock (_projectTemplatesLock)
				{
					return ProjectTemplatesList.ToArray();
				}
			}
		}

		public IWorkflow Workflow => _lazyDefaultWorkflow ?? (_lazyDefaultWorkflow = _workflowProviderRepository.GetWorkflow(StudioPluginManager.PluginRegistry));

		internal List<IProject> ProjectsList
		{
			get
			{
				if (_lazyProjectsList == null)
				{
					try
					{
						_lazyProjectsList = _projectsProviderRepository.GetProjects((IProjectsProvider)(object)this, LocalDataFolder, _projectOperation);
					}
					catch (CloudProjectsLoadException ex)
					{
						_lazyProjectsList = ex.LocalProjects;
					}
				}
				return _lazyProjectsList;
			}
			private set
			{
				_lazyProjectsList = value;
			}
		}

		public object SyncRoot { get; } = new object();


		internal List<IProjectTemplate> ProjectTemplatesList
		{
			get
			{
				if (_lazyProjectTemplatesList == null)
				{
					_lazyProjectTemplatesList = _projectTemplatesProviderRepository.GetProjectTemplates((IProjectsProvider)(object)this);
				}
				return _lazyProjectTemplatesList;
			}
		}

		public IProjectPackageInitializer PackageInitializer => _packageInitializer ?? (_packageInitializer = (IProjectPackageInitializer)(object)new ProjectPackageInitializer(this, PathUtil, _projectOperation, _eventAggregator));

		public IProjectsOriginCache ProjectsOriginCache { get; }

		internal ProjectsProvider(IApplication application, IProjectsProviderRepository projectsProviderRepository, IProjectTemplatesProviderRepository projectTemplatesProviderRepository, IProjectTemplateRepositoryFactory projectTemplateRepositoryFactory, IWorkflowProviderRepository workflowProviderRepository, string localDataFolder, IProjectPathUtil pathUtil, IUserProvider userProvider, ICustomerProvider customerProvider, IProjectOperation projectOperation, ITelemetryService telemetryService, IEventAggregator eventAggregator, IProjectsOriginCache projectsOriginCache, IDirectory directoryWrapper, ILoggerFactory loggerFactory)
		{
			_eventAggregator = eventAggregator;
			_projectOperation = projectOperation;
			_telemetryService = telemetryService;
			CustomerProvider = customerProvider;
			UserProvider = userProvider ?? throw new ArgumentNullException("userProvider");
			Application = application;
			_projectsProviderRepository = projectsProviderRepository;
			_workflowProviderRepository = workflowProviderRepository;
			_projectTemplatesProviderRepository = projectTemplatesProviderRepository;
			_projectTemplateRepositoryFactory = projectTemplateRepositoryFactory;
			PathUtil = pathUtil;
			LocalDataFolder = localDataFolder;
			ProjectsOriginCache = projectsOriginCache;
			PopulateProjectsOriginCache();
			_directoryWrapper = directoryWrapper;
			_loggerFactory = loggerFactory;
			_log = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<IProjectsProvider>(_loggerFactory);
		}

		public void Reload(bool overrideCachedData)
		{
			try
			{
				_lazyProjectsList = _projectsProviderRepository.GetProjects((IProjectsProvider)(object)this, LocalDataFolder, _projectOperation);
			}
			catch (CloudProjectsLoadException ex)
			{
				if (overrideCachedData)
				{
					_lazyProjectsList = ex.LocalProjects;
				}
				throw new CloudProjectsLoadException("Failed to load Cloud projects", ex, ex.UnauthorizedAccounts);
			}
		}

		public void CheckProjectFolder(string proposedProjectPath)
		{
			if (!Directory.Exists(proposedProjectPath))
			{
				Directory.CreateDirectory(proposedProjectPath);
			}
		}

		private void PopulateProjectsOriginCache()
		{
			if (ProjectsList == null)
			{
				return;
			}
			IEnumerable<string> enumerable = ProjectsList.Select((IProject p) => p.ProjectOrigin);
			foreach (string item in enumerable)
			{
				ProjectsOriginCache.AddOrUpdateOrigin(item);
			}
		}

		public void Save()
		{
			lock (SyncRoot)
			{
				_projectTemplatesProviderRepository.Save((IEnumerable<IProjectTemplate>)ProjectTemplatesList);
				_projectsProviderRepository.Save(ProjectsList, LocalDataFolder);
			}
			UserProvider.SaveCurrentWindowsUser();
		}

		public IProject GetProject(string projectName)
		{
			lock (SyncRoot)
			{
				return ProjectsList.FirstOrDefault((IProject project) => project.Name == projectName);
			}
		}

		public IProject GetProject(Guid projectGuid)
		{
			lock (SyncRoot)
			{
				return ProjectsList.FirstOrDefault((IProject project) => project.Guid == projectGuid);
			}
		}

		internal void AddOrUpdateManualTaskIndexEntry(IManualTask task)
		{
			lock (SyncRoot)
			{
				_workflowProviderRepository.AddOrUpdateManualTask(task);
			}
		}

		internal void RemoveManualTaskIndexEntry(ManualTask task)
		{
			lock (SyncRoot)
			{
				_workflowProviderRepository.RemoveManualTask((IManualTask)(object)task);
			}
		}

		public IProjectTemplate GetProjectTemplate(string projectTemplateName)
		{
			lock (_projectTemplatesLock)
			{
				return ProjectTemplatesList.FirstOrDefault((IProjectTemplate projectTemplate) => projectTemplate.Name == projectTemplateName);
			}
		}

		public IProjectTemplate GetProjectTemplate(Guid projectTemplateGuid)
		{
			lock (_projectTemplatesLock)
			{
				return ProjectTemplatesList.FirstOrDefault((IProjectTemplate projectTemplate) => projectTemplate.Guid == projectTemplateGuid);
			}
		}

		public IProject CreateNewProject(string projectName, Language sourceLanguage, Language[] targetLanguages, IProjectTemplate projectTemplate, string localDataFolder)
		{
			Util.CheckArgumentNotNull("projectName", projectName);
			Util.CheckArgumentNotNull("projectTemplate", projectTemplate);
			Util.CheckArgumentNotNull("targetLanguages", targetLanguages);
			Util.CheckArgumentNotNull("localDataFolder", localDataFolder);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			ValidateProjectName(projectName);
			string projectFilePath = Path.Combine(localDataFolder, GetProjectFileName(projectName));
			ProjectRepository projectRepository = new ProjectRepository(Application, PathUtil, projectName, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: false, sourceLanguage);
			projectRepository.AddUser(UserProvider.CurrentUser);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, targetLanguages, projectTemplate, projectRepository, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(IProjectRepository projectRepository, string projectName, Language sourceLanguage, IList<Language> targetLanguages, IProjectTemplate projectTemplate, string localDataFolder)
		{
			Util.CheckArgumentNotNull("projectRepository", projectRepository);
			Util.CheckArgumentNotNull("projectTemplate", projectTemplate);
			Util.CheckArgumentNotNull("localDataFolder", localDataFolder);
			Util.CheckArgumentNotNull("projectName", projectName);
			Util.CheckArgumentNotNull("targetLanguages", targetLanguages);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			string projectFilePath = Path.Combine(localDataFolder, GetProjectFileName(projectName));
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, targetLanguages.ToArray(), projectTemplate, projectRepository, _projectOperation, _eventAggregator);
			project.Save();
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectName, Language sourceLanguage, Language[] targetLanguages, IProject referenceProject, string localDataFolder)
		{
			Util.CheckArgumentNotNull("projectName", projectName);
			Util.CheckArgumentNotNull("referenceProject", referenceProject);
			Util.CheckArgumentNotNull("targetLanguages", targetLanguages);
			Util.CheckArgumentNotNull("localDataFolder", localDataFolder);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			ValidateProjectName(projectName);
			string projectFilePath = Path.Combine(localDataFolder, GetProjectFileName(projectName));
			ProjectRepository repository = new ProjectRepository(Application, PathUtil, projectName, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: false, sourceLanguage);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, targetLanguages, referenceProject, repository, UserProvider.CurrentUser, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectFilePath, Language sourceLanguage, Language targetLanguage, IProjectTemplate projectTemplate)
		{
			Util.CheckArgumentNotNull("projectFilePath", projectFilePath);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			Util.CheckArgumentNotNull("targetLanguage", targetLanguage);
			Util.CheckArgumentNotNull("projectTemplate", projectTemplate);
			string projectNameFromFilePath = PathUtil.GetProjectNameFromFilePath(projectFilePath);
			ProjectRepository projectRepository = new ProjectRepository(Application, PathUtil, projectNameFromFilePath, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: true, sourceLanguage);
			projectRepository.AddUser(UserProvider.CurrentUser);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, (Language[])(object)new Language[1] { targetLanguage }, projectTemplate, projectRepository, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectFilePath, Language sourceLanguage, IProject referenceProject)
		{
			Util.CheckArgumentNotNull("projectFilePath", projectFilePath);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			Util.CheckArgumentNotNull("referenceProject", referenceProject);
			string projectNameFromFilePath = PathUtil.GetProjectNameFromFilePath(projectFilePath);
			ProjectRepository repository = new ProjectRepository(Application, PathUtil, projectNameFromFilePath, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: true, sourceLanguage);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, (Language[])(object)new Language[0], referenceProject, repository, UserProvider.CurrentUser, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectFilePath, Language sourceLanguage, IProjectTemplate projectTemplate)
		{
			Util.CheckArgumentNotNull("projectFilePath", projectFilePath);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			Util.CheckArgumentNotNull("projectTemplate", projectTemplate);
			string projectNameFromFilePath = PathUtil.GetProjectNameFromFilePath(projectFilePath);
			ProjectRepository projectRepository = new ProjectRepository(Application, PathUtil, projectNameFromFilePath, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: true, sourceLanguage);
			projectRepository.AddUser(UserProvider.CurrentUser);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, (Language[])(object)new Language[0], projectTemplate, projectRepository, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectFilePath, Language sourceLanguage, Language targetLanguage, IProject referenceProject)
		{
			Util.CheckArgumentNotNull("projectFilePath", projectFilePath);
			Util.CheckArgumentNotNull("sourceLanguage", sourceLanguage);
			Util.CheckArgumentNotNull("targetLanguage", targetLanguage);
			Util.CheckArgumentNotNull("referenceProject", referenceProject);
			string projectNameFromFilePath = PathUtil.GetProjectNameFromFilePath(projectFilePath);
			ProjectRepository repository = new ProjectRepository(Application, PathUtil, projectNameFromFilePath, Guid.NewGuid(), UserProvider.CurrentUser, DateTime.UtcNow, inPlace: true, sourceLanguage);
			Project project = new Project((IProjectsProvider)(object)this, projectFilePath, sourceLanguage, (Language[])(object)new Language[1] { targetLanguage }, referenceProject, repository, UserProvider.CurrentUser, _projectOperation, _eventAggregator);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		public IProject CreateNewProject(string projectFilePath, IPackageProject packageProject)
		{
			Util.CheckArgumentNotNull("projectFilePath", projectFilePath);
			Util.CheckArgumentNotNull("packageProject", packageProject);
			ProjectRepository repository = ((!((IProject)packageProject).IsSecure) ? new ProjectRepository(Application, packageProject, PathUtil) : new SecureProjectRepository(Application, packageProject, PathUtil));
			Project project = new Project(this, projectFilePath, packageProject, repository, _projectOperation, _eventAggregator);
			CopyOriginSettings(project, packageProject);
			AddProject((IProject)(object)project);
			return (IProject)(object)project;
		}

		private void CopyOriginSettings(Project project, IPackageProject packageProject)
		{
			if (((IObjectWithSettings)packageProject).Settings.ContainsSettingsGroup("ProjectSettings"))
			{
				Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings settingsGroup = ((IObjectWithSettings)packageProject).Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>();
				project.Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectOrigin = settingsGroup.ProjectOrigin;
			}
		}

		public IProjectTemplate CreateNewProjectTemplate(string name, string description, string projectTemplateFilePath)
		{
			ProjectTemplateRepository repository = new ProjectTemplateRepository(Application, PathUtil);
			ProjectTemplate projectTemplate = new ProjectTemplate((IProjectsProvider)(object)this, description, projectTemplateFilePath, PathUtil, (IProjectTemplateRepository)(object)repository, UserProvider.CurrentUser, _directoryWrapper);
			AddProjectTemplate(projectTemplate);
			return (IProjectTemplate)(object)projectTemplate;
		}

		public IProjectTemplate CreateNewProjectTemplate(string description, string projectTemplateFilePath, IProject project)
		{
			ProjectTemplateRepository repository = new ProjectTemplateRepository(Application, PathUtil);
			ProjectTemplate projectTemplate = new ProjectTemplate((IProjectsProvider)(object)this, description, projectTemplateFilePath, project, PathUtil, (IProjectTemplateRepository)(object)repository, UserProvider.CurrentUser, _directoryWrapper);
			AddProjectTemplate(projectTemplate);
			return (IProjectTemplate)(object)projectTemplate;
		}

		public virtual IProject ImportProject(string projectFilePath)
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			lock (SyncRoot)
			{
				foreach (Project projects in ProjectsList)
				{
					if (object.Equals(projects.ProjectFilePath, projectFilePath))
					{
						return (IProject)(object)projects;
					}
				}
				IProject val = _projectsProviderRepository.LoadNewProject(projectFilePath, (IProjectsProvider)(object)this, _projectOperation);
				if (GetProject(val.Guid) != null)
				{
					throw new ProjectAlreadyExistsException(string.Format(ErrorMessages.Project_ProjectAlreadyExists, val.Guid));
				}
				List<Language> list = new List<Language>();
				if (!val.SourceLanguage.IsSupported)
				{
					list.Add(val.SourceLanguage);
				}
				list.AddRange(val.TargetLanguages.Where((Language l) => !l.IsSupported));
				if (list.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < list.Count; i++)
					{
						stringBuilder.Append(list[i].DisplayName);
						if (i < list.Count - 1)
						{
							stringBuilder.Append(", ");
						}
					}
					throw new ProjectApiException(string.Format(ErrorMessages.ImportProject_LanguagesNotSupported, stringBuilder));
				}
				val.UnArchive();
				IManualTask[] manualTasks = val.ManualTasks;
				for (int j = 0; j < manualTasks.Length; j++)
				{
					ManualTask task = (ManualTask)(object)manualTasks[j];
					AddOrUpdateManualTaskIndexEntry((IManualTask)(object)task);
				}
				AddProject(val);
				return val;
			}
		}

		public IProjectTemplate ImportProjectTemplate(string projectTemplateFilePath, DialogResult dialogResult = DialogResult.None)
		{
			foreach (IProjectTemplate projectTemplates in ProjectTemplatesList)
			{
				if (object.Equals(projectTemplates.ProjectTemplateFilePath, projectTemplateFilePath))
				{
					return projectTemplates;
				}
			}
			IProjectTemplateRepository repository = _projectTemplateRepositoryFactory.Create(Application, PathUtil);
			ProjectTemplate projectTemplate = new ProjectTemplate((IProjectsProvider)(object)this, projectTemplateFilePath, PathUtil, repository, _directoryWrapper);
			if (projectTemplate.Name != StringResources.LCProjectTemplate_Name && !projectTemplate.IsCloudTemplate())
			{
				switch (dialogResult)
				{
				case DialogResult.Yes:
					RemoveProjectTemplate(ProjectTemplatesList.First((IProjectTemplate p) => p.Guid == projectTemplate.Guid));
					break;
				case DialogResult.No:
				{
					IProjectConfigurationRepository repository2 = projectTemplate.Repository;
					IProjectTemplateRepository val = (IProjectTemplateRepository)(object)((repository2 is IProjectTemplateRepository) ? repository2 : null);
					val.AssignNewGuid();
					projectTemplate.Save();
					break;
				}
				case DialogResult.Cancel:
					return null;
				}
				AddProjectTemplate(projectTemplate);
				return (IProjectTemplate)(object)projectTemplate;
			}
			return null;
		}

		public Guid GetProjectTemplateGuid(string projectTemplateFilePath)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(projectTemplateFilePath);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("/ProjectTemplate");
				string value = xmlNode.Attributes["Guid"].Value;
				return Guid.Parse(value);
			}
			catch
			{
				LoggerExtensions.LogError(_log, "Failed to get project template Guid", Array.Empty<object>());
				return Guid.Empty;
			}
		}

		public IProjectPackageImport ImportProjectPackage(string packageFilename)
		{
			return ImportProjectPackage(packageFilename, null);
		}

		public IProjectPackageImport ImportProjectPackage(string packageFilename, string proposedProjectFolder, IPackageProject packageProject = null)
		{
			return (IProjectPackageImport)(object)new ProjectPackageImport((IProjectsProvider)(object)this, packageFilename, proposedProjectFolder, packageProject, PathUtil);
		}

		public IReturnPackageImport ImportReturnPackage(string packageFilename)
		{
			return (IReturnPackageImport)(object)new ReturnPackageImport((IProjectsProvider)(object)this, packageFilename, PackageInitializer, PathUtil);
		}

		public IOpenServerProjectOperation OpenServerProject(Uri serverUri, Guid projectGuid, string localProjectFolder)
		{
			return (IOpenServerProjectOperation)(object)new OpenServerProjectOperation(Application.CommuteClientManager, (IProjectsProvider)(object)this, serverUri, projectGuid, localProjectFolder, PathUtil);
		}

		public ITranslatableFile[] FindMatchingTranslatableFiles(string localFilePath)
		{
			List<IProject> list = FindInPlaceProjects(localFilePath);
			List<ITranslatableFile> list2 = new List<ITranslatableFile>();
			foreach (IProject item in list)
			{
				if (item.TargetLanguages.Length > 1)
				{
					LoggerExtensions.LogError(_log, "Only expected to find one target language this project: '" + item.Name + "'.", Array.Empty<object>());
					continue;
				}
				if (item.TargetLanguages.Length == 1)
				{
					ITranslatableFile[] translatableFiles = item.GetTranslatableFiles(item.TargetLanguages[0]);
					if (translatableFiles.Length > 1)
					{
						LoggerExtensions.LogError(_log, "Only expected to find one target file in this project: '" + item.Name + "'.", Array.Empty<object>());
						continue;
					}
					if (translatableFiles.Length == 1 && MatchesFileRevision(localFilePath, translatableFiles[0]))
					{
						list2.Add(translatableFiles[0]);
						continue;
					}
				}
				ITranslatableFile[] translatableFiles2 = item.GetTranslatableFiles(item.SourceLanguage);
				if (translatableFiles2.Length > 1)
				{
					LoggerExtensions.LogError(_log, "Only expected to find one source file in this project: '" + item.Name + "'.", Array.Empty<object>());
				}
				else if (translatableFiles2.Length == 1)
				{
					ITranslatableFile val = translatableFiles2[0];
					if (MatchesFileRevision(localFilePath, val))
					{
						list2.Add(val);
					}
				}
			}
			return list2.ToArray();
		}

		private bool MatchesFileRevision(string filePath, ITranslatableFile translatableFile)
		{
			return ((ILanguageFile)translatableFile).Revisions.Any((IFileRevision revision) => revision.LocalFilePath.StartsWith(filePath, StringComparison.InvariantCultureIgnoreCase));
		}

		private List<IProject> FindInPlaceProjects(string localFilePath)
		{
			string text = ".xliff";
			if (localFilePath.ToLower().EndsWith(text))
			{
				localFilePath = localFilePath.Substring(0, localFilePath.Length - text.Length - ((!text.StartsWith(".")) ? 1 : 0));
			}
			string directoryName = Path.GetDirectoryName(localFilePath);
			string fileName = Path.GetFileName(localFilePath);
			string searchPattern = fileName + "*" + FileTypes.ProjectFileExtension;
			string[] files = Directory.GetFiles(directoryName, searchPattern);
			List<IProject> list = new List<IProject>();
			string[] array = files;
			foreach (string text2 in array)
			{
				Guid guid = ReadProjectGuid(text2);
				if (!(guid != Guid.Empty))
				{
					continue;
				}
				IProject val = GetProject(guid);
				if (val == null)
				{
					try
					{
						val = ImportProject(text2);
					}
					catch (Exception ex)
					{
						LoggerExtensions.LogError(_log, ex, "Failed to import project '" + text2 + "'. Ignoring project file.", Array.Empty<object>());
					}
				}
				if (val != null)
				{
					try
					{
						val.Check();
					}
					catch (Exception ex2)
					{
						LoggerExtensions.LogError(_log, ex2, "Project cannot be used '" + text2 + "'. Ignoring project file.", Array.Empty<object>());
						continue;
					}
					list.Add(val);
				}
			}
			return list;
		}

		private Guid ReadProjectGuid(string projectFilePath)
		{
			try
			{
				using XmlReader xmlReader = XmlReader.Create(projectFilePath);
				if (xmlReader.ReadToFollowing("Project", ""))
				{
					string attribute = xmlReader.GetAttribute("Guid");
					if (!string.IsNullOrEmpty(attribute))
					{
						return new Guid(attribute);
					}
					LoggerExtensions.LogError(_log, "Failed to read project file '" + projectFilePath + "'. No Guid attribute found on <Project> root element found. Ignoring project file.", Array.Empty<object>());
					return Guid.Empty;
				}
				LoggerExtensions.LogError(_log, "Failed to read project file '" + projectFilePath + "'. No <Project> root element found. Ignoring project file.", Array.Empty<object>());
				return Guid.Empty;
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError(_log, ex, "Failed to read project file '" + projectFilePath + "'. Ignoring project file.", Array.Empty<object>());
				return Guid.Empty;
			}
		}

		internal void AddProjectTemplate(ProjectTemplate projectTemplate)
		{
			lock (_projectTemplatesLock)
			{
				if (ProjectTemplatesList.Count((IProjectTemplate p) => p.Guid == projectTemplate.Guid) == 0 && projectTemplate.Name != StringResources.LCProjectTemplate_Name && !projectTemplate.IsCloudTemplate())
				{
					ProjectTemplatesList.Add((IProjectTemplate)(object)projectTemplate);
				}
			}
		}

		internal void AddProject(IProject project)
		{
			bool flag = false;
			lock (SyncRoot)
			{
				if (ProjectsList != null)
				{
					int num = ProjectsList.FindIndex((IProject p) => p.Guid == project.Guid);
					if (num >= 0)
					{
						ProjectsList[num] = project;
						if (project.IsLCProject)
						{
							flag = true;
						}
					}
					else
					{
						ProjectsList.Add(project);
						flag = true;
					}
					ProjectsOriginCache.AddOrUpdateOrigin(project.ProjectOrigin);
				}
			}
			if (flag)
			{
				SendAddProjectTelemetry(project);
			}
		}

		private void SendAddProjectTelemetry(IProject project)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (_telemetryService != null && ((IService)_telemetryService).IsStarted)
			{
				TrackingEvent val = new TrackingEvent("Project Added");
				Dictionary<string, string> properties = val.Properties;
				object value;
				if (!project.IsSecure)
				{
					ProjectType projectType = project.ProjectType;
					value = ((object)(ProjectType)(ref projectType)).ToString();
				}
				else
				{
					value = "SecureProjectFromLanguageCloud";
				}
				properties.Add("Type", (string)value);
				_telemetryService.TrackEvent((ITrackingEvent)(object)val);
			}
		}

		public void RemoveProjectTemplate(IProjectTemplate projectTemplate)
		{
			if (projectTemplate == null)
			{
				throw new ArgumentNullException("projectTemplate");
			}
			lock (_projectTemplatesLock)
			{
				ProjectTemplatesList.Remove(projectTemplate);
			}
		}

		public void RemoveProject(IProject project)
		{
			lock (SyncRoot)
			{
				_workflowProviderRepository.RemoveProjectManualTask(project.Guid);
				ProjectsList?.Remove(project);
				ProjectsOriginCache.Remove(project.ProjectOrigin);
			}
		}

		internal static string GetProjectFileName(string projectName)
		{
			return projectName + FileTypes.ProjectFileExtension;
		}

		internal static void ValidateProjectName(string projectName)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (projectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				throw new InvalidProjectNameException(string.Format(ErrorMessages.InvalidProjectNameException_InvalidCharacters, new string(Path.GetInvalidFileNameChars())));
			}
		}

		public override string ToString()
		{
			return LocalDataFolder;
		}
	}
}
