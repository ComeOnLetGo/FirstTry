using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class AssociatedAttribute
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("readOnly")]
		public bool ReadOnly { get; set; }

		[JsonProperty("mandatory")]
		public bool Mandatory { get; set; }
	}
}
