using System;
using System.Globalization;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	internal class TranslationMemoryInfo : ITranslationMemoryInfo
	{
		private readonly FileBasedTranslationMemory _fileBasedTranslationMemory;

		private readonly Uri _uri;

		public bool IsFileBasedTranslationMemory => FileBasedTranslationMemory.IsFileBasedTranslationMemory(_uri);

		public TranslationMemoryInfo(Uri uri)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			_uri = uri;
			try
			{
				_fileBasedTranslationMemory = new FileBasedTranslationMemory(uri);
			}
			catch (Exception)
			{
				throw new ArgumentException(StringResources.InvalidTranslationMemoryInfo);
			}
		}

		public CultureInfo GetTargetLanguage()
		{
			return CultureCode.op_Implicit(((ITranslationProviderLanguageDirection)((AbstractLocalTranslationMemory)_fileBasedTranslationMemory).LanguageDirection).TargetLanguage);
		}

		public bool SupportsLanguageDirection(LanguagePair languagePair)
		{
			return ((AbstractLocalTranslationMemory)_fileBasedTranslationMemory).SupportsLanguageDirection(languagePair);
		}

		public bool IsPasswordProtected()
		{
			return _fileBasedTranslationMemory.IsProtected;
		}
	}
}
