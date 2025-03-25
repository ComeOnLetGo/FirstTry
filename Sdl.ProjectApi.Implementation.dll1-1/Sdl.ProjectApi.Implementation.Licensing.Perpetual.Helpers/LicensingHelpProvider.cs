using Sdl.Common.Licensing.Provider.Core;
using Sdl.Desktop.Platform.Services;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers
{
	public class LicensingHelpProvider : ILicensingHelpProvider
	{
		private const string LicensingService_LicenseStatusScreen = "LicensingService_LicenseStatusScreen";

		private const string LicensingService_AlternativeActivationOptions = "LicensingService_AlternativeActivationOptions";

		private const string LicensingService_ConnectionSettings = "LicensingService_ConnectionSettings";

		private const string LicensingService_OfflineActivation = "LicensingService_OfflineActivation";

		private const string LicensingService_OfflineDeactivation = "LicensingService_OfflineDeactivation";

		private const string LicensingService_OnlineActivation = "LicensingService_OnlineActivation";

		private const string LicensingService_OnlineDeactivation = "LicensingService_OnlineDeactivation";

		private const string LicensingService_ViewDeactivationCertificate = "LicensingService_ViewDeactivationCertificate";

		private const string LicensingService_LicenseServerConfiguration = "LicensingService_LicenseServerConfiguration";

		private const string LicensingService_BorrowLicense = "LicensingService_BorrowLicense";

		private const string LicensingService_BorrowLicenseStatusScreen = "LicensingService_BorrowLicenseStatusScreen";

		private const string LicensingService_ConnectingToServer = "LicensingService_ConnectingToServer";

		public void ShowHelp(LicensingHelpIDs licensingHelpID)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected I4, but got Unknown
			string text = null;
			switch (licensingHelpID - 1)
			{
			case 1:
				text = "LicensingService_AlternativeActivationOptions";
				break;
			case 2:
				text = "LicensingService_ConnectionSettings";
				break;
			case 0:
				text = "LicensingService_LicenseStatusScreen";
				break;
			case 3:
				text = "LicensingService_OfflineActivation";
				break;
			case 4:
				text = "LicensingService_OfflineDeactivation";
				break;
			case 5:
				text = "LicensingService_OnlineActivation";
				break;
			case 6:
				text = "LicensingService_OnlineDeactivation";
				break;
			case 7:
				text = "LicensingService_ViewDeactivationCertificate";
				break;
			case 9:
				text = "LicensingService_BorrowLicense";
				break;
			case 10:
				text = "LicensingService_BorrowLicenseStatusScreen";
				break;
			case 8:
				text = "LicensingService_LicenseServerConfiguration";
				break;
			case 11:
				text = "LicensingService_ConnectingToServer";
				break;
			}
			if (text != null)
			{
				GlobalServices.HelpService.ShowContextHelp(text);
			}
		}
	}
}
