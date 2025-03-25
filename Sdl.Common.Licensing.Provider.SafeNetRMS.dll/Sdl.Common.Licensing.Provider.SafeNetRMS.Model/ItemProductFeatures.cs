using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ItemProductFeatures
	{
		[JsonProperty("itemProductFeature")]
		public List<ItemProductFeature> ItemProductFeature { get; set; }
	}
}
