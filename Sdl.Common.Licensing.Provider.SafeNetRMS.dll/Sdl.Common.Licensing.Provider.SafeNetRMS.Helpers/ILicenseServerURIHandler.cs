using System;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	internal interface ILicenseServerURIHandler
	{
		Uri UpdateActivationServerData(string providerName);
	}
}
