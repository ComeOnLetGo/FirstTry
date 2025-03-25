using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectSettingsUpdater : IProjectSettingsUpdater
	{
		public void SuppressSetting(ISettingsBundle projectSettings, string settingGroupId, string settingId)
		{
			projectSettings.GetSettingsGroup(settingGroupId).RemoveSetting(settingId);
		}

		public void ImportSetting(ISettingsBundle projectSettings, ISettingsGroup newSettingsGroup)
		{
			projectSettings.GetSettingsGroup(newSettingsGroup.Id).ImportSettings(newSettingsGroup);
		}
	}
}
