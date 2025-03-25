using System;

namespace Sdl.ProjectApi.Implementation
{
	public class TranslationMemoryInfoFactory : ITranslationMemoryInfoFactory
	{
		public ITranslationMemoryInfo Create(Uri translationProvideUri)
		{
			try
			{
				return (ITranslationMemoryInfo)(object)new TranslationMemoryInfo(translationProvideUri);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
	}
}
