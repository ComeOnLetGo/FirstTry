using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationProductKeys
	{
		[JsonProperty("activationProductKey")]
		public IEnumerable<BulkActivationProductKey> ActivationProductKey { get; set; }
	}
}
