using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sdl.BestMatchServiceStudioIntegration.Common;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.Services;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Operations;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Interfaces;
using Sdl.ProjectApi.Server;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation
{
	public class Application : IApplication
	{
		private IPackageImportEvents _lazyProjectPackageImportEvents;

		private IServerEvents _lazyServerEvents;

		private readonly IApplicationRepository _applicationRepository;

		private readonly ITelemetryService _telemetryService;

		private readonly ILanguageCloudService _languageCloudService;

		private readonly IEventAggregator _eventAggregator;

		private readonly ILoggerFactory _loggerFactory;

		private ISettingsBundle _settingsBundle;

		private readonly IProjectPathUtil _pathUtil;

		private ITranslationProviderCredentialStore _translationProviderCredentialStore;

		private ITerminologyProviderCredentialStore _terminologyProviderCredentialStore;

		private IProjectOperation _basicOperationComposite;

		private ICommuteClientManager _commuteClientManager;

		private IBackgroundProjectUpdater _backgroundProjectUpdater;

		private readonly Dictionary<string, IProjectsProviderRepository> _repositoryCache = new Dictionary<string, IProjectsProviderRepository>();

		public ITranslationProviderCache DefaultTranslationProviderCache { get; set; }

		public ITranslationProviderCredentialStore TranslationProviderCredentialStore
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Expected O, but got Unknown
				//IL_0017: Expected O, but got Unknown
				ITranslationProviderCredentialStore obj = _translationProviderCredentialStore;
				if (obj == null)
				{
					TranslationProviderCredentialStore val = new TranslationProviderCredentialStore();
					ITranslationProviderCredentialStore val2 = (ITranslationProviderCredentialStore)val;
					_translationProviderCredentialStore = (ITranslationProviderCredentialStore)val;
					obj = val2;
				}
				return obj;
			}
			set
			{
				_translationProviderCredentialStore = value;
			}
		}

		public ITerminologyProviderCredentialStore TerminologyProviderCredentialStore
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Expected O, but got Unknown
				//IL_0017: Expected O, but got Unknown
				ITerminologyProviderCredentialStore obj = _terminologyProviderCredentialStore;
				if (obj == null)
				{
					TerminologyProviderCredentialStore val = new TerminologyProviderCredentialStore();
					ITerminologyProviderCredentialStore val2 = (ITerminologyProviderCredentialStore)val;
					_terminologyProviderCredentialStore = (ITerminologyProviderCredentialStore)val;
					obj = val2;
				}
				return obj;
			}
			set
			{
				_terminologyProviderCredentialStore = value;
			}
		}

		public ICommuteClientManager CommuteClientManager
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Expected O, but got Unknown
				//IL_0017: Expected O, but got Unknown
				ICommuteClientManager obj = _commuteClientManager;
				if (obj == null)
				{
					CommuteClientManager val = new CommuteClientManager();
					ICommuteClientManager val2 = (ICommuteClientManager)val;
					_commuteClientManager = (ICommuteClientManager)val;
					obj = val2;
				}
				return obj;
			}
			set
			{
				_commuteClientManager = value;
			}
		}

		public IBackgroundProjectUpdater BackgroundProjectUpdater
		{
			get
			{
				return _backgroundProjectUpdater ?? (_backgroundProjectUpdater = (IBackgroundProjectUpdater)(object)new DefaultBackgroundProjectUpdater());
			}
			set
			{
				_backgroundProjectUpdater = value;
			}
		}

		public List<ProjectsProviderInfo> AllProjectsProviders => _applicationRepository.GetAllProviders();

		public IProjectOperation BasicProjectOperationComposite => _basicOperationComposite ?? (_basicOperationComposite = (IProjectOperation)(object)new BasicOperationComposite((IApplication)(object)this, PathUtil));

		public IPackageImportEvents PackageImportEvents
		{
			get
			{
				return _lazyProjectPackageImportEvents ?? (_lazyProjectPackageImportEvents = (IPackageImportEvents)(object)new DefaultProjectPackageImportEvents());
			}
			set
			{
				_lazyProjectPackageImportEvents = value ?? throw new ArgumentNullException();
			}
		}

		public IServerEvents ServerEvents
		{
			get
			{
				return _lazyServerEvents ?? (_lazyServerEvents = (IServerEvents)(object)new DefaultServerEvents());
			}
			set
			{
				_lazyServerEvents = value ?? throw new ArgumentNullException();
			}
		}

		public ISettingsBundle SettingsBundle
		{
			get
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				return _settingsBundle ?? throw new ProjectApiException("Settings not set.");
			}
			set
			{
				_settingsBundle = value;
			}
		}

		public IEncryptionKeyProvider EncryptionKeyProvider { get; set; }

		public IProjectPathUtil PathUtil => _pathUtil;

		internal Application(IApplicationRepository repository, ITelemetryService telemetryService, ILanguageCloudService languageCloudService, IEventAggregator eventAggregator, ILoggerFactory loggerFactory)
		{
			_applicationRepository = repository;
			_telemetryService = telemetryService;
			_languageCloudService = languageCloudService;
			_eventAggregator = eventAggregator;
			_loggerFactory = loggerFactory;
			_pathUtil = (IProjectPathUtil)(object)new ProjectPathUtil();
		}

		public IProjectsProvider GetProjectsProvider(ProjectsProviderInfo providerInfo, IProjectOperation operationComposite)
		{
			IProjectsProvider result = CreateProjectsProvider(providerInfo, operationComposite);
			if (!_applicationRepository.Exists(providerInfo.LocalDataFolder))
			{
				_applicationRepository.AddProjectsProvider(providerInfo);
			}
			return result;
		}

		public IProjectsProvider CreateProjectsProvider(ProjectsProviderInfo providerInfo, IProjectOperation operationComposite)
		{
			ProjectsProviderBuilder projectsProviderBuilder = new ProjectsProviderBuilder((IApplication)(object)this, PathUtil, _telemetryService, _eventAggregator, new ProjectTemplateRepositoryFactory(), _loggerFactory);
			ILogger<MainRepository> log = LoggerFactoryExtensions.CreateLogger<MainRepository>(_loggerFactory);
			MainRepository mainRepository = new MainRepository(log, providerInfo.LocalDataFolder);
			IProjectsProviderRepository projectsProviderRepository = GetProjectsProviderRepository(providerInfo, PathUtil, mainRepository);
			return projectsProviderBuilder.Build(providerInfo, mainRepository, projectsProviderRepository, operationComposite);
		}

		private IProjectsProviderRepository GetProjectsProviderRepository(ProjectsProviderInfo providerInfo, IProjectPathUtil projectPathUtil, IMainRepository mainRepository)
		{
			string key = ((object)providerInfo).ToString();
			ProjectsProviderRepositoryBuilder projectsProviderRepositoryBuilder = new ProjectsProviderRepositoryBuilder((IApplication)(object)this, projectPathUtil, _telemetryService, _eventAggregator);
			IProjectsProviderRepository val = (_repositoryCache.ContainsKey(key) ? _repositoryCache[key] : projectsProviderRepositoryBuilder.Build(providerInfo, mainRepository, _languageCloudService));
			if (!_repositoryCache.ContainsKey(key))
			{
				_repositoryCache.Add(key, val);
			}
			return val;
		}

		public void RemoveProjectsProvider(IProjectsProvider projectsProvider)
		{
			if (projectsProvider != null)
			{
				_applicationRepository.RemoveProjectsProvider(projectsProvider.LocalDataFolder);
			}
		}

		public void Save()
		{
			_applicationRepository.Save();
		}
	}
}
