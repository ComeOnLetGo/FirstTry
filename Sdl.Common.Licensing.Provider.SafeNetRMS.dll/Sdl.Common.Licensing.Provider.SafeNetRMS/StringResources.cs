using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class StringResources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("Sdl.Common.Licensing.Provider.SafeNetRMS.StringResources", typeof(StringResources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string BorrowLicenseControl_BorrowLicenseFailed => ResourceManager.GetString("BorrowLicenseControl_BorrowLicenseFailed", resourceCulture);

		internal static string Error_CreatePermissionTicketFailed => ResourceManager.GetString("Error_CreatePermissionTicketFailed", resourceCulture);

		internal static string Error_FailedToContactLicenseServer => ResourceManager.GetString("Error_FailedToContactLicenseServer", resourceCulture);

		internal static string Error_GetFeaturesFailed => ResourceManager.GetString("Error_GetFeaturesFailed", resourceCulture);

		internal static string Error_InvalidActivationResponse => ResourceManager.GetString("Error_InvalidActivationResponse", resourceCulture);

		internal static string Error_NetworkLicenseNotSupportedInStandaloneProduct => ResourceManager.GetString("Error_NetworkLicenseNotSupportedInStandaloneProduct", resourceCulture);

		internal static string Error_SubmitRevocationRequestFailed => ResourceManager.GetString("Error_SubmitRevocationRequestFailed", resourceCulture);

		internal static string Error_TrialAidMissing => ResourceManager.GetString("Error_TrialAidMissing", resourceCulture);

		internal static string SafeNet_CannotRetrieveLockingCriteria => ResourceManager.GetString("SafeNet_CannotRetrieveLockingCriteria", resourceCulture);

		internal static string SafeNet_CheckInCommuterFeatureFailed => ResourceManager.GetString("SafeNet_CheckInCommuterFeatureFailed", resourceCulture);

		internal static string SafeNet_CheckOutCommuterFeatureFailed => ResourceManager.GetString("SafeNet_CheckOutCommuterFeatureFailed", resourceCulture);

		internal static string SafeNet_EMS_NotEnoughLicensesAvailable => ResourceManager.GetString("SafeNet_EMS_NotEnoughLicensesAvailable", resourceCulture);

		internal static string SafeNet_FailedActivation => ResourceManager.GetString("SafeNet_FailedActivation", resourceCulture);

		internal static string SafeNet_FailedLicenseInstallation => ResourceManager.GetString("SafeNet_FailedLicenseInstallation", resourceCulture);

		internal static string SafeNet_FailedLicensingResourcesCleanup => ResourceManager.GetString("SafeNet_FailedLicensingResourcesCleanup", resourceCulture);

		internal static string SafeNet_FailedLogin => ResourceManager.GetString("SafeNet_FailedLogin", resourceCulture);

		internal static string SafeNet_FailedRMSCreation => ResourceManager.GetString("SafeNet_FailedRMSCreation", resourceCulture);

		internal static string SafeNet_FailedRMSLicenseStringInstallation => ResourceManager.GetString("SafeNet_FailedRMSLicenseStringInstallation", resourceCulture);

		internal static string SafeNet_FailedtoCheckInFeature => ResourceManager.GetString("SafeNet_FailedtoCheckInFeature", resourceCulture);

		internal static string SafeNet_FailedToCheckOutFeature => ResourceManager.GetString("SafeNet_FailedToCheckOutFeature", resourceCulture);

		internal static string SafeNet_FailedToConnectToOOS => ResourceManager.GetString("SafeNet_FailedToConnectToOOS", resourceCulture);

		internal static string SafeNet_FailedToGetActivationId => ResourceManager.GetString("SafeNet_FailedToGetActivationId", resourceCulture);

		internal static string SafeNet_FailedToGetLicenseInfo => ResourceManager.GetString("SafeNet_FailedToGetLicenseInfo", resourceCulture);

		internal static string SafeNet_FeatureNotSupported => ResourceManager.GetString("SafeNet_FeatureNotSupported", resourceCulture);

		internal static string SafeNet_InvalidResponseFromOOS => ResourceManager.GetString("SafeNet_InvalidResponseFromOOS", resourceCulture);

		internal static string SafeNet_KeyNotValidForProduct => ResourceManager.GetString("SafeNet_KeyNotValidForProduct", resourceCulture);

		internal static string SafeNet_LicenseAlreadyRevoked => ResourceManager.GetString("SafeNet_LicenseAlreadyRevoked", resourceCulture);

		internal static string SafeNet_RevokeFailed => ResourceManager.GetString("SafeNet_RevokeFailed", resourceCulture);

		internal static string SafeNet_RevokeFailedByTicket => ResourceManager.GetString("SafeNet_RevokeFailedByTicket", resourceCulture);

		internal static string SafeNet_ServerConnectionError => ResourceManager.GetString("SafeNet_ServerConnectionError", resourceCulture);

		internal static string SafeNet_ServerConnectionError_CheckAddress => ResourceManager.GetString("SafeNet_ServerConnectionError_CheckAddress", resourceCulture);

		internal static string SafeNet_ServerNotRunning => ResourceManager.GetString("SafeNet_ServerNotRunning", resourceCulture);

		internal StringResources()
		{
		}
	}
}
