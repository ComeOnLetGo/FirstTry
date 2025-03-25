using System.Collections;

namespace Sdl.ProjectApi.Implementation
{
	internal class FileRoleComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Invalid comparison between Unknown and I4
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Invalid comparison between Unknown and I4
			IProjectFile val = (IProjectFile)x;
			IProjectFile val2 = (IProjectFile)y;
			if (val.Guid != val2.Guid)
			{
				if ((int)val.FileRole == 4 && (int)val2.FileRole != 4)
				{
					return 1;
				}
				if ((int)val.FileRole != 4 && (int)val2.FileRole == 4)
				{
					return -1;
				}
				return val.Guid.CompareTo(val2.Guid);
			}
			return 0;
		}
	}
}
