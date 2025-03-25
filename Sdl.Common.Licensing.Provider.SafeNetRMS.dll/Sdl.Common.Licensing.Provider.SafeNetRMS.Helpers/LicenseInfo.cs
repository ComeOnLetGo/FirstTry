namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	internal class LicenseInfo
	{
		public string LicenseStorage { get; private set; }

		public string LicenseHash { get; private set; }

		public bool IsAdditive { get; private set; }

		public LicenseInfo(string storage, string hash, bool isAdditive)
		{
			LicenseStorage = storage;
			LicenseHash = hash;
			IsAdditive = isAdditive;
		}
	}
}
