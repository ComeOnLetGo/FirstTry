using Microsoft.Extensions.Logging;
using Sdl.BestMatchServiceStudioIntegration.Common;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Services;

namespace Sdl.ProjectApi.Implementation
{
	public class Factory : IApplicationFactory
	{
		public IApplication CreateApplication(ILanguageCloudService languageCloudService = null)
		{
			ILoggerFactory loggerFactory = LogProvider.GetLoggerFactory();
			ILogger<IApplication> log = LoggerFactoryExtensions.CreateLogger<IApplication>(loggerFactory);
			ApplicationRepository repository = new ApplicationRepository((ILogger)(object)log);
			ITelemetryService telemetryService = default(ITelemetryService);
			bool flag = GlobalServices.Context.TryGetService<ITelemetryService>(ref telemetryService);
			IEventAggregator eventAggregator = default(IEventAggregator);
			bool flag2 = GlobalServices.Context.TryGetService<IEventAggregator>(ref eventAggregator);
			return (IApplication)(object)new Application((IApplicationRepository)(object)repository, telemetryService, languageCloudService, eventAggregator, loggerFactory);
		}
	}
}
