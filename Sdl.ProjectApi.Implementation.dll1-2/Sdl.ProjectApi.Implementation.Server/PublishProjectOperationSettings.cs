using System;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class PublishProjectOperationSettings : SettingsGroup
	{
		private const string ServerUriSetting = "ServerUri";

		private const string OrganizationPathSetting = "OrganizationPath";

		private const string OrganizationIdsSetting = "OrganizationIds";

		private const string PublicationStatusSetting = "PublicationStatus";

		private const string LastSyncedAtSetting = "LastSyncedAt";

		private const string ServerUserNameSetting = "ServerUserName";

		private const string ServerUserTypeSetting = "ServerUserType";

		private const string PermissionsDeniedSetting = "PermissionsDenied";

		public Setting<string> ServerUri => ((SettingsGroup)this).GetSetting<string>("ServerUri");

		public Setting<string> OrganizationPath => ((SettingsGroup)this).GetSetting<string>("OrganizationPath");

		public Setting<string> OrganizationIds => ((SettingsGroup)this).GetSetting<string>("OrganizationIds");

		public Setting<bool> PermissionsDenied => ((SettingsGroup)this).GetSetting<bool>("PermissionsDenied");

		public Setting<DateTime> LastSyncedAt => ((SettingsGroup)this).GetSetting<DateTime>("LastSyncedAt");

		public Setting<PublicationStatus> PublicationStatus => ((SettingsGroup)this).GetSetting<PublicationStatus>("PublicationStatus");

		public Setting<string> ServerUserName => ((SettingsGroup)this).GetSetting<string>("ServerUserName");

		public Setting<UserManagerTokenType> ServerUserType => ((SettingsGroup)this).GetSetting<UserManagerTokenType>("ServerUserType");

		protected override object GetDefaultValue(string settingId)
		{
			if (!(settingId == "PublicationStatus"))
			{
				if (settingId == "LastSyncedAt")
				{
					return DateTime.MinValue;
				}
				return ((SettingsGroup)this).GetDefaultValue(settingId);
			}
			return (object)(PublicationStatus)0;
		}
	}
}
