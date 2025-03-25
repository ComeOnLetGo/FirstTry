using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascadeEntry : CascadeEntry, IProjectCascadeEntry
	{
		public IProjectCascadeEntryData ProjectCascadeEntryData { get; private set; }

		public ProjectCascadeEntry(ITranslationProviderLanguageDirection translationProviderLanguageDirection, int penalty, IProjectCascadeEntryData projectCascadeEntryData)
			: base(translationProviderLanguageDirection, penalty)
		{
			if (projectCascadeEntryData == null)
			{
				throw new ArgumentNullException("projectCascadeEntryData");
			}
			ProjectCascadeEntryData = projectCascadeEntryData;
		}

		ITranslationProviderLanguageDirection IProjectCascadeEntry.get_TranslationProviderLanguageDirection()
		{
			return ((CascadeEntry)this).TranslationProviderLanguageDirection;
		}
	}
}
