using System.Collections;
using System.Collections.Generic;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseIndexes : ItemCollection<IProjectTermbaseIndex>, IProjectTermbaseIndexes, IItemCollection<IProjectTermbaseIndex>, IList<IProjectTermbaseIndex>, ICollection<IProjectTermbaseIndex>, IEnumerable<IProjectTermbaseIndex>, IEnumerable, ICopyable<IProjectTermbaseIndexes>
	{
		public IProjectTermbaseIndexes Copy()
		{
			IProjectTermbaseIndexes val = (IProjectTermbaseIndexes)(object)new ProjectTermbaseIndexes();
			using IEnumerator<IProjectTermbaseIndex> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				IProjectTermbaseIndex current = enumerator.Current;
				((ICollection<IProjectTermbaseIndex>)val).Add(((ICopyable<IProjectTermbaseIndex>)(object)current).Copy());
			}
			return val;
		}
	}
}
