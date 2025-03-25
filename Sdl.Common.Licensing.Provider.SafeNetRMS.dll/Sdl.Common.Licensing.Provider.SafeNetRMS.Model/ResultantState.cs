using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ResultantState
	{
		[JsonProperty("keyResultantState")]
		public string KeyResultantState { get; set; }

		[JsonProperty("keyResultantStateRef")]
		public object KeyResultantStateRef { get; set; }
	}
}
