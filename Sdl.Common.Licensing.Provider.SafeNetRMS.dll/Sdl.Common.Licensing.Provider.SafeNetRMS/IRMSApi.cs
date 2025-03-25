using System.Collections.Generic;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal interface IRMSApi
	{
		void InstallLicense(string licenseString);

		string InstallRevocationRequest(string permissionTicket);

		string GetLockingCode(RmsLockSelectors entLockSelector = RmsLockSelectors.VLS_LOCK_DISK_ID | RmsLockSelectors.VLS_LOCK_HOSTNAME);

		void CleanupLicensingResources();

		IReadOnlyCollection<FeatureInfo> GetFeatures();

		IReadOnlyCollection<FeatureInfo> GetFeaturesFromServer(string serverName);

		void ReturnCommuterLicense(string name, string version, int lockCriteria);

		void CheckoutCommuterLicense(string name, string version, int duration, int lockCriteria);

		ILoginSession CheckOutFeature(LicenseFeature feature);

		LicenseServerStatus IsServerRunning();
	}
}
