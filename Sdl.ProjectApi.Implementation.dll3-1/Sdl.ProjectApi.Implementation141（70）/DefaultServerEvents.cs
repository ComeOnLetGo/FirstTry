using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	internal class DefaultServerEvents : IServerEvents
	{
		public bool ShouldAttachProject(IProject project)
		{
			return true;
		}

		public string GetLocalTranslationMemoryLocation(IProjectConfiguration projectConfig, string invalidTranslationMemoryFilePath)
		{
			return null;
		}

		public string GetLanguagesResourcesLocation(IProjectConfiguration projectConfig, string invalidLanguageResourcesFilePath)
		{
			return null;
		}

		public void HandleTranslationProviderException(ITranslationProviderItem translationProviderItem, Exception exception)
		{
		}

		public void HandleTranslationProviderAuthenticationException(ITranslationProviderItem translationProviderItem, TranslationProviderAuthenticationException exception, bool performUpdate)
		{
		}

		public void ShowMigrationMessage(string migrationMessage)
		{
		}

		public string GetFilterDefinitionLocation(IProjectConfiguration projectConfig, string invalidFilterDefinitionFilePath)
		{
			return null;
		}
	}
}
