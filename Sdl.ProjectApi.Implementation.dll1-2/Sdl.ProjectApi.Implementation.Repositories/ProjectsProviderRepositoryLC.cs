using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.BestMatchService.Common.Identity;
using Sdl.BestMatchServiceStudioIntegration.Common;
using Sdl.BestMatchServiceStudioIntegration.Common.Utils;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.LanguageCloud;
using Sdl.ProjectApi.Implementation.ProjectSettings;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Interfaces;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectsProviderRepositoryLC : IProjectsProviderRepository
	{
		private readonly IProjectsProviderRepository _localProjectsProviderRepository;

		private readonly IAccountServicesCache _accountServicesCache;

		private readonly ILanguageCloudService _languageCloudService;

		private List<IProject> _localProjects;

		private readonly ILogger _log;

		private readonly IEventAggregator _eventAggregator;

		private readonly IProjectPathUtil _projectPathUtil;

		public ProjectsProviderRepositoryLC(IProjectsProviderRepository localProjectsProviderRepository, IAccountServicesCache accountServicesCache, ILanguageCloudService languageCloudService, ILogger log, IEventAggregator eventAggregator, IProjectPathUtil projectPathUtil)
		{
			_localProjectsProviderRepository = localProjectsProviderRepository;
			_accountServicesCache = accountServicesCache;
			_languageCloudService = languageCloudService;
			_log = log;
			_eventAggregator = eventAggregator;
			_projectPathUtil = projectPathUtil;
		}

		public List<IProject> GetProjects(IProjectsProvider projectsProvider, string localDataFolder, IProjectOperation projectOperation)
		{
			List<IProject> localProjects = GetLocalProjects(projectsProvider, localDataFolder, projectOperation);
			try
			{
				List<IProject> cloudProjects = GetCloudProjects(projectsProvider, projectOperation);
				GetCloudCompletedProjects(cloudProjects, localProjects, projectsProvider, projectOperation);
				return MergeProjects(cloudProjects, localProjects);
			}
			catch (AggregateException ex)
			{
				List<AccountException> list = new List<AccountException>();
				foreach (Exception innerException in ex.InnerExceptions)
				{
					list.Add(new AccountException
					{
						AccountName = innerException.Message,
						Exception = innerException.InnerException
					});
				}
				string text = "Failed to load Cloud projects";
				LoggerExtensions.LogError(_log, (Exception)ex, text, Array.Empty<object>());
				throw new CloudProjectsLoadException(text, ex, localProjects, list);
			}
		}

		private void GetCloudCompletedProjects(List<IProject> cloudProjects, List<IProject> localProjects, IProjectsProvider projectsProvider, IProjectOperation projectOperation)
		{
			if (localProjects == null || !localProjects.Any((IProject p) => p.IsCloudBased || p.IsLCProject) || !_languageCloudService.IsLoggedIn())
			{
				return;
			}
			IEnumerable<IProject> first = localProjects.Where((IProject p) => p.IsCloudBased || p.IsLCProject);
			List<IProject> cloudCompletedProjects = first.Except(cloudProjects).ToList();
			List<IAccount> accountsToQueryFor = GetAccountsToQueryFor();
			Dictionary<string, string[]> dictionary = accountsToQueryFor.ToDictionary((IAccount g) => g.Id, (IAccount g) => (from p in cloudCompletedProjects
				where p.AccountId == g.Id
				select p.Guid.ToString()).ToArray());
			try
			{
				List<Project> projects = _accountServicesCache.GetProjects(dictionary);
				cloudProjects.AddRange((IEnumerable<IProject>)projects.Select((Project project) => CreateProjectFromCloud(project, projectsProvider, projectOperation)));
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError(_log, ex, "Failed to load the projects completed in Cloud", Array.Empty<object>());
			}
		}

		internal List<IProject> MergeProjects(List<IProject> cloudProjects, List<IProject> localProjects)
		{
			if (localProjects == null)
			{
				return null;
			}
			IEnumerable<IProject> removedItems = localProjects.Where((IProject p) => p.IsCloudBased && (int)p.ProjectType == 11);
			localProjects.RemoveAll((IProject p) => removedItems.Contains(p));
			IEnumerable<IProject> collection = cloudProjects.Where((IProject virtualProject) => !localProjects.Any((IProject localProject) => virtualProject.Guid == localProject.Guid));
			localProjects.AddRange(collection);
			IEnumerable<IProject> enumerable = from localProject in localProjects
				where cloudProjects.Any((IProject virtualProject) => localProject.Guid == virtualProject.Guid)
				select localProject into p
				where p.IsLCProject && (int)p.ProjectType == 8
				select p;
			foreach (IProject localProject2 in enumerable)
			{
				IProject source = cloudProjects.Single((IProject project) => project.Guid == localProject2.Guid);
				UpdateProjectInfo(localProject2, source);
			}
			return localProjects;
		}

		public void Save(List<IProject> projects, string localDataFolder)
		{
			_localProjectsProviderRepository.Save(projects, localDataFolder);
		}

		public List<IProject> GetLocalProjects(IProjectsProvider projectsProvider, string localDataFolder, IProjectOperation projectOperation)
		{
			List<IProject> localProjects = _localProjects;
			if (localProjects == null || localProjects.Count <= 0)
			{
				return _localProjects = _localProjectsProviderRepository.GetProjects(projectsProvider, localDataFolder, projectOperation);
			}
			return _localProjects;
		}

		public List<IProject> GetCloudProjects(IProjectsProvider projectsProvider, IProjectOperation projectOperation)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			List<IProject> list = new List<IProject>();
			if (!_languageCloudService.IsLoggedIn())
			{
				ILanguageCloudService languageCloudService = _languageCloudService;
				LoginInformation val = default(LoginInformation);
				((LoginInformation)(ref val)).ShowSubscriptionDialog = false;
				((LoginInformation)(ref val)).ShowLoginDialog = false;
				if (!languageCloudService.EnsureLoggedIn(val))
				{
					return list;
				}
			}
			List<IAccount> accountsToQueryFor = GetAccountsToQueryFor();
			List<Project> projectsInParallel = _accountServicesCache.GetProjectsInParallel(accountsToQueryFor);
			foreach (Project item in projectsInParallel)
			{
				list.Add((IProject)(object)CreateProjectFromCloud(item, projectsProvider, projectOperation));
			}
			return list;
		}

		private void UpdateProjectInfo(IProject destination, IProject source)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			destination.ChangeProjectName(source.Name);
			destination.Description = source.Description;
			destination.DueDate = source.DueDate;
			destination.UpdateStatus(source.Status);
			((IProjectConfiguration)destination).Save();
		}

		private Project CreateProjectFromCloud(Project cloudProject, IProjectsProvider projectsProvider, IProjectOperation projectOperation)
		{
			OnlineProjectRepository onlineProjectRepository = new OnlineProjectRepository(cloudProject, _projectPathUtil);
			ISettingsBundle val = SettingsUtil.CreateSettingsBundle((ISettingsBundle)null);
			val.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().AccountId = cloudProject.AccountId;
			val.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().TenantName = GetTenantAccountName(cloudProject);
			XmlDocument xmlDocument = new XmlDocument();
			XmlNodeWriter xmlNodeWriter = new XmlNodeWriter(xmlDocument, clearCurrentContents: true);
			SettingsUtil.SerializeSettingsBundle((XmlWriter)xmlNodeWriter, val);
			return new Project(projectsProvider, new ProjectListItem
			{
				ProjectFilePath = string.Empty,
				ProjectInfo = onlineProjectRepository.ProjectInfo,
				Guid = onlineProjectRepository.ProjectGuid,
				SettingsBundle = new SettingsBundle
				{
					Any = xmlDocument.DocumentElement,
					Guid = onlineProjectRepository.SettingsBundleGuid
				}
			}, onlineProjectRepository, projectOperation, _eventAggregator);
		}

		private string GetTenantAccountName(Project cloudProject)
		{
			if (string.IsNullOrEmpty(cloudProject.AccountId))
			{
				return string.Empty;
			}
			IList<IAccount> accounts = _languageCloudService.LanguageCloudCredential.Accounts;
			object obj;
			if (accounts == null)
			{
				obj = null;
			}
			else
			{
				IAccount obj2 = accounts.FirstOrDefault((IAccount c) => c.Id.Equals(cloudProject.AccountId));
				obj = ((obj2 != null) ? obj2.DecodedName : null);
			}
			if (obj == null)
			{
				obj = string.Empty;
			}
			return (string)obj;
		}

		private List<IAccount> GetAccountsToQueryFor()
		{
			if (!ShowProjectsForThisAccountOnly())
			{
				return _languageCloudService.LanguageCloudCredential.GetFullyLoggedAccounts();
			}
			return new List<IAccount> { _languageCloudService.LanguageCloudCredential.Accounts.SingleOrDefault((IAccount a) => a.Id.Equals(_languageCloudService.ApiContext.SelectedTenantId)) };
		}

		private bool ShowProjectsForThisAccountOnly()
		{
			ISettingsGroup settingsGroup = GlobalServices.UserSettingsService.UserSettings.GetSettingsGroup("LanguageCloudAccountUserSettings");
			return settingsGroup.GetSetting<bool>("ShowProjectsForThisAccountOnly", false).Value;
		}

		public IProject LoadNewProject(string projectFilePath, IProjectsProvider projectsProvider, IProjectOperation projectOperation)
		{
			return _localProjectsProviderRepository.LoadNewProject(projectFilePath, projectsProvider, projectOperation);
		}
	}
}
