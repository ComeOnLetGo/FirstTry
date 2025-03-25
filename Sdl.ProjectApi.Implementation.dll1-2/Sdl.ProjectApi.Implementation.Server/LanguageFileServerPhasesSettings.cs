using System.Collections.Generic;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class LanguageFileServerPhasesSettings : SettingsGroup
	{
		private const string LanguageFileServerPhases_Setting = "Phases";

		public Setting<List<string>> Phases => ((SettingsGroup)this).GetSetting<List<string>>("Phases");
	}
}
