using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class SubmitRevocationRequest
	{
		[JsonProperty("revocation")]
		public SubmitRevocation Revocation { get; set; }

		public SubmitRevocationRequest()
		{
		}

		public SubmitRevocationRequest(string revocationTicket)
		{
			Revocation = new SubmitRevocation
			{
				RevokeProofs = new RevokeProofs
				{
					RevokeProof = revocationTicket
				}
			};
		}
	}
}
