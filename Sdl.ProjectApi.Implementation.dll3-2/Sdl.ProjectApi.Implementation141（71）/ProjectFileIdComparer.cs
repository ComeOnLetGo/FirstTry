using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectFileIdComparer : IEqualityComparer<IProjectFile>
	{
		public bool Equals(IProjectFile x, IProjectFile y)
		{
			return x.Guid == y.Guid;
		}

		public int GetHashCode(IProjectFile obj)
		{
			return obj.Guid.GetHashCode();
		}
	}
}
