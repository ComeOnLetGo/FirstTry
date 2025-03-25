namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	internal class FeatureInfo
	{
		private readonly string _featureInfoXml;

		public int CommuterMaxCheckOutDays { get; set; }

		public bool IsCommuted { get; set; }

		public bool IsNodeLocked { get; set; }

		public int DeathDay { get; set; }

		public int LicenseType { get; set; }

		public string FeatureName { get; set; }

		public int NumLicenses { get; set; }

		public bool IsAllowedOnVm { get; set; }

		public int TrialCalendarPeriodLeft { get; set; }

		public string VendorInfo { get; set; }

		public string Version { get; set; }

		public bool IsTrialLicense => LicenseType == 1;

		public int LockingCrit { get; set; }

		public int KeyLifetimeSec { get; set; }

		public bool IsNetwork { get; set; }

		public FeatureInfo(string featureInfoXml)
		{
			_featureInfoXml = featureInfoXml;
		}

		public string GetDiagonsticData()
		{
			return _featureInfoXml;
		}
	}
}
