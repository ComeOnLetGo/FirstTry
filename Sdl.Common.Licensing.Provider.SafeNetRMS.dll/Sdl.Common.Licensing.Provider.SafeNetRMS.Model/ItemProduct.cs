using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ItemProduct
	{
		[JsonProperty("product")]
		public Product Product { get; set; }

		[JsonProperty("itemProductFeatures")]
		public ItemProductFeatures ItemProductFeatures { get; set; }
	}
}
