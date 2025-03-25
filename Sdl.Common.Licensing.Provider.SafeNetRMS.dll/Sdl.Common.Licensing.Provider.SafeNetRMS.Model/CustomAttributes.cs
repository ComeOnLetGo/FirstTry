using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class CustomAttributes
	{
		[JsonProperty("customAttribute")]
		public List<CustomAttribute> CustomAttribute { get; set; }
	}
}
