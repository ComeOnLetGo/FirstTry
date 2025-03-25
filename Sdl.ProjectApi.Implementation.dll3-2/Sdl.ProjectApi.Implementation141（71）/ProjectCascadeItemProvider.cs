using System.Collections.Generic;
using System.Linq;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascadeItemProvider
	{
		private readonly IProject _project;

		public ProjectCascadeItemProvider(IProject project)
		{
			_project = project;
		}

		public ProjectCascadeItem GetCascadeItemWithoutRedundantEntries()
		{
			ProjectCascadeItem val = ((IProjectConfiguration)_project).CascadeItem.Copy();
			RemoveCascadeEntries(val, GetRedundantCascadeEntries());
			return val;
		}

		private IEnumerable<ProjectCascadeEntryItem> GetRedundantCascadeEntries()
		{
			return ((IProjectConfiguration)_project).CascadeItem.CascadeEntryItems.Where(IsRedundantCascadeEntry).ToList();
		}

		private bool IsRedundantCascadeEntry(ProjectCascadeEntryItem cascadeEntryItem)
		{
			if (IsFileBasedTranslationMemoryCascadeEntry(cascadeEntryItem))
			{
				ILanguageDirection[] languageDirections = ((IProjectConfiguration)_project).LanguageDirections;
				foreach (ILanguageDirection val in languageDirections)
				{
					if (val.CascadeItem == null || !val.CascadeItem.OverrideParent)
					{
						continue;
					}
					foreach (ProjectCascadeEntryItem cascadeEntryItem2 in val.CascadeItem.CascadeEntryItems)
					{
						if (IsFileBasedTranslationMemoryCascadeEntry(cascadeEntryItem2) && object.Equals(cascadeEntryItem2.MainTranslationProviderItem, cascadeEntryItem.MainTranslationProviderItem))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool IsFileBasedTranslationMemoryCascadeEntry(ProjectCascadeEntryItem cascadeEntry)
		{
			if (cascadeEntry != null && cascadeEntry.MainTranslationProviderItem != null)
			{
				return FileBasedTranslationMemory.IsFileBasedTranslationMemory(cascadeEntry.MainTranslationProviderItem.Uri);
			}
			return false;
		}

		private void RemoveCascadeEntries(ProjectCascadeItem projectCascade, IEnumerable<ProjectCascadeEntryItem> cascadeEntries)
		{
			foreach (ProjectCascadeEntryItem cascadeEntry in cascadeEntries)
			{
				RemoveCascadeEntry(projectCascade, cascadeEntry);
			}
		}

		private void RemoveCascadeEntry(ProjectCascadeItem projectCascade, ProjectCascadeEntryItem cascadeEntryItem)
		{
			for (int i = 0; i < projectCascade.CascadeEntryItems.Count; i++)
			{
				ProjectCascadeEntryItem val = projectCascade.CascadeEntryItems[i];
				if (object.Equals(val.MainTranslationProviderItem, cascadeEntryItem.MainTranslationProviderItem))
				{
					projectCascade.CascadeEntryItems.RemoveAt(i);
					break;
				}
			}
		}
	}
}
