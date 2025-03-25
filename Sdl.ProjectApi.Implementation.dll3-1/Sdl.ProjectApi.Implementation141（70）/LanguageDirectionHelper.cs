using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sdl.Core.Globalization;
using Sdl.Core.PluginFramework;
using Sdl.Desktop.Platform.PluginManagement;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public static class LanguageDirectionHelper
	{
		private static IList<ITranslationProviderFactory> _translationProviderFactories;

		private static readonly object _factoriesLock = new object();

		public static ProjectCascade CreateCascade(this ILanguageDirection languageDirection, bool refreshCache)
		{
			return languageDirection.CreateCascade(null, null, readOnly: true, refreshCache);
		}

		public static ProjectCascade CreateCascade(this ILanguageDirection languageDirection, ProjectCascadeEntryDataFilterFunction filter, IComparer<ProjectCascadeEntryData> sort, bool readOnly, bool refreshCache)
		{
			ProjectCascadeSettings cascadeSettings = languageDirection.GetCascadeSettings();
			cascadeSettings.Filter = filter;
			cascadeSettings.Sort = sort;
			cascadeSettings.ReadOnly = readOnly;
			ProjectCascadeFactory cascadeFactory = languageDirection.Configuration.GetCascadeFactory();
			return cascadeFactory.CreateCascade(cascadeSettings, refreshCache);
		}

		public static ProjectCascadeSettings GetCascadeSettings(this ILanguageDirection languageDirection)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			ProjectCascadeItem cascadeItem = languageDirection.Configuration.CascadeItem;
			ProjectCascadeItem cascadeItem2 = languageDirection.CascadeItem;
			LanguagePair languageDirection2 = new LanguagePair(CultureCode.op_Implicit(languageDirection.SourceLanguage.CultureInfo), CultureCode.op_Implicit(languageDirection.TargetLanguage.CultureInfo));
			return new ProjectCascadeSettings(cascadeItem, cascadeItem2, languageDirection2);
		}

		public static ProjectCascadeFactory GetCascadeFactory(this IProjectConfiguration projectConfiguration)
		{
			return new ProjectCascadeFactory(GetTranslationProviderCache(projectConfiguration), GetTranslationProviderCredentialStore(projectConfiguration), projectConfiguration.ProjectsProvider.Application.ServerEvents);
		}

		private static ITranslationProviderCache GetTranslationProviderCache(IProjectConfiguration projectConfiguration)
		{
			return projectConfiguration.TranslationProviderCache;
		}

		private static ITranslationProviderCredentialStore GetTranslationProviderCredentialStore(IProjectConfiguration projectConfiguration)
		{
			return projectConfiguration.ProjectsProvider.Application.TranslationProviderCredentialStore;
		}

		public static string GetTranslationProviderName(ITranslationProviderItem translationProviderItem)
		{
			IList<ITranslationProviderFactory> translationProviderFactories = GetTranslationProviderFactories();
			foreach (ITranslationProviderFactory item in translationProviderFactories)
			{
				if (item.SupportsTranslationProviderUri(translationProviderItem.Uri))
				{
					TranslationProviderInfo translationProviderInfo = item.GetTranslationProviderInfo(translationProviderItem.Uri, translationProviderItem.State);
					return translationProviderInfo.Name;
				}
			}
			return string.Empty;
		}

		private static IList<ITranslationProviderFactory> GetTranslationProviderFactories()
		{
			lock (_factoriesLock)
			{
				if (_translationProviderFactories == null)
				{
					_translationProviderFactories = new List<ITranslationProviderFactory>();
					IExtensionPoint extensionPoint = StudioPluginManager.PluginRegistry.GetExtensionPoint<TranslationProviderFactoryAttribute>();
					foreach (IExtension item in (ReadOnlyCollection<IExtension>)(object)extensionPoint.Extensions)
					{
						object obj = item.CreateInstance();
						ITranslationProviderFactory val = (ITranslationProviderFactory)((obj is ITranslationProviderFactory) ? obj : null);
						if (val == null)
						{
							throw new InvalidCastException("translationProviderFactory is not of the expected type.");
						}
						_translationProviderFactories.Add(val);
					}
				}
			}
			return _translationProviderFactories;
		}
	}
}
