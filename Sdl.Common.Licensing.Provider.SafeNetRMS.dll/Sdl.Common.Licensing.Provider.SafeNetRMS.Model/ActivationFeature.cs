using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationFeature
	{
		[JsonProperty("feature")]
		public Feature Feature { get; set; }

		[JsonProperty("activationLicenseModel")]
		public ActivationLicenseModel ActivationLicenseModel { get; set; }

		[JsonProperty("licenseKeyRef")]
		public int LicenseKeyRef { get; set; }
	}
}
