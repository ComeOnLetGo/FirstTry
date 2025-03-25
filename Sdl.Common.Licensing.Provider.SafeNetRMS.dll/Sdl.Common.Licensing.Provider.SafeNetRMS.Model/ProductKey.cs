using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ProductKey
	{
		[JsonProperty("pkId")]
		public string PkId { get; set; }

		[JsonProperty("creationDate")]
		public string CreationDate { get; set; }

		[JsonProperty("lastModifiedDate")]
		public string LastModifiedDate { get; set; }

		[JsonProperty("startDate")]
		public string StartDate { get; set; }

		[JsonProperty("expiry")]
		public Expiry Expiry { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("enforcement")]
		public Enforcement Enforcement { get; set; }

		[JsonProperty("totalQuantity")]
		public int TotalQuantity { get; set; }

		[JsonProperty("availableQuantity")]
		public int AvailableQuantity { get; set; }

		[JsonProperty("splittedQuantity")]
		public int SplittedQuantity { get; set; }

		[JsonProperty("activationMethod")]
		public string ActivationMethod { get; set; }

		[JsonProperty("fixedQuantity")]
		public int FixedQuantity { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("item")]
		public Item Item { get; set; }

		[JsonProperty("commonLicenseAttributes")]
		public CommonLicenseAttributes CommonLicenseAttributes { get; set; }

		[JsonProperty("activationAttributes")]
		public ActivationAttributes ActivationAttributes { get; set; }

		[JsonProperty("customAttributes")]
		public CustomAttributes CustomAttributes { get; set; }

		[JsonProperty("productKeyFingerprints")]
		public object ProductKeyFingerprints { get; set; }
	}
}
