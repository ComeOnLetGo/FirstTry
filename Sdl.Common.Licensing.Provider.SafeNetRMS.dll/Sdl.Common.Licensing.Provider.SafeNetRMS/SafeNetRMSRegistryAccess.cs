using Sdl.Common.Licensing.Provider.Core;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class SafeNetRMSRegistryAccess : LicenseRegistryAccess
	{
		public SafeNetRMSRegistryAccess(string currentUserRegistryPath)
			: base(currentUserRegistryPath)
		{
		}
	}
}
