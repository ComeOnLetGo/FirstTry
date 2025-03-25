using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public static class ProjectConfigurationHelper
	{
		public static IList<Language> GetProjectLanguages(this IProjectConfiguration project)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			IList<Language> list = new List<Language>();
			if (project is IProject)
			{
				Language sourceLanguage = ((IProject)project).SourceLanguage;
				list.Add(sourceLanguage);
			}
			ILanguageDirection[] languageDirections = project.LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				Language sourceLanguage2 = val.SourceLanguage;
				if (!list.Contains(sourceLanguage2))
				{
					list.Add(sourceLanguage2);
				}
				Language targetLanguage = val.TargetLanguage;
				if (!list.Contains(targetLanguage))
				{
					list.Add(targetLanguage);
				}
			}
			return list;
		}

		public static void RemoveRedundantCascadeEntryItemsFromAllLanguagePairs(this IProjectConfiguration project, bool checkServerBasedTM)
		{
			if (project.CascadeItem == null || project.CascadeItem.CascadeEntryItems == null)
			{
				return;
			}
			IEnumerable<LanguagePair> languagePairs = project.GetLanguagePairs();
			for (int num = project.CascadeItem.CascadeEntryItems.Count - 1; num >= 0; num--)
			{
				ProjectCascadeEntryItem val = project.CascadeItem.CascadeEntryItems[num];
				ITranslationProviderItem mainTranslationProviderItem = val.MainTranslationProviderItem;
				if (mainTranslationProviderItem != null && (!mainTranslationProviderItem.IsServerBasedTranslationMemory() || checkServerBasedTM))
				{
					try
					{
						ITranslationProvider translationProvider = project.TranslationProviderCache.GetTranslationProvider(mainTranslationProviderItem.Uri, mainTranslationProviderItem.State, project.ProjectsProvider.Application.TranslationProviderCredentialStore, false);
						if (translationProvider != null)
						{
							bool flag = true;
							foreach (LanguagePair item in languagePairs)
							{
								if (translationProvider.SupportsLanguageDirection(item))
								{
									flag = false;
								}
							}
							if (flag)
							{
								project.CascadeItem.CascadeEntryItems.RemoveAt(num);
							}
						}
					}
					catch
					{
					}
				}
			}
		}

		public static IEnumerable<LanguagePair> GetLanguagePairs(this IProjectConfiguration project)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			IList<LanguagePair> list = new List<LanguagePair>();
			ILanguageDirection[] languageDirections = project.LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				if (val.SourceLanguage != null && val.TargetLanguage != null)
				{
					list.Add(new LanguagePair(CultureCode.op_Implicit(val.SourceLanguage.CultureInfo), CultureCode.op_Implicit(val.TargetLanguage.CultureInfo)));
				}
			}
			return list;
		}
	}
}
