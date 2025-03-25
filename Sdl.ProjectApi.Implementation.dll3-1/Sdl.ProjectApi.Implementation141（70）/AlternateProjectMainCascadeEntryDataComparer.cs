using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	public class AlternateProjectMainCascadeEntryDataComparer : IComparer<ProjectCascadeEntryData>
	{
		public int Compare(ProjectCascadeEntryData item0, ProjectCascadeEntryData item1)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			int num = 2 * item0.ProjectCascadeEntryItemIndex + (((int)item0.ProjectCascadeEntryType == 0) ? 1 : 0);
			int value = 2 * item1.ProjectCascadeEntryItemIndex + (((int)item1.ProjectCascadeEntryType == 0) ? 1 : 0);
			return num.CompareTo(value);
		}
	}
}
