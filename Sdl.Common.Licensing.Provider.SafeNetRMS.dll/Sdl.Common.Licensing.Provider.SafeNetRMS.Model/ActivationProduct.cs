using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationProduct
	{
		[JsonProperty("product")]
		public Product Product { get; set; }

		[JsonProperty("variantIdentifier")]
		public object VariantIdentifier { get; set; }

		[JsonProperty("activationFeatures")]
		public ActivationFeatures ActivationFeatures { get; set; }
	}
}
