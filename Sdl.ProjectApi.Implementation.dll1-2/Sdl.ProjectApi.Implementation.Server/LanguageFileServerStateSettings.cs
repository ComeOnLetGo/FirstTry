using System;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class LanguageFileServerStateSettings : SettingsGroup
	{
		private const string LatestServerVersionTimestamp_Setting = "LatestServerVersionTimestamp";

		private const string CurrentServerVersionTimestamp_Setting = "CurrentServerVersionTimestamp";

		private const string LatestServerVersionNumber_Setting = "LatestServerVersionNumber";

		private const string CurrentServerVersionNumber_Setting = "CurrentServerVersionNumber";

		private const string CheckedOutTo_Setting = "CheckedOutTo";

		private const string CheckedOutAt_Setting = "CheckedOutAt";

		private const string IsCheckedOutOnline_Setting = "IsCheckedOutOnline";

		public Setting<long> LatestServerVersionTimestamp => ((SettingsGroup)this).GetSetting<long>("LatestServerVersionTimestamp");

		public Setting<int> LatestServerVersionNumber => ((SettingsGroup)this).GetSetting<int>("LatestServerVersionNumber");

		public Setting<string> CheckedOutTo => ((SettingsGroup)this).GetSetting<string>("CheckedOutTo");

		public Setting<DateTime> CheckedOutAt => ((SettingsGroup)this).GetSetting<DateTime>("CheckedOutAt");

		public Setting<bool> IsCheckedOutOnline => ((SettingsGroup)this).GetSetting<bool>("IsCheckedOutOnline");

		public Setting<long> GetCurrentServerVersionTimestamp(string fileName)
		{
			return ((SettingsGroup)this).GetSetting<long>("CurrentServerVersionTimestamp_" + fileName);
		}

		public Setting<int> GetCurrentServerVersionNumber(string fileName)
		{
			return ((SettingsGroup)this).GetSetting<int>("CurrentServerVersionNumber_" + fileName);
		}

		protected override object GetDefaultValue(string settingId)
		{
			if (settingId == "CheckedOutAt")
			{
				return DateTime.MinValue;
			}
			return ((SettingsGroup)this).GetDefaultValue(settingId);
		}
	}
}
