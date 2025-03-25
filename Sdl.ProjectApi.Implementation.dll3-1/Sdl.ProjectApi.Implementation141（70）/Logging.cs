using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;

namespace Sdl.ProjectApi.Implementation
{
	public class Logging
	{
		private static ILogger _defaultLog;

		public static ILogger DefaultLog => _defaultLog ?? (_defaultLog = LogProvider.GetLoggerFactory().CreateLogger("ProjectApi"));
	}
}
