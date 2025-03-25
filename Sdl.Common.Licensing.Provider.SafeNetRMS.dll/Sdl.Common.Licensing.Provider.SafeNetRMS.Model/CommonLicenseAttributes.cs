using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class CommonLicenseAttributes
	{
		[JsonProperty("commonLicenseAttribute")]
		public List<CommonLicenseAttribute> CommonLicenseAttribute { get; set; }
	}
}
