using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ActivationFeatures
	{
		[JsonProperty("activationFeature")]
		public List<ActivationFeature> ActivationFeature { get; set; }
	}
}
