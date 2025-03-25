using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class PermissionTicketResponse
	{
		[JsonProperty("revocation")]
		public Revocation Revocation { get; set; }
	}
}
