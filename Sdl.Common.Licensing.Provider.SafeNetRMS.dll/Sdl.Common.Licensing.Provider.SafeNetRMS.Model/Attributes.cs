using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Attributes
	{
		[JsonProperty("attribute")]
		public List<Attribute> Attribute { get; set; }
	}
}
