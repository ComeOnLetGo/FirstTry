namespace Sdl.ProjectApi.Implementation
{
	public interface IPackageProjectArchiver
	{
		string ExtractPackage(string packageFilePath);

		void ZipDirectory(string packageFilepath, string localDataFolder);

		void UnZip(string zipfile, string targetDir);
	}
}
