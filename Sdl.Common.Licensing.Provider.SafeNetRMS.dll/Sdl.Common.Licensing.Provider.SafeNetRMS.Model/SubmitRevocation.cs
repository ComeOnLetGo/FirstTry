using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class SubmitRevocation
	{
		[JsonProperty("revokeProofs")]
		public RevokeProofs RevokeProofs { get; set; }
	}
}
