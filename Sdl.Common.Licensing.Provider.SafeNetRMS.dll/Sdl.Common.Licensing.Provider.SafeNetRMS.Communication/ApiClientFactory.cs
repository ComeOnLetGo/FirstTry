using System;
using Microsoft.Extensions.Logging;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Communication
{
	internal class ApiClientFactory : IApiClientFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		private readonly ILicenseServerURIHandler _licenseServerURIHandler;

		public ApiClientFactory(ILicenseServerURIHandler licenseServerURIHandler, ILoggerFactory logger)
		{
			_loggerFactory = logger;
			_licenseServerURIHandler = licenseServerURIHandler;
		}

		public IThalesRestClient GetRestClient(string provider, string code)
		{
			ILogger<ApiClientFactory> val = LoggerFactoryExtensions.CreateLogger<ApiClientFactory>(_loggerFactory);
			try
			{
				Uri uri = _licenseServerURIHandler.UpdateActivationServerData(provider);
				LoggerExtensions.LogDebug((ILogger)(object)val, "EMS server URL retireved : " + uri, Array.Empty<object>());
				return new ThalesRestClient(uri, code, (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ThalesRestClient>(_loggerFactory));
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError((ILogger)(object)val, ex, "Unable to get EMS server URL ", Array.Empty<object>());
			}
			return null;
		}
	}
}
