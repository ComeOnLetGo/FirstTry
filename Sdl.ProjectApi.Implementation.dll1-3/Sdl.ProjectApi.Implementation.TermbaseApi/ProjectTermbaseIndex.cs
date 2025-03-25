using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseIndex : IProjectTermbaseIndex, ICopyable<IProjectTermbaseIndex>
	{
		public string Name { get; }

		public ProjectTermbaseIndex(string name)
		{
			Name = name;
		}

		public IProjectTermbaseIndex Copy()
		{
			return (IProjectTermbaseIndex)(object)new ProjectTermbaseIndex(Name);
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTermbaseIndex projectTermbaseIndex)
			{
				return object.Equals(Name, projectTermbaseIndex.Name);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = 17;
			return num + ((Name != null) ? (291 * Name.GetHashCode()) : 0);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
