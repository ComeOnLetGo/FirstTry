using System;

namespace Sdl.ProjectApi.Implementation
{
	public interface IFileBasedTranslationMemoryHelper
	{
		string GetFilePath(Uri uri);

		Uri GetUri(string filePath);
	}
}
