using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class LicenseKey
	{
		[JsonProperty("keyGenTechnology")]
		public string KeyGenTechnology { get; set; }

		[JsonProperty("keyFormat")]
		public string KeyFormat { get; set; }

		[JsonProperty("keyType")]
		public string KeyType { get; set; }

		[JsonProperty("keyFileName")]
		public string KeyFileName { get; set; }

		[JsonProperty("keyFileDescription")]
		public string KeyFileDescription { get; set; }

		[JsonProperty("isWWU")]
		public bool IsWWU { get; set; }

		[JsonProperty("keyEncodingType")]
		public string KeyEncodingType { get; set; }

		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("licenseKeyRef")]
		public int LicenseKeyRef { get; set; }

		[JsonProperty("keyResultantStateRef")]
		public object KeyResultantStateRef { get; set; }
	}
}
