using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationItem
	{
		[JsonProperty("activationProduct")]
		public ActivationProduct ActivationProduct { get; set; }
	}
}
