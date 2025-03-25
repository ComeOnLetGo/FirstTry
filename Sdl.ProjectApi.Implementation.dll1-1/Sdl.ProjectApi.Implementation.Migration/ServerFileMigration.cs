using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Migration.ServerFile;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal class ServerFileMigration : AbstractFileMigration
	{
		public override IEnumerable<IMigration> GetDocumentMigrations(Version fileVersion)
		{
			List<IMigration> list = new List<IMigration>();
			if (fileVersion < new Version("3.2.0.0"))
			{
				list.AddRange(new List<IMigration>
				{
					new AddTranslateAndAnalyzeTaskSequenceMigration()
				});
			}
			return list;
		}

		public override Version GetCurrentFileVersion()
		{
			return new Version("3.2.0.0");
		}

		public override string GetMigrationMessage(string filePath)
		{
			return null;
		}

		public override string GetInvalidVersionExceptionMessage(Version fileVersion, string filePath)
		{
			return string.Format(ErrorMessages.VersionUtil_InvalidServerFileVersion, fileVersion, filePath);
		}
	}
}
