using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.Core.UI;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Services.Licensing;
using Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual
{
	public class PerpetualLicense : IPerpetualLicense, IFeatureBasedLicense, ISupportTerminalServices
	{
		private readonly ILicensingHelpProvider _licensingHelpProvider;

		private readonly ITelemetrySupport _telemetrySupport;

		private readonly IDomainExecutionChecker _domainExecutionChecker;

		private static readonly Lazy<ApplicationLicenseManager> AppLicenseManagerLazy = new Lazy<ApplicationLicenseManager>(CreateAppLicenseManager, LazyThreadSafetyMode.ExecutionAndPublication);

		[CompilerGenerated]
		private bool _003CIsTerminalServicesEnabled_003Ek__BackingField;

		private ApplicationLicenseManager ApplicationLicenseManager => AppLicenseManagerLazy.Value;

		private IProductLicense CurrentProductLicense => ApplicationLicenseManager.GetCurrentProductLicense();

		public string LicenseDescription => "Professional Perpetual License";

		public bool Authorised => true;

		public bool NotAuthorised => false;

		public bool NotAuthorisedOrConnectedToServer => false;

		public bool IsBorrowed => false;

		private bool IsLoggedIn => true;

		public bool IsLocal => true;

		public bool IsTrial => false;

		public bool IsStarterLicenseInExpiryPeriod => false;

		public bool IsTimeLimitedLicenseInExpiryPeriod => false;

		public int? MaxTargetLanguages => CurrentProductLicense.GetMaxTargetLanguages();

		public int? MaxTranslationUnits => CurrentProductLicense.GetMaxTranslationUnits();

		public bool AllowTerminalServices => true;

		public bool IsTerminalServicesEnabled
		{
			[CompilerGenerated]
			get
			{
				return true;
			}
			[CompilerGenerated]
			set
			{
				_003CIsTerminalServicesEnabled_003Ek__BackingField = value;
			}
		}

		private static ApplicationLicenseManager CreateAppLicenseManager()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			return new ApplicationLicenseManager((IApplicationLicensingProviderConfiguration)(object)new StudioAppLicensingProviderConfiguration(new LicenseFilesHelper()), LogProvider.GetLoggerFactory());
		}

		public PerpetualLicense(ILicensingHelpProvider licensingHelpProvider, IDomainExecutionChecker domainExecutionChecker, ITelemetrySupport telemetrySupport)
		{
			_licensingHelpProvider = licensingHelpProvider;
			_domainExecutionChecker = domainExecutionChecker;
			_telemetrySupport = telemetrySupport;
		}

		public void Initialize()
		{
			try
			{
				AppLicenseManagerLazy.Value.SetTelemetrySupport(_telemetrySupport);
				CurrentProductLicense.CheckOut();
			}
			catch
			{
			}
		}

		public void Uninitialize()
		{
			try
			{
				if (IsLoggedIn)
				{
					CurrentProductLicense.CheckIn();
				}
				AppLicenseManagerLazy.Value.DisposeLicesingProviders();
			}
			catch
			{
			}
		}

		public LicensingFormConfiguration GetLicensingFormConfiguration()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			LicensingFormConfiguration val = new LicensingFormConfiguration();
			val.IsLicenseValidForMachine = new IsLicenseValidForMachineDelegate(IsLicenseValidForMachine);
			val.LicenseInfoProvider = new LicenseInfoProviderDelegate(GetLicenseSummaryInfo);
			val.StatusPageTitle = StringResources.PerpetualLicense_Title;
			val.StatusPageSubtitle = StringResources.PerpetualLicense_Description;
			val.PurchaseLinkUrl = StringResources.PerpetualLicense_PurchaseLicenseURL;
			val.MyAccountLinkUrl = "https://oos.sdl.com/asp/products/ssl/account/mylicenses/mylicenses.aspx";
			val.LicensingHelpProvider = _licensingHelpProvider;
			return val;
		}

		private static string GetLicenseSummaryInfo(IProductLicense license)
		{
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Invalid comparison between Unknown and I4
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Invalid comparison between Unknown and I4
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Invalid comparison between Unknown and I4
			if (license.IsUnlocked())
			{
				string text = (dynamic)license.GetProperty("CustomerName");
				if (!string.IsNullOrEmpty(text) && !text.StartsWith("<Add"))
				{
					return string.Format(StringResources.PerpetualLicense_ProfessionalLicenseWithCustomerName_Description, text);
				}
				return StringResources.PerpetualLicense_ProfessionalLicenseWithEULA_Description;
			}
			if (license.IsTrial())
			{
				return StringResources.PerpetualLicense_TrialLicense_Description;
			}
			if (license.IsProfessional())
			{
				return StringResources.PerpetualLicense_ProfessionalLicense_Description;
			}
			if (license.IsFreelance())
			{
				return StringResources.PerpetualLicense_FreelanceLicense_Description;
			}
			if (license.IsStarter())
			{
				return StringResources.PerpetualLicense_StarterLicense_Description;
			}
			if (license.IsExpress())
			{
				return StringResources.PerpetualLicense_ExpressLicense_Description;
			}
			if (license.IsWorkGroup())
			{
				return StringResources.PerpetualLicense_WorkGroupLicense_Description;
			}
			if ((int)license.Status == 2)
			{
				return StringResources.PerpetualLicense_TrialExpired_Description;
			}
			if ((int)license.Status == 3)
			{
				return StringResources.PerpetualLicense_LeaseExpired_Description;
			}
			if ((int)license.Status == 4)
			{
				return StringResources.PerpetualLicense_BorrowedLicenseExpired_Description;
			}
			return null;
		}

		private bool IsLicenseValidForMachine(IProductLicense license, out string shortStatus)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return CurrentProductLicense.GetHashCode();
		}

		public ILicensingDialog GetLicensingDialog(LicensingFormConfiguration formConfiguration, IList<CustomStatusPage> customPages = null)
		{
			return ApplicationLicenseManager.GetLicensingDialog(formConfiguration, customPages);
		}

		public bool IsFeatureActive(string feature)
		{
			return true;
		}

		internal static IProductLicense GetLicenseWithoutConsumingSeatsOrUsages()
		{
			return AppLicenseManagerLazy.Value.GetLicenseWithoutConsumingSeatsOrUsages();
		}

		public bool ShouldShowLicensingDialog(bool showAtStartupForStarter)
		{
			return false;
		}
	}
}
