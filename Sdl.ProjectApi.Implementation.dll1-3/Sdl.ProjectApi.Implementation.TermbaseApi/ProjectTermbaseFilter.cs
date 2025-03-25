using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseFilter : IProjectTermbaseFilter, ICopyable<IProjectTermbaseFilter>
	{
		private readonly int _id;

		private readonly string _name;

		public int Id => _id;

		public string Name => _name;

		public ProjectTermbaseFilter(int id, string name)
		{
			_id = id;
			_name = name;
		}

		public IProjectTermbaseFilter Copy()
		{
			return (IProjectTermbaseFilter)(object)new ProjectTermbaseFilter(_id, _name);
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTermbaseFilter projectTermbaseFilter)
			{
				return object.Equals(_id, projectTermbaseFilter._id);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 61 + 197 * _id;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
