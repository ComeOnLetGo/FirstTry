using System;
using System.ComponentModel;
using System.IO;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Settings;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectTemplate : AbstractProjectConfiguration, IProjectTemplate, IProjectConfiguration, ISettingsBundleProvider, IObjectWithSettings, INotifyPropertyChanged
	{
		private readonly IProjectTemplateRepository _templateRepository;

		private readonly IDirectory _directoryWrapper;

		public string ProjectTemplateFilePath => base.PathUtil.MakeAbsolutePath(_templateRepository.ProjectTemplateFilePath, base.ProjectsProvider.LocalDataFolder);

		public override IProjectConfigurationRepository Repository
		{
			get
			{
				if (!_templateRepository.IsInitialized)
				{
					_templateRepository.Load(ProjectTemplateFilePath, base.ProjectsProvider.LocalDataFolder);
				}
				return (IProjectConfigurationRepository)(object)_templateRepository;
			}
		}

		public Guid Guid => _templateRepository.ProjectTemplateGuid;

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(FilePath);
			}
			set
			{
				string text = Path.Combine(Path.GetDirectoryName(FilePath), value + FileTypes.TemplateFileExtension);
				if (File.Exists(FilePath))
				{
					File.Move(FilePath, text);
				}
				_templateRepository.ProjectTemplateFilePath = base.PathUtil.MakeRelativePath(text, base.ProjectsProvider.LocalDataFolder);
			}
		}

		public string Description
		{
			get
			{
				return _templateRepository.Description;
			}
			set
			{
				_templateRepository.Description = value;
			}
		}

		public string FilePath => ProjectTemplateFilePath;

		public string ProjectLocation
		{
			get
			{
				ProjectTemplateSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectTemplateSettings>();
				return Setting<string>.op_Implicit(settingsGroup.ProjectLocation);
			}
			set
			{
				ProjectTemplateSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectTemplateSettings>();
				settingsGroup.ProjectLocation.Value = value;
			}
		}

		internal ProjectTemplate(IProjectsProvider projectsProvider, IProjectPathUtil projectPathUtil, IProjectTemplateRepository repository, IDirectory directoryWrapper)
			: base(projectsProvider, projectPathUtil)
		{
			_templateRepository = repository;
			_directoryWrapper = directoryWrapper;
		}

		internal ProjectTemplate(IProjectsProvider projectsProvider, string description, string projectTemplateFilePath, IProjectPathUtil projectPathUtil, IProjectTemplateRepository repository, IUser currentUser, IDirectory directoryWrapper)
			: base(projectsProvider, projectPathUtil)
		{
			_templateRepository = repository;
			_directoryWrapper = directoryWrapper;
			Guid guid = Guid.NewGuid();
			_templateRepository.Load(projectTemplateFilePath, base.ProjectsProvider.LocalDataFolder, description, guid, currentUser, true);
			base.SettingsBundlesList.AddSettingsBundle(guid, SettingsUtil.CreateSettingsBundle((ISettingsBundle)null));
			base.StartTaskTemplate = base.ProjectsProvider.Workflow.ComplexTaskTemplates[0];
		}

		internal ProjectTemplate(IProjectsProvider projectsProvider, string description, string projectTemplateFilePath, IProject project, IProjectPathUtil projectPathUtil, IProjectTemplateRepository repository, IUser currentUser, IDirectory directoryWrapper)
			: base(projectsProvider, projectPathUtil)
		{
			_templateRepository = repository;
			_directoryWrapper = directoryWrapper;
			Project project2 = project as Project;
			_templateRepository.Load(projectTemplateFilePath, base.ProjectsProvider.LocalDataFolder, description, project2.Repository.SettingsBundleGuid, currentUser, false);
			_templateRepository.UpdateFromProject(project, project2.Repository);
			SetAnalysisBands(((IProjectConfiguration)project).AnalysisBands);
			_templateRepository.CopyProjectSettingsAndLanguageDirections((IProjectTemplate)(object)this, project);
			KeepServerAndLocation();
			ResetPublicationAndSyncSettings();
			_templateRepository.UpdateCascadeItems((IRelativePathManager)(object)project2);
			base.TermbaseConfiguration = ((ICopyable<IProjectTermbaseConfiguration>)(object)((IProjectConfiguration)project).TermbaseConfiguration).Copy();
			_templateRepository.UpdateTermbasePaths(project);
			ProjectTemplateSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectTemplateSettings>();
			settingsGroup.ProjectLocation.Value = Directory.GetParent(project.LocalDataFolder).FullName;
			((IProjectConfigurationRepository)_templateRepository).SetInitialComplexTaskTemplate(((IProjectConfiguration)project).StartTaskTemplate);
			AddGeneralProjectInfoSettings((IObjectWithSettings)(object)this, project);
			Save();
		}

		internal ProjectTemplate(IProjectsProvider projectsProvider, string projectTemplateFilePath, IProjectPathUtil projectPathUtil, IProjectTemplateRepository repository, IDirectory directoryWrapper)
			: base(projectsProvider, projectPathUtil)
		{
			_templateRepository = repository;
			_directoryWrapper = directoryWrapper;
			_templateRepository.Load(projectTemplateFilePath, base.ProjectsProvider.LocalDataFolder);
		}

		private void AddGeneralProjectInfoSettings(IObjectWithSettings projectTemplate, IProject project)
		{
			GeneralProjectInfoSettings settingsGroup = projectTemplate.Settings.GetSettingsGroup<GeneralProjectInfoSettings>();
			settingsGroup.CreatedAt.Value = DateTime.UtcNow;
			settingsGroup.CreatedBy.Value = base.ProjectsProvider.UserProvider.CurrentUser.UserId;
			Setting<string> customerId = settingsGroup.CustomerId;
			ICustomer customer = project.Customer;
			customerId.Value = ((customer != null) ? customer.Name : null);
			settingsGroup.Description.Value = Description;
			settingsGroup.DueDate.Value = project.DueDate;
		}

		private void ResetPublicationAndSyncSettings()
		{
			PublishProjectOperationSettings settingsGroup = base.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
			((AbstractSettingsGroupBase)settingsGroup).Reset();
			ProjectSyncSettings settingsGroup2 = base.Settings.GetSettingsGroup<ProjectSyncSettings>();
			((AbstractSettingsGroupBase)settingsGroup2).Reset();
		}

		private void KeepServerAndLocation()
		{
			PublishProjectOperationSettings settingsGroup = base.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
			ProjectPlanningSettings settingsGroup2 = base.Settings.GetSettingsGroup<ProjectPlanningSettings>();
			Setting<string> serverUri = settingsGroup.ServerUri;
			settingsGroup2.ServerUri.Value = (string.IsNullOrEmpty(Setting<string>.op_Implicit(serverUri)) ? string.Empty : AdjustSchema(Setting<string>.op_Implicit(serverUri)));
			settingsGroup2.OrganizationPath.Value = Setting<string>.op_Implicit(settingsGroup.OrganizationPath);
		}

		public bool IsCloudTemplate()
		{
			ProjectSettings settingsGroup = base.Settings.GetSettingsGroup<ProjectSettings>();
			return string.Equals(settingsGroup.ProjectOrigin.Value, "LC project");
		}

		private static string AdjustSchema(string server)
		{
			bool flag = server.Contains("https://");
			return new UriBuilder(server)
			{
				Scheme = (flag ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)
			}.ToString();
		}

		public void Delete()
		{
			base.ProjectsProvider.RemoveProjectTemplate((IProjectTemplate)(object)this);
			if (Directory.Exists(ProjectTemplateFilePath))
			{
				File.Delete(ProjectTemplateFilePath);
			}
		}

		public override void Check()
		{
			if (!_templateRepository.IsInitialized)
			{
				_templateRepository.Load(ProjectTemplateFilePath, base.ProjectsProvider.LocalDataFolder);
			}
		}

		public override void Save()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				string directoryName = Path.GetDirectoryName(ProjectTemplateFilePath);
				if (!_directoryWrapper.Exists(directoryName))
				{
					_directoryWrapper.CreateDirectory(directoryName);
				}
				OnBeforeSerialization();
				_templateRepository.Save(ProjectTemplateFilePath);
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.ProjectTemplate_ErrorSerializing, Path.GetFileNameWithoutExtension(ProjectTemplateFilePath), ProjectTemplateFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		public override void DiscardCachedData()
		{
			if (File.Exists(FilePath))
			{
				base.DiscardCachedData();
				((IProjectConfigurationRepository)_templateRepository).DiscardData();
			}
		}

		public void SaveAs(string projectTemplateFilePath)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				_templateRepository.ProjectTemplateFilePath = base.PathUtil.MakeRelativePath(projectTemplateFilePath, base.ProjectsProvider.LocalDataFolder);
				Save();
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.ProjectTemplate_ErrorSerializing, Path.GetFileNameWithoutExtension(ProjectTemplateFilePath), ProjectTemplateFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		public void UpdateFromProject(IProject p)
		{
			Project project = (Project)(object)p;
			ResetLazyObjects();
			base.Settings = ((IObjectWithSettings)p).Settings;
			_templateRepository.UpdateFromProject((IProject)(object)project, project.Repository);
			SetAnalysisBands(project.AnalysisBands);
			ILanguageDirection[] languageDirections = project.LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				LanguageDirection languageDirection = (LanguageDirection)(object)GetLanguageDirection(val.SourceLanguage, val.TargetLanguage);
				if (languageDirection == null)
				{
					languageDirection = (LanguageDirection)(object)AddLanguageDirection(val.SourceLanguage, val.TargetLanguage);
				}
				languageDirection.InitializeSettingsFromConfiguration((IProjectConfiguration)(object)project, copyTms: true);
				CopyAutoSuggestDictionaries(val, (ILanguageDirection)(object)languageDirection);
			}
			_templateRepository.UpdateCascadeItems((IRelativePathManager)(object)project);
			((IProjectConfigurationRepository)_templateRepository).SetInitialComplexTaskTemplate(project.StartTaskTemplate);
			base.TermbaseConfiguration = ((ICopyable<IProjectTermbaseConfiguration>)(object)project.TermbaseConfiguration).Copy();
			KeepServerAndLocation();
			ResetPublicationAndSyncSettings();
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTemplate projectTemplate)
			{
				return projectTemplate.Guid == Guid;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}
	}
}
