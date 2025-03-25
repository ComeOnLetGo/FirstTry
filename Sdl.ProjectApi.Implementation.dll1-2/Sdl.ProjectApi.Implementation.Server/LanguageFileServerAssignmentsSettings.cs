using System;
using System.Collections.Generic;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class LanguageFileServerAssignmentsSettings : SettingsGroup
	{
		private const string LanguageFileAssignmentIsCurrentAssignment_Setting = "IsCurrentAssignment";

		private const string LanguageFileAssignmentAssignedAt_Setting = "AssignedAt";

		private const string LanguageFileAssignmentAssignedBy_Setting = "AssignedBy";

		private const string LanguageFileAssignmentAssignees_Setting = "Assignees";

		private const string LanguageFileAssignmentDueDate_Setting = "DueDate";

		public Setting<bool> IsCurrentAssignment
		{
			get
			{
				return ((SettingsGroup)this).GetSetting<bool>("IsCurrentAssignment");
			}
			set
			{
				((SettingsGroup)this).GetSetting<bool>("IsCurrentAssignment").Value = Setting<bool>.op_Implicit(value);
			}
		}

		public Setting<DateTime> AssignedAt
		{
			get
			{
				return ((SettingsGroup)this).GetSetting<DateTime>("AssignedAt");
			}
			set
			{
				((SettingsGroup)this).GetSetting<DateTime>("AssignedAt").Value = Setting<DateTime>.op_Implicit(value);
			}
		}

		public Setting<DateTime> DueDate
		{
			get
			{
				return ((SettingsGroup)this).GetSetting<DateTime>("DueDate");
			}
			set
			{
				((SettingsGroup)this).GetSetting<DateTime>("DueDate").Value = Setting<DateTime>.op_Implicit(value);
			}
		}

		public Setting<string> AssignedBy
		{
			get
			{
				return ((SettingsGroup)this).GetSetting<string>("AssignedBy");
			}
			set
			{
				((SettingsGroup)this).GetSetting<string>("AssignedBy").Value = Setting<string>.op_Implicit(value);
			}
		}

		public Setting<List<string>> Assignees
		{
			get
			{
				return ((SettingsGroup)this).GetSetting<List<string>>("Assignees");
			}
			set
			{
				((SettingsGroup)this).GetSetting<List<string>>("Assignees").Value = Setting<List<string>>.op_Implicit(value);
			}
		}
	}
}
