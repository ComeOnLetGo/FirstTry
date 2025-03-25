using System.Collections.Generic;
using Sdl.Common.Licensing.Provider.Core;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	public class SafeNetRMSProviderConfiguration : ILicensingProviderConfiguration
	{
		public static string DefaultLicensingProviderId = "SafeNetRMS";

		private readonly ILicenseRegistryAccess _registry;

		public string ProviderId => DefaultLicensingProviderId;

		public string Name { get; set; }

		public string ProductName { get; set; }

		public List<LicenseEdition> AvailableEditions { get; set; }

		public string LicenseCommuterFilePath { get; set; }

		public string LicenseCommuterDefinition { get; set; }

		public string DeactivationCode
		{
			get
			{
				return _registry.DeactivationCode;
			}
			set
			{
				_registry.DeactivationCode = value;
			}
		}

		public string LicenseCode
		{
			get
			{
				return _registry.LicenseCode;
			}
			set
			{
				_registry.LicenseCode = value;
			}
		}

		public string LicenseServer
		{
			get
			{
				return _registry.LicenseServer;
			}
			set
			{
				_registry.LicenseServer = value;
			}
		}

		public bool UseLicenseServer
		{
			get
			{
				return _registry.UseLicenseServer;
			}
			set
			{
				_registry.UseLicenseServer = value;
			}
		}

		public bool UseBorrowedLicense
		{
			get
			{
				return _registry.UseBorrowedLicense;
			}
			set
			{
				_registry.UseBorrowedLicense = value;
			}
		}

		public string CheckedOutEdition
		{
			get
			{
				return _registry.CheckedOutEdition;
			}
			set
			{
				_registry.CheckedOutEdition = value;
			}
		}

		public string LicenseFilePath { get; set; }

		public ProductToLicenseProviderFeatureMap<string> ProductFeatureMapper { get; set; }

		public string ProductVersion { get; set; }

		public ILicenseTypeMapper LicenseTypeMapper { get; set; }

		public string TrialActivationId { get; set; }

		public string NetworkLicensingServerName { get; set; }

		public SafeNetRMSProviderConfiguration(string registryPath)
		{
			_registry = (ILicenseRegistryAccess)(object)new SafeNetRMSRegistryAccess(registryPath);
		}

		public SafeNetRMSProviderConfiguration(ILicenseRegistryAccess licenseRegistryAccess)
		{
			_registry = licenseRegistryAccess;
		}

		public bool CanUpdateLicenseServerName()
		{
			return _registry.CanUpdateLicenseServerName();
		}
	}
}
