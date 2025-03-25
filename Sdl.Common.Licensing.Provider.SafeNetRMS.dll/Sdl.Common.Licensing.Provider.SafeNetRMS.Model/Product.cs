using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Product
	{
		[JsonProperty("externalId")]
		public string ExternalId { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		[JsonProperty("productType")]
		public string ProductType { get; set; }

		[JsonProperty("nameVersion")]
		public NameVersion NameVersion { get; set; }
	}
}
