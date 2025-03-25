using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class GetProductKeyResponse
	{
		[JsonProperty("productKey")]
		public ProductKey ProductKey { get; set; }
	}
}
