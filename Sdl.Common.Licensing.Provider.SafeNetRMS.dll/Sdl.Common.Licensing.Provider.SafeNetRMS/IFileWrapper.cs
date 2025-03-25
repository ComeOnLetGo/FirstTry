using System.IO;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal interface IFileWrapper
	{
		StreamReader OpenText(string filePath);
	}
}
