using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class RevokeProofs
	{
		[JsonProperty("revokeProof")]
		public string RevokeProof { get; set; }
	}
}
