using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal class ApplicationFileMigration : AbstractFileMigration
	{
		public override IEnumerable<IMigration> GetDocumentMigrations(Version fileVersion)
		{
			return new List<IMigration>();
		}

		public override Version GetCurrentFileVersion()
		{
			return new Version("3.0.0.0");
		}

		public override string GetMigrationMessage(string filePath)
		{
			return null;
		}

		public override string GetInvalidVersionExceptionMessage(Version fileVersion, string filePath)
		{
			return string.Format(ErrorMessages.VersionUtil_InvalidApplicationFileVersion, fileVersion, filePath);
		}
	}
}
