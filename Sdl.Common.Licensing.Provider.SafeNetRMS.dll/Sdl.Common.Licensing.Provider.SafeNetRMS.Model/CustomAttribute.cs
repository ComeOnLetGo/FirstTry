using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class CustomAttribute
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
