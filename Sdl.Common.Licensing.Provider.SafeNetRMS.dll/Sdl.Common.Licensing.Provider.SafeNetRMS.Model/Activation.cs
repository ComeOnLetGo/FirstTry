using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Activation
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("creationDate")]
		public string CreationDate { get; set; }

		[JsonProperty("lastModifiedDate")]
		public string LastModifiedDate { get; set; }

		[JsonProperty("aId")]
		public string AId { get; set; }

		[JsonProperty("externalId")]
		public string ExternalId { get; set; }

		[JsonProperty("groupActivationId")]
		public string GroupActivationId { get; set; }

		[JsonProperty("activationComments")]
		public string ActivationComments { get; set; }

		[JsonProperty("activationQuantity")]
		public int ActivationQuantity { get; set; }

		[JsonProperty("timeZoneId")]
		public string TimeZoneId { get; set; }

		[JsonProperty("activationDate")]
		public string ActivationDate { get; set; }

		[JsonProperty("sendNotification")]
		public bool SendNotification { get; set; }

		[JsonProperty("activatorEmailId")]
		public string ActivatorEmailId { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("marketGroup")]
		public MarketGroup MarketGroup { get; set; }

		[JsonProperty("revocation")]
		public object Revocation { get; set; }

		[JsonProperty("activatees")]
		public object Activatees { get; set; }

		[JsonProperty("activationProductKey")]
		public ActivationProductKey ActivationProductKey { get; set; }

		[JsonProperty("activationAttributes")]
		public ActivationAttributes ActivationAttributes { get; set; }

		[JsonProperty("fingerprint")]
		public object Fingerprint { get; set; }

		[JsonProperty("licenseKeys")]
		public LicenseKeys LicenseKeys { get; set; }

		[JsonProperty("resultantStates")]
		public ResultantStates ResultantStates { get; set; }

		[JsonProperty("downloads")]
		public object Downloads { get; set; }

		[JsonProperty("customAttributes")]
		public object CustomAttributes { get; set; }

		[JsonProperty("entitlement")]
		public Entitlement Entitlement { get; set; }

		[JsonProperty("customer")]
		public Customer Customer { get; set; }

		[JsonProperty("enforcement")]
		public Enforcement Enforcement { get; set; }
	}
}
