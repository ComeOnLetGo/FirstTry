using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class NameVersion
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }
	}
}
