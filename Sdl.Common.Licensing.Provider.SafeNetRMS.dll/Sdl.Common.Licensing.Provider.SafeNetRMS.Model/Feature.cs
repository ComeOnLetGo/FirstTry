using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Feature
	{
		[JsonProperty("externalId")]
		public string ExternalId { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("identifier")]
		public int Identifier { get; set; }

		[JsonProperty("nameVersion")]
		public NameVersion NameVersion { get; set; }
	}
}
