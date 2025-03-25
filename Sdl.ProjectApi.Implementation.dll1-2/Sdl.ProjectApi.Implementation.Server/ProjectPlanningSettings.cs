using System.Collections.Generic;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectPlanningSettings : SettingsGroup, IDefaultValueAware
	{
		private const string ServerUriSetting = "ServerUri";

		private const string OrganizationPathSetting = "OrganizationPath";

		private const string AssignmentsSetting = "Assignments";

		public Setting<string> ServerUri => ((SettingsGroup)this).GetSetting<string>("ServerUri");

		public Setting<string> OrganizationPath => ((SettingsGroup)this).GetSetting<string>("OrganizationPath");

		public Setting<List<ProjectAssignmentSettings>> Assignments => ((SettingsGroup)this).GetSetting<List<ProjectAssignmentSettings>>("Assignments");

		protected override object GetDefaultValue(string settingId)
		{
			return settingId switch
			{
				"ServerUri" => string.Empty, 
				"OrganizationPath" => string.Empty, 
				"Assignments" => new List<ProjectAssignmentSettings>(), 
				_ => ((SettingsGroup)this).GetDefaultValue(settingId), 
			};
		}

		public bool IsDefaultValue(string settingId)
		{
			switch (settingId)
			{
			case "ServerUri":
				return ((SettingsGroup)this).GetDefaultValue(settingId).Equals(ServerUri.Value);
			case "OrganizationPath":
				return ((SettingsGroup)this).GetDefaultValue(settingId).Equals(OrganizationPath.Value);
			case "Assignments":
			{
				Setting<List<ProjectAssignmentSettings>> assignments = Assignments;
				if (assignments == null)
				{
					return false;
				}
				return assignments.Value?.Count == 0;
			}
			default:
				return false;
			}
		}
	}
}
