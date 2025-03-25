using System.Collections.Generic;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class TerminologyProviderSettings : SettingsGroup
	{
		private const string TermbasesSetting = "Termbases";

		private const string TermbasesOrderSetting = "TermbasesOrder";

		public Setting<List<Termbase>> Termbases => ((SettingsGroup)this).GetSetting<List<Termbase>>("Termbases");

		public Setting<List<string>> TermbasesOrder => ((SettingsGroup)this).GetSetting<List<string>>("TermbasesOrder");
	}
}
