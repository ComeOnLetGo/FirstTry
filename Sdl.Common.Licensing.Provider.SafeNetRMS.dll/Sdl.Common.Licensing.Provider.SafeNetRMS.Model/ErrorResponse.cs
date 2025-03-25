using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ErrorResponse
	{
		[JsonProperty("errorCode")]
		public int ErrorCode { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("developerMessage")]
		public string DeveloperMessage { get; set; }
	}
}
