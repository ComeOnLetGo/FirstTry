using System;
using Microsoft.Extensions.Logging;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Communication;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	[LicensingProviderFactory(Id = "SafeNetRMSLicensingProviderFactory", Name = "SafeNet RMS Licensing Provider", Description = "The SafeNet RMS Licensing Provider Factory")]
	public class SafeNetRMSLicensingProviderFactory : ILicensingProviderFactory
	{
		public string ProviderId => SafeNetRMSProviderConfiguration.DefaultLicensingProviderId;

		public bool IsActivationCode(string activationCode)
		{
			Guid result;
			return Guid.TryParse(activationCode, out result);
		}

		public ILicensingProvider CreateLicensingProvider(ILicensingProviderConfiguration config, ILoggerFactory loggerFactory)
		{
			if (!(config is SafeNetRMSProviderConfiguration safeNetRMSProviderConfiguration))
			{
				throw new ArgumentException("This licensing provider factory does not support the provided configuration.", "config");
			}
			ApiClientFactory apiClientFactory = new ApiClientFactory(new LicenseServerURIHandler(), loggerFactory);
			UnifiedAPIProvider rmsApi = new UnifiedAPIProvider(safeNetRMSProviderConfiguration.LicenseFilePath, safeNetRMSProviderConfiguration.NetworkLicensingServerName, loggerFactory, new AppConfigWrapper(), safeNetRMSProviderConfiguration);
			return (ILicensingProvider)(object)new LicensingProvider(safeNetRMSProviderConfiguration, apiClientFactory, rmsApi, new FileWrapper(), loggerFactory);
		}
	}
}
