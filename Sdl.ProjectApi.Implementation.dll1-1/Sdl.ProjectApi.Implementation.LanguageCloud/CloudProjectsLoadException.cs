using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class CloudProjectsLoadException : Exception
	{
		public List<IProject> LocalProjects { get; }

		public List<AccountException> UnauthorizedAccounts { get; }

		public CloudProjectsLoadException()
		{
		}

		public CloudProjectsLoadException(string message)
			: base(message)
		{
		}

		public CloudProjectsLoadException(string message, Exception exception)
			: base(message, exception)
		{
		}

		public CloudProjectsLoadException(string message, Exception exception, List<IProject> projects, List<AccountException> unauthorizedAccounts)
			: base(message, exception)
		{
			LocalProjects = projects;
			UnauthorizedAccounts = unauthorizedAccounts;
		}

		public CloudProjectsLoadException(string message, Exception exception, List<AccountException> unauthorizedAccounts)
			: base(message, exception)
		{
			UnauthorizedAccounts = unauthorizedAccounts;
		}
	}
}
