using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	[ExcludeFromCodeCoverage]
	internal class AppConfigWrapper : IAppConfigWrapper
	{
		internal const string TraceEnableConfig = "RMSTrace";

		internal const string TraceFilePathConfig = "RMSTraceFilePath";

		public bool TraceEnabled => string.Compare(ConfigurationManager.AppSettings["RMSTrace"], "true", StringComparison.InvariantCultureIgnoreCase) == 0;

		public string TraceFilePath => ConfigurationManager.AppSettings["RMSTraceFilePath"];
	}
}
