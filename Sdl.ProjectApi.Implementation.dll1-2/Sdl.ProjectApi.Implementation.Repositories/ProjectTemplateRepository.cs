using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectTemplateRepository : AbstractProjectConfigurationRepository, IProjectTemplateRepository, IProjectConfigurationRepository
	{
		private Sdl.ProjectApi.Implementation.Xml.ProjectTemplate _lazyXmlProjectTemplate;

		private readonly IProjectPathUtil _pathUtil;

		private CascadeItemUpdater _cascadeUpdater;

		protected override ProjectConfiguration XmlConfiguration => _lazyXmlProjectTemplate;

		public Guid ProjectTemplateGuid => ProjectTemplateListItem.Guid;

		public ProjectTemplateListItem ProjectTemplateListItem { get; private set; }

		public string Description
		{
			get
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				return new LocalizableString(ProjectTemplateListItem.ProjectTemplateInfo.Description).Content;
			}
			set
			{
				ProjectTemplateListItem.ProjectTemplateInfo.Description = value;
			}
		}

		public string ProjectTemplateFilePath
		{
			get
			{
				return ProjectTemplateListItem.ProjectTemplateFilePath;
			}
			set
			{
				ProjectTemplateListItem.ProjectTemplateFilePath = value;
			}
		}

		public ProjectTemplateRepository(IApplication application, IProjectPathUtil pathUtil)
			: base(application)
		{
			_pathUtil = pathUtil;
		}

		public ProjectTemplateRepository(IApplication application, IProjectPathUtil pathUtil, ProjectTemplateListItem projectTemplateListItem)
			: base(application)
		{
			_pathUtil = pathUtil;
			ProjectTemplateListItem = projectTemplateListItem;
		}

		public void Load(string projectTemplateFilePath, string localDataFolder)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				VersionUtil.CheckProjectTemplateVersion(projectTemplateFilePath, Application.ServerEvents);
				_lazyXmlProjectTemplate = Sdl.ProjectApi.Implementation.Xml.ProjectTemplate.Deserialize(projectTemplateFilePath);
				MarkAsInitialized();
				ProjectTemplateListItem = GetProjectTemplateListItem(projectTemplateFilePath, localDataFolder);
			}
			catch (InvalidVersionException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.ProjectTemplate_ErrorDeserializing, "", projectTemplateFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		public void Load(string projectTemplateFilePath, string localDataFolder, string description, Guid settingsBundleGuid, IUser user, bool useDefaultFilterConfiguration)
		{
			Guid guid = Guid.NewGuid();
			_lazyXmlProjectTemplate = new Sdl.ProjectApi.Implementation.Xml.ProjectTemplate();
			_lazyXmlProjectTemplate.Guid = guid;
			_lazyXmlProjectTemplate.Version = "4.0.0.0";
			_lazyXmlProjectTemplate.SettingsBundleGuid = settingsBundleGuid;
			_lazyXmlProjectTemplate.GeneralProjectTemplateInfo = new GeneralProjectTemplateInfo();
			_lazyXmlProjectTemplate.GeneralProjectTemplateInfo.CreatedAt = DateTime.UtcNow;
			_lazyXmlProjectTemplate.GeneralProjectTemplateInfo.CreatedBy = user.UserId;
			_lazyXmlProjectTemplate.GeneralProjectTemplateInfo.Description = description;
			AddUser(user);
			if (useDefaultFilterConfiguration)
			{
				_lazyXmlProjectTemplate.FilterConfiguration = null;
			}
			MarkAsInitialized();
			ProjectTemplateListItem = GetProjectTemplateListItem(projectTemplateFilePath, localDataFolder);
		}

		public void UpdateFromProject(IProject project, IProjectConfigurationRepository projectConfigurationRepository)
		{
			Project pathManager = project as Project;
			_lazyXmlProjectTemplate.LanguageResourceFilePath = projectConfigurationRepository.LanguageResourceFilePath;
			if (((IProjectConfiguration)project).CascadeItem != null)
			{
				SetCascadeItem((IRelativePathManager)(object)pathManager, ((IProjectConfiguration)project).CascadeItem.Copy());
			}
		}

		public void UpdateTermbasePaths(IProject project)
		{
			Project project2 = project as Project;
			foreach (Termbase termbasis in _lazyXmlProjectTemplate.TermbaseConfiguration.Termbases)
			{
				termbasis.SettingsXml = project2?.GetAbsoluteSettingsXml(termbasis.SettingsXml);
			}
		}

		public void UpdateCascadeItems(IRelativePathManager projectPathManager)
		{
			if (_cascadeUpdater == null)
			{
				_cascadeUpdater = new CascadeItemUpdater(_lazyXmlProjectTemplate);
			}
			_cascadeUpdater.UpdateCascadeItems(projectPathManager);
		}

		public void Save(string projectTemplateFilePath)
		{
			base.SettingsBundles.Save();
			_lazyXmlProjectTemplate.Serialize(projectTemplateFilePath);
		}

		public override void DiscardData()
		{
			_lazyXmlProjectTemplate = null;
			base.DiscardData();
		}

		private ProjectTemplateListItem GetProjectTemplateListItem(string projectTemplateFilePath, string localDataFolder)
		{
			return new ProjectTemplateListItem
			{
				Guid = _lazyXmlProjectTemplate.Guid,
				ProjectTemplateFilePath = _pathUtil.MakeRelativePath(projectTemplateFilePath, localDataFolder),
				ProjectTemplateInfo = _lazyXmlProjectTemplate.GeneralProjectTemplateInfo
			};
		}

		public void AssignNewGuid()
		{
			_lazyXmlProjectTemplate.AssignNewGuid();
			ProjectTemplateListItem.Guid = _lazyXmlProjectTemplate.Guid;
		}

		public void CopyProjectSettingsAndLanguageDirections(IProjectTemplate toProjectTemplate, IProject fromProject)
		{
			Dictionary<Guid, Guid> dictionary = new Dictionary<Guid, Guid>();
			Project project = fromProject as Project;
			IProjectRepository projectRepository = project.ProjectRepository;
			List<ILanguageDirection> languageDirections = ((IProjectConfigurationRepository)projectRepository).GetLanguageDirections((IProjectConfiguration)(object)fromProject);
			foreach (ILanguageDirection item in languageDirections)
			{
				LanguageDirection languageDirection = AddLanguageDirection((IProjectConfiguration)(object)toProjectTemplate, item.SourceLanguage, item.TargetLanguage) as LanguageDirection;
				dictionary.Add(item.SettingsBundleGuid, languageDirection.XmlLanguageDirection.SettingsBundleGuid);
			}
			List<Guid> list = new List<Guid> { ((IProjectConfigurationRepository)projectRepository).SettingsBundleGuid };
			list.AddRange(projectRepository.LanguageDirectionSettingsGuids);
			base.SettingsBundles = ((IProjectConfigurationRepository)projectRepository).SettingsBundles.Copy(list, dictionary);
		}
	}
}
