using System;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Interfaces;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectsProviderBuilder
	{
		private readonly IApplication _application;

		private readonly IEventAggregator _eventAggregator;

		private readonly IProjectTemplateRepositoryFactory _projectTemplateRepositoryFactory;

		private readonly ILoggerFactory _loggerFactory;

		private readonly IProjectPathUtil _pathUtil;

		private readonly ITelemetryService _telemetryService;

		public ProjectsProviderBuilder(IApplication application, IProjectPathUtil pathUtil, ITelemetryService telemetryService, IEventAggregator eventAggregator, IProjectTemplateRepositoryFactory projectTemplateRepositoryFactory, ILoggerFactory loggerFactory)
		{
			_pathUtil = pathUtil;
			_telemetryService = telemetryService;
			_application = application;
			_eventAggregator = eventAggregator;
			_projectTemplateRepositoryFactory = projectTemplateRepositoryFactory;
			_loggerFactory = loggerFactory;
		}

		public IProjectsProvider Build(ProjectsProviderInfo providerInfo, IMainRepository mainRepository, IProjectsProviderRepository repository, IProjectOperation operationComposite)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			IUserProvider userProvider = CreateUserProviderAndSetCurrentUser(providerInfo.UserId, mainRepository);
			CustomerProviderRepository customerProviderRepository = new CustomerProviderRepository(mainRepository);
			CustomerProvider customerProvider = new CustomerProvider((ICustomerProviderRepository)(object)customerProviderRepository);
			ProjectTemplateProviderRepository projectTemplatesProviderRepository = new ProjectTemplateProviderRepository(mainRepository, _pathUtil, _application, _projectTemplateRepositoryFactory, (IFile)new FileWrapper());
			WorkflowProviderRepository workflowProviderRepository = new WorkflowProviderRepository(mainRepository);
			ProjectsOriginCache projectsOriginCache = new ProjectsOriginCache(_eventAggregator);
			return (IProjectsProvider)(object)new ProjectsProvider(_application, repository, (IProjectTemplatesProviderRepository)(object)projectTemplatesProviderRepository, _projectTemplateRepositoryFactory, (IWorkflowProviderRepository)(object)workflowProviderRepository, providerInfo.LocalDataFolder, _pathUtil, userProvider, (ICustomerProvider)(object)customerProvider, operationComposite, _telemetryService, _eventAggregator, (IProjectsOriginCache)(object)projectsOriginCache, (IDirectory)new DirectoryWrapper(), _loggerFactory);
		}

		public IUserProvider CreateUserProviderAndSetCurrentUser(string userId, IMainRepository repository)
		{
			if (string.IsNullOrEmpty(userId))
			{
				throw new ArgumentNullException("userId");
			}
			UserProviderRepository userProviderRepository = new UserProviderRepository(repository);
			UserProvider userProvider = new UserProvider((IUserProviderRepository)(object)userProviderRepository, new UserRegistryStorage(LogProvider.GetLogger<UserRegistryStorage>()));
			userProvider.CurrentUser = ((string.Compare(userId, UserHelper.WindowsUserId, ignoreCase: true) == 0) ? userProvider.CreateCurrentWindowsUser() : userProvider.CreateOrUpdateUser((IUser)(object)new User(userId)));
			return (IUserProvider)(object)userProvider;
		}
	}
}
