using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.ProjectSettings
{
	public class ProjectSettings : SettingsGroup
	{
		private const string ProjectOriginSetting = "ProjectOrigin";

		private const string ProjectIconPathSetting = "ProjectIconPath";

		private const string AccountIdSetting = "AccountId";

		private const string OfflineUserIdSetting = "OfflineUserId";

		private const string TenantNameSetting = "TenatNameSetting";

		private const string FolderStructureMigrationSetting = "FolderStructureMigration";

		public string ProjectOrigin
		{
			get
			{
				return Setting<string>.op_Implicit(((SettingsGroup)this).GetSetting<string>("ProjectOrigin"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("ProjectOrigin").Value = value;
			}
		}

		public string ProjectIconPath
		{
			get
			{
				return Setting<string>.op_Implicit(((SettingsGroup)this).GetSetting<string>("ProjectIconPath"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("ProjectIconPath").Value = value;
			}
		}

		public string AccountId
		{
			get
			{
				return Setting<string>.op_Implicit(((SettingsGroup)this).GetSetting<string>("AccountId"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("AccountId").Value = value;
			}
		}

		public string TenantName
		{
			get
			{
				return Setting<string>.op_Implicit(((SettingsGroup)this).GetSetting<string>("TenatNameSetting"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("TenatNameSetting").Value = value;
			}
		}

		public string OfflineUserId
		{
			get
			{
				return Setting<string>.op_Implicit(((SettingsGroup)this).GetSetting<string>("OfflineUserId"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("OfflineUserId").Value = value;
			}
		}

		public FolderStructureMigrationStatus FolderStructureMigration
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return Setting<FolderStructureMigrationStatus>.op_Implicit(((SettingsGroup)this).GetSetting<FolderStructureMigrationStatus>("FolderStructureMigration"));
			}
			set
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				((SettingsGroup)this).GetSetting<FolderStructureMigrationStatus>("FolderStructureMigration").Value = value;
			}
		}
	}
}
