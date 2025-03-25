using System;
using System.Text;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class LicenseFeature : ILicenseFeature
	{
		private readonly FeatureInfo _featureInfo;

		public string Id { get; set; }

		public string Name => _featureInfo.FeatureName;

		public string Version => _featureInfo.Version;

		public bool IsLocked => false;

		public bool IsCommuted => true;

		public bool IsAllowedOnVm => true;

		public int CommuterMaxCheckOutDays => _featureInfo.CommuterMaxCheckOutDays;

		public int LockingCriteria => _featureInfo.LockingCrit;

		public ILoginSession LoginSession { get; internal set; }

		public DateTime? ExpirationDate
		{
			get
			{
				if (_featureInfo.DeathDay != -1)
				{
					return ConvertFromUnixTimestamp(_featureInfo.DeathDay);
				}
				if (_featureInfo.IsTrialLicense)
				{
					return new DateTimeOffset(DateTime.Now).AddDays(_featureInfo.TrialCalendarPeriodLeft).Date;
				}
				return null;
			}
		}

		public int KeyLifetimeSec => _featureInfo.KeyLifetimeSec;

		public int LicenseType => _featureInfo.LicenseType;

		public LicenseMode Mode => (LicenseMode)2;

		public LicenseModeDetails ModeDetail => (LicenseModeDetails)2;

		public string Value => _featureInfo.VendorInfo;

		public bool HasExpired => false;

		public bool IsInstalledTrial => false;

		public bool IsNetwork => _featureInfo.IsNetwork;

		public LicenseFeature(FeatureInfo featInfo)
		{
			_featureInfo = featInfo;
		}

		public bool IsLoggedIn()
		{
			return true;
		}

		public void Logout()
		{
			try
			{
				LoginSession.Logout();
			}
			finally
			{
				LoginSession = null;
			}
		}

		public void ToString(StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine(_featureInfo.GetDiagonsticData());
		}

		private static DateTime ConvertFromUnixTimestamp(double timestamp)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp), TimeZoneInfo.Local);
		}
	}
}
