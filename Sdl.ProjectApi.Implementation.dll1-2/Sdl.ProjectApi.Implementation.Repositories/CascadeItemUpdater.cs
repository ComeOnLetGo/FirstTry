using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class CascadeItemUpdater
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.ProjectTemplate _xmlProjectTemplate;

		public CascadeItemUpdater(Sdl.ProjectApi.Implementation.Xml.ProjectTemplate xmlProjectTemplate)
		{
			_xmlProjectTemplate = xmlProjectTemplate;
		}

		public void UpdateCascadeItems(IRelativePathManager projectPathManager)
		{
			UpdateCascadeItem(projectPathManager, _xmlProjectTemplate.CascadeItem);
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection in _xmlProjectTemplate.LanguageDirections)
			{
				SetLanguageDirectionCascadeItem(projectPathManager, languageDirection);
				UpdateCascadeItem(projectPathManager, languageDirection.CascadeItem);
			}
		}

		public void SetLanguageDirectionCascadeItem(IRelativePathManager relativePathManager, Sdl.ProjectApi.Implementation.Xml.LanguageDirection xmlLanguageDirection)
		{
			IProjectConfiguration val = (IProjectConfiguration)(object)((relativePathManager is IProjectConfiguration) ? relativePathManager : null);
			if (val != null)
			{
				ILanguageDirection projectLanguageDirection = GetProjectLanguageDirection(val, xmlLanguageDirection);
				if (projectLanguageDirection != null)
				{
					xmlLanguageDirection.CascadeItem = projectLanguageDirection.CascadeItem.ToXml(relativePathManager);
				}
			}
		}

		public ILanguageDirection GetProjectLanguageDirection(IProjectConfiguration projectConfiguration, Sdl.ProjectApi.Implementation.Xml.LanguageDirection xmlLanguageDirection)
		{
			return (projectConfiguration != null) ? projectConfiguration.LanguageDirections.FirstOrDefault((ILanguageDirection a) => string.Compare(a.SourceLanguage.CultureInfo.Name, xmlLanguageDirection?.SourceLanguageCode, StringComparison.CurrentCultureIgnoreCase) == 0 && string.Compare(a.TargetLanguage.CultureInfo.Name, xmlLanguageDirection?.TargetLanguageCode, StringComparison.CurrentCultureIgnoreCase) == 0) : null;
		}

		private void UpdateCascadeItem(IRelativePathManager projectPathManager, CascadeItem cascadeItem)
		{
			foreach (CascadeEntryItem item in cascadeItem.CascadeEntryItem)
			{
				if (item.MainTranslationProviderItem != null)
				{
					ChangeRelativeUriToAbsoluteUri(projectPathManager, item.MainTranslationProviderItem);
				}
				item.ProjectTranslationProviderItem = new List<TranslationProviderItem>();
			}
		}

		private void ChangeRelativeUriToAbsoluteUri(IRelativePathManager projectPathManager, TranslationProviderItem xmlTranslationProviderItem)
		{
			string uri = xmlTranslationProviderItem.Uri;
			Uri absoluteUri = TranslationProviderItemConverter.GetAbsoluteUri(uri, projectPathManager);
			xmlTranslationProviderItem.Uri = absoluteUri.ToString();
		}
	}
}
