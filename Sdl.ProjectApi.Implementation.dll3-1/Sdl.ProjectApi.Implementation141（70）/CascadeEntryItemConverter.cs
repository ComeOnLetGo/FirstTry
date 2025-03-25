using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal static class CascadeEntryItemConverter
	{
		public static ProjectCascadeEntryItem ToObject(this CascadeEntryItem xmlCascadeEntryItem, IRelativePathManager relativePathManager)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			ITranslationProviderItem val = xmlCascadeEntryItem.MainTranslationProviderItem.ToObject(relativePathManager);
			IList<ITranslationProviderItem> list = xmlCascadeEntryItem.ProjectTranslationProviderItem.ToList().ConvertAll((TranslationProviderItem projectTranslationProviderItem) => projectTranslationProviderItem.ToObject(relativePathManager));
			bool performUpdate = xmlCascadeEntryItem.PerformUpdate;
			bool performNormalSearch = xmlCascadeEntryItem.PerformNormalSearch;
			bool performConcordanceSearch = xmlCascadeEntryItem.PerformConcordanceSearch;
			int penalty = xmlCascadeEntryItem.Penalty;
			return new ProjectCascadeEntryItem(val, list, performUpdate, performNormalSearch, performConcordanceSearch, penalty);
		}

		public static CascadeEntryItem ToXml(this ProjectCascadeEntryItem objCascadeEntryItem, IRelativePathManager relativePathManager)
		{
			CascadeEntryItem cascadeEntryItem = new CascadeEntryItem();
			cascadeEntryItem.MainTranslationProviderItem = objCascadeEntryItem.MainTranslationProviderItem.ToXml(relativePathManager);
			cascadeEntryItem.ProjectTranslationProviderItem = objCascadeEntryItem.ProjectTranslationProviderItems.ToList().ConvertAll((ITranslationProviderItem projectTranslationProviderItem) => projectTranslationProviderItem.ToXml(relativePathManager));
			cascadeEntryItem.PerformUpdate = objCascadeEntryItem.PerformUpdate;
			cascadeEntryItem.PerformNormalSearch = objCascadeEntryItem.PerformNormalSearch;
			cascadeEntryItem.PerformConcordanceSearch = objCascadeEntryItem.PerformConcordanceSearch;
			cascadeEntryItem.Penalty = objCascadeEntryItem.Penalty;
			return cascadeEntryItem;
		}
	}
}
