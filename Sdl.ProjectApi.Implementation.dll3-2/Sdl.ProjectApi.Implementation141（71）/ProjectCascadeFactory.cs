using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascadeFactory
	{
		private readonly ITranslationProviderCache _translationProviderCache;

		private readonly ITranslationProviderCredentialStore _translationProviderCredentialStore;

		private readonly IServerEvents _serverEvents;

		public ProjectCascadeFactory(ITranslationProviderCache translationProviderCache, ITranslationProviderCredentialStore translationProviderCredentialStore)
			: this(translationProviderCache, translationProviderCredentialStore, null)
		{
		}

		public ProjectCascadeFactory(ITranslationProviderCache translationProviderCache, ITranslationProviderCredentialStore translationProviderCredentialStore, IServerEvents serverEvents)
		{
			if (translationProviderCache == null)
			{
				throw new ArgumentNullException("translationProviderCache");
			}
			_translationProviderCache = translationProviderCache;
			_translationProviderCredentialStore = translationProviderCredentialStore;
			_serverEvents = serverEvents;
		}

		public ProjectCascade CreateCascade(ProjectCascadeSettings projectCascadeSettings, bool refreshCache)
		{
			if (projectCascadeSettings == null)
			{
				throw new ArgumentNullException("projectCascadeSettings");
			}
			ProjectCascadeItem activeProjectCascadeItem = GetActiveProjectCascadeItem(projectCascadeSettings.GeneralProjectCascadeItem, projectCascadeSettings.SpecificProjectCascadeItem);
			IList<ProjectCascadeEntryData> projectCascadeEntryDataList = CreateProjectCascadeEntryDataList(activeProjectCascadeItem);
			IList<ProjectCascadeEntryData> projectCascadeEntryDataList2 = FilterProjectCascadeEntryDataList(projectCascadeEntryDataList, projectCascadeSettings.Filter);
			IList<ProjectCascadeEntryData> projectCascadeEntryDataList3 = SortProjectCascadeEntryDataList(projectCascadeEntryDataList2, projectCascadeSettings.Sort);
			IList<ProjectCascadeEntry> cascadeEntries = CreateProjectCascadeEntries(projectCascadeEntryDataList3, projectCascadeSettings.LanguageDirection, projectCascadeSettings.ReadOnly, refreshCache);
			return new ProjectCascade(cascadeEntries, activeProjectCascadeItem.StopSearchingWhenResultsFound, projectCascadeSettings.RemoveDuplicates, CultureCode.op_Implicit(projectCascadeSettings.LanguageDirection.SourceCulture), CultureCode.op_Implicit(projectCascadeSettings.LanguageDirection.TargetCulture), activeProjectCascadeItem);
		}

		private ProjectCascadeItem GetActiveProjectCascadeItem(ProjectCascadeItem generalProjectCascadeItem, ProjectCascadeItem specificProjectCascadeItem)
		{
			if (specificProjectCascadeItem != null && specificProjectCascadeItem.OverrideParent)
			{
				return specificProjectCascadeItem;
			}
			if (generalProjectCascadeItem != null)
			{
				return generalProjectCascadeItem;
			}
			return null;
		}

		private IList<ProjectCascadeEntryData> CreateProjectCascadeEntryDataList(ProjectCascadeItem projectCascadeItem)
		{
			List<ProjectCascadeEntryData> list = new List<ProjectCascadeEntryData>();
			for (int i = 0; i < projectCascadeItem.CascadeEntryItems.Count; i++)
			{
				ProjectCascadeEntryItem val = projectCascadeItem.CascadeEntryItems[i];
				if (val.MainTranslationProviderItem != null && val.MainTranslationProviderItem.Enabled)
				{
					ProjectCascadeEntryData item = new ProjectCascadeEntryData(projectCascadeItem, i, (ProjectCascadeEntryType)0, null);
					list.Add(item);
				}
				if (val.ProjectTranslationProviderItems == null)
				{
					continue;
				}
				for (int j = 0; j < val.ProjectTranslationProviderItems.Count; j++)
				{
					ITranslationProviderItem val2 = val.ProjectTranslationProviderItems[j];
					if (val2 != null && val2.Enabled)
					{
						ProjectCascadeEntryData item2 = new ProjectCascadeEntryData(projectCascadeItem, i, (ProjectCascadeEntryType)1, j);
						list.Add(item2);
					}
				}
			}
			return list;
		}

		private IList<ProjectCascadeEntryData> FilterProjectCascadeEntryDataList(IList<ProjectCascadeEntryData> projectCascadeEntryDataList, ProjectCascadeEntryDataFilterFunction filter)
		{
			IList<ProjectCascadeEntryData> list = new List<ProjectCascadeEntryData>();
			foreach (ProjectCascadeEntryData projectCascadeEntryData in projectCascadeEntryDataList)
			{
				if (filter == null || !filter(projectCascadeEntryData))
				{
					list.Add(projectCascadeEntryData);
				}
			}
			return list;
		}

		private IList<ProjectCascadeEntryData> SortProjectCascadeEntryDataList(IList<ProjectCascadeEntryData> projectCascadeEntryDataList, IComparer<ProjectCascadeEntryData> sort)
		{
			List<ProjectCascadeEntryData> list = new List<ProjectCascadeEntryData>(projectCascadeEntryDataList);
			if (sort != null)
			{
				list.Sort(sort);
			}
			return list;
		}

		private IList<ProjectCascadeEntry> CreateProjectCascadeEntries(IList<ProjectCascadeEntryData> projectCascadeEntryDataList, LanguagePair languagePair, bool readOnly, bool refreshCache)
		{
			List<ProjectCascadeEntry> list = new List<ProjectCascadeEntry>();
			foreach (ProjectCascadeEntryData projectCascadeEntryData in projectCascadeEntryDataList)
			{
				ITranslationProviderItem translationProviderItem = projectCascadeEntryData.TranslationProviderItem;
				if (translationProviderItem != null)
				{
					bool performUpdate = !readOnly && projectCascadeEntryData.ProjectCascadeEntryItem.PerformUpdate;
					ITranslationProviderLanguageDirection translationProviderLanguageDirection = GetTranslationProviderLanguageDirection(translationProviderItem, languagePair, performUpdate, refreshCache);
					int penalty = projectCascadeEntryData.ProjectCascadeEntryItem.Penalty;
					if (translationProviderLanguageDirection != null)
					{
						ProjectCascadeEntry item = new ProjectCascadeEntry(translationProviderLanguageDirection, penalty, (IProjectCascadeEntryData)(object)projectCascadeEntryData);
						list.Add(item);
					}
				}
			}
			return list;
		}

		private ITranslationProviderLanguageDirection GetTranslationProviderLanguageDirection(ITranslationProviderItem translationProviderItem, LanguagePair languageDirection, bool performUpdate, bool refreshCache)
		{
			//IL_0037: Expected O, but got Unknown
			try
			{
				ITranslationProvider translationProvider = _translationProviderCache.GetTranslationProvider(translationProviderItem.Uri, translationProviderItem.State, _translationProviderCredentialStore, performUpdate, refreshCache);
				if (translationProvider.SupportsLanguageDirection(languageDirection))
				{
					return translationProvider.GetLanguageDirection(languageDirection);
				}
			}
			catch (TranslationProviderAuthenticationException val)
			{
				TranslationProviderAuthenticationException val2 = val;
				if (_serverEvents != null)
				{
					_serverEvents.HandleTranslationProviderAuthenticationException(translationProviderItem, val2, performUpdate);
				}
			}
			catch (Exception ex)
			{
				if (_serverEvents != null)
				{
					_serverEvents.HandleTranslationProviderException(translationProviderItem, ex);
				}
			}
			return null;
		}
	}
}
