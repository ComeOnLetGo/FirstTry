using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Customer
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		[JsonProperty("externalId")]
		public string ExternalId { get; set; }
	}
}
