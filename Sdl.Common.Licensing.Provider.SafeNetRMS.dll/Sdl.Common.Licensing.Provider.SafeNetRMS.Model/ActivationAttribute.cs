using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationAttribute
	{
		[JsonProperty("groupName")]
		public string GroupName { get; set; }

		[JsonProperty("subGroupName")]
		public string SubGroupName { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("readOnly")]
		public bool ReadOnly { get; set; }

		[JsonProperty("mandatory")]
		public bool Mandatory { get; set; }

		[JsonProperty("associatedAttribute")]
		public AssociatedAttribute AssociatedAttribute { get; set; }
	}
}
