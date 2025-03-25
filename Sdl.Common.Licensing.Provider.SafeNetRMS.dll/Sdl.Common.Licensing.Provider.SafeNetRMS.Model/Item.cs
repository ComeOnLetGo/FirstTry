using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class Item
	{
		[JsonProperty("itemProduct")]
		public ItemProduct ItemProduct { get; set; }
	}
}
