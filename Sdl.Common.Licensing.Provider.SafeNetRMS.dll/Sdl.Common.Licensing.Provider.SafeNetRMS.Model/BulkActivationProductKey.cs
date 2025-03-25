using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class BulkActivationProductKey
	{
		[JsonProperty("activationQuantity")]
		public int ActivationQuantity { get; set; }

		[JsonProperty("pkId")]
		public string PkId { get; set; }
	}
}
