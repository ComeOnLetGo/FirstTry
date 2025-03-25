using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationProductKey
	{
		[JsonProperty("itemId")]
		public string ItemId { get; set; }

		[JsonProperty("pkId")]
		public string PkId { get; set; }

		[JsonProperty("activationItem")]
		public ActivationItem ActivationItem { get; set; }
	}
}
