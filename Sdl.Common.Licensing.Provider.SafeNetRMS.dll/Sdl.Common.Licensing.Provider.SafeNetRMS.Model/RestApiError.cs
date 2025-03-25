using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class RestApiError
	{
		[JsonProperty("errorResponse")]
		public ErrorResponse Response { get; set; }
	}
}
