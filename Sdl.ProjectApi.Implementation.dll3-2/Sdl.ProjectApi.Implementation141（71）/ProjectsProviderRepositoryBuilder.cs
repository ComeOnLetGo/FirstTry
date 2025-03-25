using Microsoft.Extensions.Logging;
using Sdl.BestMatchServiceStudioIntegration.Common;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.LanguageCloud;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Interfaces;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectsProviderRepositoryBuilder
	{
		private readonly IApplication _application;

		private readonly IProjectPathUtil _pathUtil;

		private readonly ITelemetryService _telemetryService;

		private readonly IEventAggregator _eventAggregator;

		private readonly ILogger<ProjectsProviderRepository> _logger;

		public ProjectsProviderRepositoryBuilder(IApplication application, IProjectPathUtil pathUtil, ITelemetryService telemetryService, IEventAggregator eventAggregator)
		{
			_application = application;
			_pathUtil = pathUtil;
			_telemetryService = telemetryService;
			_eventAggregator = eventAggregator;
			_logger = LogProvider.GetLogger<ProjectsProviderRepository>();
		}

		public IProjectsProviderRepository Build(ProjectsProviderInfo providerInfo, IMainRepository mainRepository, ILanguageCloudService languageCloudService)
		{
			ProjectFilePathValidator projFilePaths = new ProjectFilePathValidator(_pathUtil, providerInfo.LocalDataFolder);
			ProjectsProviderRepository projectsProviderRepository = new ProjectsProviderRepository(_pathUtil, _application, new ProjectRepositoryFactoryStrategy(Constants.SupportedProjectRepositoryFactories), mainRepository, (IProjectFilePathValidator)(object)projFilePaths, _eventAggregator, _logger);
			if (languageCloudService != null)
			{
				return (IProjectsProviderRepository)(object)new ProjectsProviderRepositoryLC((IProjectsProviderRepository)(object)projectsProviderRepository, (IAccountServicesCache)(object)new AccountServicesCache(languageCloudService, (ILogger)(object)LoggerFactoryExtensions.CreateLogger<IAccountServicesCache>(LogProvider.GetLoggerFactory())), languageCloudService, (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ProjectsProviderRepositoryLC>(LogProvider.GetLoggerFactory()), _eventAggregator, _pathUtil);
			}
			return (IProjectsProviderRepository)(object)projectsProviderRepository;
		}
	}
}
