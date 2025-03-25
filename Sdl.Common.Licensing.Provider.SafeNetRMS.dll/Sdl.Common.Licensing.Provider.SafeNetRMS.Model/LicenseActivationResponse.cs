using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class LicenseActivationResponse
	{
		[JsonProperty("activations")]
		public Activations Activations { get; set; }

		[JsonProperty("errorResponse")]
		public ErrorResponse Error { get; set; }
	}
}
