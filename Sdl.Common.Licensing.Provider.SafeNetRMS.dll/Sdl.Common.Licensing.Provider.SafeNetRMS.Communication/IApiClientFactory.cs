namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Communication
{
	public interface IApiClientFactory
	{
		IThalesRestClient GetRestClient(string provider, string code);
	}
}
