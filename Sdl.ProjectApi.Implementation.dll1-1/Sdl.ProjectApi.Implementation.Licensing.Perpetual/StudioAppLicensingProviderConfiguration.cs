using System.Collections.Generic;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.SafeNetRMS;
using Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers;
using Sdl.Versioning;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual
{
	internal sealed class StudioAppLicensingProviderConfiguration : IApplicationLicensingProviderConfiguration
	{
		public ILicensingProviderConfiguration Configuration { get; }

		public bool EnableServerLicensing => true;

		public string PreferredProviderId => "SafeNetEMS";

		public StudioAppLicensingProviderConfiguration(ILicenseFilesHelper licenseFilesHelper)
		{
			licenseFilesHelper.AddTrialLicense(((SafeNetRMSProviderConfiguration)(object)(Configuration = (ILicensingProviderConfiguration)(object)CreateSafeNetRmsProviderConfiguration(licenseFilesHelper))).LicenseFilePath);
		}

		private string GetLicenseFilePath(string filePath)
		{
			return VersionedPaths.RetailProgramDataPath + filePath;
		}

		private SafeNetRMSProviderConfiguration CreateSafeNetRmsProviderConfiguration(ILicenseFilesHelper licenseFilesHelper)
		{
			return new SafeNetRMSProviderConfiguration("SOFTWARE\\Trados\\Studio17License\\")
			{
				Name = "Studio SafeNet RMS configuration",
				LicenseFilePath = GetLicenseFilePath("\\Data\\Studio.lic"),
				LicenseCommuterFilePath = GetLicenseFilePath("\\Data\\StudioCommuter.lic"),
				LicenseCommuterDefinition = licenseFilesHelper.GetCommuterLicenseDefinition(),
				ProductFeatureMapper = CreateStudioToSafeNetRmsFeatureMap(),
				ProductVersion = "2022",
				LicenseTypeMapper = CreateSafeNetRmsLicenseTypeMapper(),
				AvailableEditions = GetAvailableEditions(),
				TrialActivationId = licenseFilesHelper.GetTrialActivationId()
			};
		}

		private static ProductToLicenseProviderFeatureMap<string> CreateStudioToSafeNetRmsFeatureMap()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			ProductToLicenseProviderFeatureMap<string> obj = new ProductToLicenseProviderFeatureMap<string>();
			((Dictionary<ProductFeature, string>)(object)obj).Add(new ProductFeature(26, PerpetualLicenseFeature.AllowPerfectMatch.ToString()), "AllowPerfectMatch");
			((Dictionary<ProductFeature, string>)(object)obj).Add(new ProductFeature(32, PerpetualLicenseFeature.AllowAutoSuggestCreation.ToString()), "AllowAutoSuggestCreation");
			((Dictionary<ProductFeature, string>)(object)obj).Add(new ProductFeature(35, PerpetualLicenseFeature.AllowTQA.ToString()), "AllowTQA");
			((Dictionary<ProductFeature, string>)(object)obj).Add(new ProductFeature(36, PerpetualLicenseFeature.AllowStudio2021.ToString()), "AllowStudio2021");
			return obj;
		}

		private static ILicenseTypeMapper CreateSafeNetRmsLicenseTypeMapper()
		{
			LicenseTypeToFeatureMap<string> obj = new LicenseTypeToFeatureMap<string>();
			((Dictionary<int, string>)(object)obj).Add(8, "ProfessionalEdition");
			((Dictionary<int, string>)(object)obj).Add(16, "WorkgroupEdition");
			((Dictionary<int, string>)(object)obj).Add(4, "FreelanceEdition");
			((Dictionary<int, string>)(object)obj).Add(1, "ExpressEdition");
			((Dictionary<int, string>)(object)obj).Add(2, "StarterEdition");
			return (ILicenseTypeMapper)(object)new SingleFeatureLicenseTypeMapper<string>(obj);
		}

		private static List<LicenseEdition> GetAvailableEditions()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			return new List<LicenseEdition>
			{
				new LicenseEdition
				{
					Id = "ProfessionalEdition",
					DisplayName = StringResources.PerpetualLicense_ProfessionalLicense_Description
				},
				new LicenseEdition
				{
					Id = "WorkgroupEdition",
					DisplayName = StringResources.PerpetualLicense_WorkGroupLicense_Description
				},
				new LicenseEdition
				{
					Id = "ExpressEdition",
					DisplayName = StringResources.PerpetualLicense_ExpressLicense_Description
				}
			};
		}
	}
}
