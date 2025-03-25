using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class FileBasedTranslationMemoryHelper : IFileBasedTranslationMemoryHelper
	{
		public string GetFilePath(Uri uri)
		{
			return FileBasedTranslationMemory.GetFileBasedTranslationMemoryFilePath(uri);
		}

		public Uri GetUri(string filePath)
		{
			return FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(filePath);
		}
	}
}
