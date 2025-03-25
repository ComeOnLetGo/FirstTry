using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class PermissionTickets
	{
		[JsonProperty("permissionTicket")]
		public string PermissionTicket { get; set; }
	}
}
