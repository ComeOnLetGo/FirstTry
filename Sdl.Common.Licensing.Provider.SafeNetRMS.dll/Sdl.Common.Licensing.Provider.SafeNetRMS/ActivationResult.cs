using Sdl.Common.Licensing.Provider.SafeNetRMS.Model;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class ActivationResult
	{
		public LicenseActivationResponse ActivationResponse { get; set; }

		public ProductKey ProductKeyInfo { get; set; }
	}
}
