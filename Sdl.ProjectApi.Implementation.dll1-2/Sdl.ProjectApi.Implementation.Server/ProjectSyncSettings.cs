using System;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectSyncSettings : SettingsGroup
	{
		private const string LastSynchronizationTimestamp_Setting = "LastSynchronizationTimestamp";

		public DateTime? LastSynchronizationTimestamp
		{
			get
			{
				long num = Setting<long>.op_Implicit(((SettingsGroup)this).GetSetting<long>("LastSynchronizationTimestamp"));
				if (num != 0L)
				{
					return new DateTime(num, DateTimeKind.Utc);
				}
				return null;
			}
			set
			{
				if (!value.HasValue)
				{
					((SettingsGroup)this).GetSetting<long>("LastSynchronizationTimestamp").Value = 0L;
				}
				else
				{
					((SettingsGroup)this).GetSetting<long>("LastSynchronizationTimestamp").Value = value.Value.Ticks;
				}
			}
		}
	}
}
