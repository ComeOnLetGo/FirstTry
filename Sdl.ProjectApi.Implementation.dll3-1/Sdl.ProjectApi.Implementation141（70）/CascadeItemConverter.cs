using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal static class CascadeItemConverter
	{
		public static ProjectCascadeItem ToObject(this CascadeItem xmlCascadeItem, IRelativePathManager relativePathManager)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (xmlCascadeItem != null)
			{
				IList<ProjectCascadeEntryItem> list = xmlCascadeItem.CascadeEntryItem.ConvertAll((CascadeEntryItem xmlCascadeEntryItem) => xmlCascadeEntryItem.ToObject(relativePathManager));
				bool stopSearchingWhenResultsFound = xmlCascadeItem.StopSearchingWhenResultsFound;
				bool overrideParent = xmlCascadeItem.OverrideParent;
				return new ProjectCascadeItem(list, stopSearchingWhenResultsFound, overrideParent);
			}
			return new ProjectCascadeItem();
		}

		public static CascadeItem ToXml(this ProjectCascadeItem objCascadeItem, IRelativePathManager relativePathManager)
		{
			if (objCascadeItem == null)
			{
				return new CascadeItem();
			}
			CascadeItem cascadeItem = new CascadeItem();
			cascadeItem.CascadeEntryItem = objCascadeItem.CascadeEntryItems.ToList().ConvertAll((ProjectCascadeEntryItem objCascadeEntryItem) => objCascadeEntryItem.ToXml(relativePathManager));
			cascadeItem.StopSearchingWhenResultsFound = objCascadeItem.StopSearchingWhenResultsFound;
			cascadeItem.OverrideParent = objCascadeItem.OverrideParent;
			return cascadeItem;
		}
	}
}
