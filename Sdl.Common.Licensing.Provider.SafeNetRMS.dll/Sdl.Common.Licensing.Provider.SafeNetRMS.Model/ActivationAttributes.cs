using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationAttributes
	{
		[JsonProperty("activationAttribute")]
		public List<ActivationAttribute> ActivationAttribute { get; set; }
	}
}
