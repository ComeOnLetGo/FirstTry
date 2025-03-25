using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Activations
	{
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("activation")]
		public List<Activation> Activation { get; set; }
	}
}
