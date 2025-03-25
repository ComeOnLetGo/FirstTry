using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Expiry
	{
		[JsonProperty("neverExpires")]
		public bool NeverExpires { get; set; }

		[JsonProperty("endDate")]
		public string EndDate { get; set; }
	}
}
