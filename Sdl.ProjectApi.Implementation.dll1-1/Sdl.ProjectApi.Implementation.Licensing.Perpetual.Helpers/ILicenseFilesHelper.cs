namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers
{
	internal interface ILicenseFilesHelper
	{
		string GetCommuterLicenseDefinition();

		void AddTrialLicense(string licenseFilePath);

		string GetTrialActivationId();
	}
}
