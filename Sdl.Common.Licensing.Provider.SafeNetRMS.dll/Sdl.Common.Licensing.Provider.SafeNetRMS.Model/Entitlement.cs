using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Entitlement
	{
		[JsonProperty("eId")]
		public string EId { get; set; }

		[JsonProperty("externalId")]
		public string ExternalId { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("revocationAllowed")]
		public bool RevocationAllowed { get; set; }

		[JsonProperty("customAttributes")]
		public CustomAttributes CustomAttributes { get; set; }
	}
}
