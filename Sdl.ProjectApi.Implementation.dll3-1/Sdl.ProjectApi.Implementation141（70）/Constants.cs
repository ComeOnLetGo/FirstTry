using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Repositories;

namespace Sdl.ProjectApi.Implementation
{
	public class Constants
	{
		public class Registry
		{
			public static readonly string KeyCurrentUser = "SOFTWARE\\SDL\\Studio17\\MachineSupport";

			public const string ValueCurrentUserId = "CurrentUserId";

			public const string ValueCurrentUserEmail = "CurrentUserEmail";

			public const string ValueCurrentUserFullName = "CurrentUserFullName";

			public const string ValueCurrentUserPhoneNumber = "CurrentUserPhoneNumber";

			public const string ValueCurrentUserDescription = "CurrentUserDescription";
		}

		public const string LCProject = "LC project";

		public const string OfflineCloudPackage = "Offline cloud package";

		internal static List<IProjectRepositoryFactory> SupportedProjectRepositoryFactories = new List<IProjectRepositoryFactory>
		{
			new LanguageCloudProjectRepositoryFactory(),
			new SecureProjectRepositoryFactory(),
			new ProjectRepositoryFactory()
		};
	}
}
