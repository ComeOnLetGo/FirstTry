using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class ResultantStates
	{
		[JsonProperty("resultantState")]
		public List<ResultantState> ResultantState { get; set; }
	}
}
