using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class CommonLicenseAttribute
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("displayText")]
		public string DisplayText { get; set; }

		[JsonProperty("encodeToBase64")]
		public bool? EncodeToBase64 { get; set; }
	}
}
