using Sdl.Common.Licensing.Provider.Core;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers
{
	public static class LicenseCheckWithoutUsingNetworkSeat
	{
		private static IProductLicense ProductLicense => PerpetualLicense.GetLicenseWithoutConsumingSeatsOrUsages();

		public static bool IsLicensed => ProductLicense?.IsAuthorised() ?? false;

		public static bool IsProfessional => ProductLicense?.IsProfessional() ?? false;

		public static bool IsFreelance => ProductLicense?.IsFreelance() ?? false;

		public static bool IsExpress => ProductLicense?.IsExpress() ?? false;

		public static bool IsWorkGroup => ProductLicense?.IsWorkGroup() ?? false;

		public static bool IsNotLicensed => ProductLicense?.IsNotAuthorised() ?? false;

		public static bool IsTrial => ProductLicense?.IsTrial() ?? false;

		public static bool IsFeatureActive(string feature)
		{
			return ProductLicense?.IsFeatureActive(feature) ?? false;
		}

		public static bool IsStarter(bool includeExpiredStarter = false)
		{
			return ProductLicense?.IsStarter(includeExpiredStarter) ?? false;
		}
	}
}
