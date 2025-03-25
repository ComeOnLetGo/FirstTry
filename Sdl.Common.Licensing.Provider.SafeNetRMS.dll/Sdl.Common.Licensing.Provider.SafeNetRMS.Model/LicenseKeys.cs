using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class LicenseKeys
	{
		[JsonProperty("licenseKey")]
		public List<LicenseKey> LicenseKey { get; set; }
	}
}
