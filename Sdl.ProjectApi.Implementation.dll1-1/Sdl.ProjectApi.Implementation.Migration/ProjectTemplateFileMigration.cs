using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal class ProjectTemplateFileMigration : AbstractFileMigration
	{
		public ProjectTemplateFileMigration()
		{
		}

		public ProjectTemplateFileMigration(IServerEvents serverEvents)
			: base(serverEvents)
		{
			_serverEvents = serverEvents;
		}

		public override IEnumerable<IMigration> GetDocumentMigrations(Version fileVersion)
		{
			return new List<IMigration>();
		}

		public override Version GetCurrentFileVersion()
		{
			return new Version("4.0.0.0");
		}

		public override string GetMigrationMessage(string filePath)
		{
			return null;
		}

		public override string GetInvalidVersionExceptionMessage(Version fileVersion, string filePath)
		{
			return string.Format(ErrorMessages.VersionUtil_InvalidProjectTemplateFileVersion, fileVersion, filePath);
		}
	}
}
