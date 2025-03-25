using System.Collections.Generic;
using System.Globalization;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascade : Cascade<ProjectCascadeEntry>
	{
		public ProjectCascadeItem ProjectCascadeItem { get; private set; }

		public bool ContainsProjectTranslationMemories
		{
			get
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Invalid comparison between Unknown and I4
				foreach (ProjectCascadeEntry cascadeEntry in base.CascadeEntries)
				{
					if ((int)cascadeEntry.ProjectCascadeEntryData.ProjectCascadeEntryType == 1)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool ContainsMainTranslationMemories
		{
			get
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				foreach (ProjectCascadeEntry cascadeEntry in base.CascadeEntries)
				{
					if ((int)cascadeEntry.ProjectCascadeEntryData.ProjectCascadeEntryType == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ProjectCascade(IEnumerable<ProjectCascadeEntry> cascadeEntries, bool stopSearchingWhenResultsFound, bool removeDuplicates, CultureInfo sourceLanguage, CultureInfo targetLanguage, ProjectCascadeItem projectCascadeItem)
			: base(cascadeEntries, stopSearchingWhenResultsFound, removeDuplicates, CultureCode.op_Implicit(sourceLanguage), CultureCode.op_Implicit(targetLanguage))
		{
			ProjectCascadeItem = projectCascadeItem;
		}

		public IList<ProjectCascadeEntry> FindProjectCascadeEntries(ProjectCascadeEntry mainCascadeEntry)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Invalid comparison between Unknown and I4
			IList<ProjectCascadeEntry> list = new List<ProjectCascadeEntry>();
			if (mainCascadeEntry != null)
			{
				ITranslationProviderItem mainTranslationProviderItem = mainCascadeEntry.ProjectCascadeEntryData.ProjectCascadeEntryItem.MainTranslationProviderItem;
				foreach (ProjectCascadeEntry cascadeEntry in base.CascadeEntries)
				{
					if ((int)cascadeEntry.ProjectCascadeEntryData.ProjectCascadeEntryType == 1)
					{
						ProjectCascadeEntryItem projectCascadeEntryItem = cascadeEntry.ProjectCascadeEntryData.ProjectCascadeEntryItem;
						if (projectCascadeEntryItem.MainTranslationProviderItem.Equals((object)mainTranslationProviderItem))
						{
							list.Add(cascadeEntry);
						}
					}
				}
			}
			return list;
		}
	}
}
