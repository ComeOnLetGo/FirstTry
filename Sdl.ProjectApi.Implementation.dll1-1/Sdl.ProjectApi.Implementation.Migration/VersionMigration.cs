using System;
using System.Xml.Linq;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal class VersionMigration : IMigration
	{
		private readonly Version _currentVersion;

		public VersionMigration(Version currentVersion)
		{
			_currentVersion = currentVersion;
		}

		public void Migrate(XDocument document, Version documentVersion)
		{
			document.Root?.Attribute("Version")?.SetValue(_currentVersion.ToString());
		}
	}
}
