using System;
using Sdl.MultiTerm.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal class ProjectApiExtensions
	{
		public bool IsLocalTermbase(string settingsXml)
		{
			TermbaseSettings val = TermbaseSettings.FromXml(settingsXml);
			return val.Local;
		}

		public string NewGuid()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
