using System.IO;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class FileWrapper : IFileWrapper
	{
		public StreamReader OpenText(string filePath)
		{
			return File.OpenText(filePath);
		}
	}
}
