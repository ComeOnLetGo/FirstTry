using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectMainCascadeEntryDataComparer : IComparer<ProjectCascadeEntryData>
	{
		public int Compare(ProjectCascadeEntryData item0, ProjectCascadeEntryData item1)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			int num = item0.ProjectCascadeEntryItemIndex + (((int)item0.ProjectCascadeEntryType == 0) ? 1024 : 0);
			int value = item1.ProjectCascadeEntryItemIndex + (((int)item1.ProjectCascadeEntryType == 0) ? 1024 : 0);
			return num.CompareTo(value);
		}
	}
}
