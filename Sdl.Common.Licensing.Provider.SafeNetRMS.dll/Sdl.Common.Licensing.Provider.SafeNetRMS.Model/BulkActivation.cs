using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class BulkActivation
	{
		[JsonProperty("activationProductKeys")]
		public ActivationProductKeys ActivationProductKeys { get; set; }

		[JsonProperty("activationAttributes")]
		public ActivationAttributes ActivationAttributes { get; set; }
	}
}
