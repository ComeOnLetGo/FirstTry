using Sdl.ProjectApi.Implementation.Migration;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal class VersionUtil
	{
		public const string CURRENT_PROJECT_VERSION = "4.0.0.0";

		public const string CURRENT_PROJECTTEMPLATE_VERSION = "4.0.0.0";

		public const string CURRENT_SERVER_VERSION = "3.2.0.0";

		public const string CURRENT_APPLICATION_VERSION = "3.0.0.0";

		public static void CheckProjectVersion(string projectFilePath, IServerEvents serverEvents)
		{
			ProjectFileMigration projectFileMigration = new ProjectFileMigration(serverEvents);
			projectFileMigration.Migrate(projectFilePath);
		}

		public static void CheckProjectTemplateVersion(string projectTemplateFilePath, IServerEvents serverEvents)
		{
			ProjectTemplateFileMigration projectTemplateFileMigration = new ProjectTemplateFileMigration(serverEvents);
			projectTemplateFileMigration.Migrate(projectTemplateFilePath);
		}

		public static void CheckServerVersion(string serverFilePath)
		{
			ServerFileMigration serverFileMigration = new ServerFileMigration();
			serverFileMigration.Migrate(serverFilePath);
		}

		public static void CheckApplicationVersion(string applicationFilePath)
		{
			ApplicationFileMigration applicationFileMigration = new ApplicationFileMigration();
			applicationFileMigration.Migrate(applicationFilePath);
		}
	}
}
