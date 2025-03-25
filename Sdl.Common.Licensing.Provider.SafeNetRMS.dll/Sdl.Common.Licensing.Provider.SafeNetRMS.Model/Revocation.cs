using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Revocation
	{
		[JsonProperty("permissionTickets")]
		public PermissionTickets PermissionTickets { get; set; }
	}
}
