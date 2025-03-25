namespace Sdl.ProjectApi.Implementation
{
	public interface IFileOperations
	{
		void CopyFile(string sourceFilePath, string targetFilePath);

		bool FileExists(string fileName);
	}
}
