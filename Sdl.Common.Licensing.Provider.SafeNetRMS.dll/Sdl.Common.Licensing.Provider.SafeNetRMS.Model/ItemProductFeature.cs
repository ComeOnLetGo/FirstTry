using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ItemProductFeature
	{
		[JsonProperty("feature")]
		public Feature Feature { get; set; }

		[JsonProperty("itemFeatureLicenseModel")]
		public ItemFeatureLicenseModel ItemFeatureLicenseModel { get; set; }

		[JsonProperty("itemFeatureState")]
		public string ItemFeatureState { get; set; }

		[JsonProperty("SAOT")]
		public string SAOT { get; set; }
	}
}
