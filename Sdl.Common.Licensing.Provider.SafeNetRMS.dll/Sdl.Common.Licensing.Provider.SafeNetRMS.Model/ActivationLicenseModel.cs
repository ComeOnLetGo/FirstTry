using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationLicenseModel
	{
		[JsonProperty("licenseModel")]
		public LicenseModel LicenseModel { get; set; }

		[JsonProperty("attributes")]
		public Attributes Attributes { get; set; }
	}
}
