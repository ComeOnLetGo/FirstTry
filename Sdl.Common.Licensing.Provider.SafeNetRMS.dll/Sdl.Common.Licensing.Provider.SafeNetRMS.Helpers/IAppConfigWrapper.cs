namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	internal interface IAppConfigWrapper
	{
		bool TraceEnabled { get; }

		string TraceFilePath { get; }
	}
}
