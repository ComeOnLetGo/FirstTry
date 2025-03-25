using System;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class AccountException
	{
		public string AccountName { get; set; }

		public Exception Exception { get; set; }
	}
}
