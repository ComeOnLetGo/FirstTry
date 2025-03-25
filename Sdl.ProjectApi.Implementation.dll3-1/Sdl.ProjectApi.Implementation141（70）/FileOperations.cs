using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	public class FileOperations : IFileOperations
	{
		public void CopyFile(string sourceFilePath, string targetFilePath)
		{
			Util.CopyFile(sourceFilePath, targetFilePath);
		}

		public bool FileExists(string fileName)
		{
			return File.Exists(fileName);
		}
	}
}
