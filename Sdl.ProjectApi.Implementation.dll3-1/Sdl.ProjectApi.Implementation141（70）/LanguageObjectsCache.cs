using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.LanguageProcessing.Resources;
using Sdl.Core.LanguageProcessing.Tokenization;
using Sdl.Desktop.Platform.Events;
using Sdl.Desktop.Platform.Services;
using Sdl.LanguagePlatform.Core.Tokenization;
using Sdl.LanguagePlatform.Lingua;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class LanguageObjectsCache : ILanguageObjectsCache, IDisposable
	{
		private readonly ConcurrentDictionary<string, object> _cache;

		public LanguageObjectsCache()
		{
			IServiceContext context = GlobalServices.Context;
			this._002Ector((context != null) ? context.GetService<IEventAggregator>() : null);
		}

		public LanguageObjectsCache(IEventAggregator eventAggregator)
		{
			_cache = new ConcurrentDictionary<string, object>();
			base._002Ector();
			if (eventAggregator != null)
			{
				ObservableExtensions.Subscribe<LanguageResourcesChangedEvent>(eventAggregator.GetEvent<LanguageResourcesChangedEvent>(), (Action<LanguageResourcesChangedEvent>)OnLanguageResourcesChanged);
			}
		}

		public LanguageResourceBundleCollection GetLanguageResources(ITranslatableFile file)
		{
			return GetLanguageResources(((IProjectFile)file).Project, ((IProjectFile)file).Language);
		}

		public LanguageResourceBundleCollection GetLanguageResources(IProject project, Language language)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			return (LanguageResourceBundleCollection)_cache.GetOrAdd("LanguageResources_" + ((LanguageBase)language).IsoAbbreviation.ToLower(), (string _) => ((object)project.SourceLanguage).Equals((object)language) ? ((((IProjectConfiguration)project).LanguageResources == null) ? ((object)new LanguageResourceBundleCollection()) : ((object)((ISupportPlaceables)((IProjectConfiguration)project).LanguageResources).LanguageResourceBundles)) : GetLanguageResources(project.GetLanguageDirection(language)));
		}

		private static LanguageResourceBundleCollection GetLanguageResources(ILanguageDirection languageDirection)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			ProjectCascade projectCascade = languageDirection.CreateCascade(refreshCache: false);
			foreach (ProjectCascadeEntry cascadeEntry in ((Cascade<ProjectCascadeEntry>)projectCascade).CascadeEntries)
			{
				if (((CascadeEntry)cascadeEntry).TranslationProviderLanguageDirection != null)
				{
					ITranslationProvider translationProvider = ((CascadeEntry)cascadeEntry).TranslationProviderLanguageDirection.TranslationProvider;
					ISupportPlaceables val = (ISupportPlaceables)(object)((translationProvider is ISupportPlaceables) ? translationProvider : null);
					if (val != null && val.LanguageResourceBundles != null)
					{
						return val.LanguageResourceBundles;
					}
				}
			}
			if (languageDirection.Configuration.LanguageResources != null)
			{
				return ((ISupportPlaceables)languageDirection.Configuration.LanguageResources).LanguageResourceBundles;
			}
			return new LanguageResourceBundleCollection();
		}

		public BuiltinRecognizers GetRecognizers(ILanguageDirection languageDirection)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			return (BuiltinRecognizers)_cache.GetOrAdd("Recognizers_" + ((LanguageBase)languageDirection.SourceLanguage).IsoAbbreviation + "_" + ((LanguageBase)languageDirection.TargetLanguage).IsoAbbreviation, delegate
			{
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				ProjectCascade projectCascade = languageDirection.CreateCascade(refreshCache: false);
				foreach (ProjectCascadeEntry cascadeEntry in ((Cascade<ProjectCascadeEntry>)projectCascade).CascadeEntries)
				{
					ITranslationProviderLanguageDirection translationProviderLanguageDirection = ((CascadeEntry)cascadeEntry).TranslationProviderLanguageDirection;
					ITranslationMemoryLanguageDirection val = (ITranslationMemoryLanguageDirection)(object)((translationProviderLanguageDirection is ITranslationMemoryLanguageDirection) ? translationProviderLanguageDirection : null);
					if (val != null)
					{
						return ((ISupportPlaceables)val.TranslationProvider).Recognizers;
					}
				}
				return (object)(BuiltinRecognizers)127;
			});
		}

		public Tokenizer GetTokenizer(ITranslatableFile file)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			Language language = (((ILanguageFile)file).IsSource ? ((IProjectFile)file).Language : ((ILanguageFile)file).LanguageDirection.SourceLanguage);
			return (Tokenizer)_cache.GetOrAdd("Tokenizer_" + ((LanguageBase)language).IsoAbbreviation + "-" + (((ILanguageFile)file).IsSource ? "none" : ((LanguageBase)((IProjectFile)file).Language).IsoAbbreviation), delegate
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				BuiltinRecognizers val = (BuiltinRecognizers)(((ILanguageFile)file).IsSource ? 127 : ((int)GetRecognizers(((ILanguageFile)file).LanguageDirection)));
				TokenizerSetup val2 = TokenizerSetupFactory.Create(CultureCode.op_Implicit(language.CultureInfo), val);
				val2.CreateWhitespaceTokens = true;
				return Tokenizer.Create(val2, GetLanguageResources(file).ResourceDataAccessor);
			});
		}

		public LanguageTools GetLanguageTools(ILanguageDirection languageDirection)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			LanguageResourceBundleCollection languageResources = GetLanguageResources(languageDirection);
			return GetLanguageTools(languageDirection.SourceLanguage, languageDirection.TargetLanguage, new LanguageResources(CultureCode.op_Implicit(languageDirection.SourceLanguage.CultureInfo), (languageResources != null) ? languageResources.ResourceDataAccessor : null), GetRecognizers(languageDirection));
		}

		public LanguageTools GetLanguageTools(Language sourceLanguage, Language targetLanguage, LanguageResources languageResources, BuiltinRecognizers recognizers)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			return (LanguageTools)_cache.GetOrAdd("LanguageTools_" + ((LanguageBase)sourceLanguage).IsoAbbreviation + "_" + ((LanguageBase)targetLanguage).IsoAbbreviation, (string _) => (object)new LanguageTools(languageResources, recognizers));
		}

		public void Dispose()
		{
			ResetCache();
		}

		private void OnLanguageResourcesChanged(LanguageResourcesChangedEvent _)
		{
			ResetCache();
		}

		private void ResetCache()
		{
			IEnumerable<IDisposable> enumerable = _cache.OfType<IDisposable>();
			foreach (IDisposable item in enumerable)
			{
				item.Dispose();
			}
			_cache.Clear();
		}
	}
}
