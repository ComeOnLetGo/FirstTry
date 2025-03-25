using Sdl.Core.Settings;
using Sdl.ProjectApi.Settings.SettingTypes;

namespace Sdl.ProjectApi.Implementation.ProjectSettings
{
	public class MigrationDataSettings : SettingsGroup
	{
		private const string MIGRATIONDATA = "MigrationData";

		public MigrationData MigrationData
		{
			get
			{
				return Setting<MigrationData>.op_Implicit(((SettingsGroup)this).GetSetting<MigrationData>("MigrationData"));
			}
			set
			{
				((SettingsGroup)this).GetSetting<MigrationData>("MigrationData").Value = value;
			}
		}
	}
}
