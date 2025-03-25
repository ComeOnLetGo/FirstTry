using System;
using System.Collections;
using System.Collections.Generic;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseLanguageIndexes : ItemCollection<IProjectTermbaseLanguageIndex>, IProjectTermbaseLanguageIndexes, IItemCollection<IProjectTermbaseLanguageIndex>, IList<IProjectTermbaseLanguageIndex>, ICollection<IProjectTermbaseLanguageIndex>, IEnumerable<IProjectTermbaseLanguageIndex>, IEnumerable, ICopyable<IProjectTermbaseLanguageIndexes>
	{
		public event EventHandler<ProjectTermbaseIndexChangedEventArgs> TermbaseIndexChanged;

		public IProjectTermbaseLanguageIndexes Copy()
		{
			IProjectTermbaseLanguageIndexes val = (IProjectTermbaseLanguageIndexes)(object)new ProjectTermbaseLanguageIndexes();
			using IEnumerator<IProjectTermbaseLanguageIndex> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				IProjectTermbaseLanguageIndex current = enumerator.Current;
				((ICollection<IProjectTermbaseLanguageIndex>)val).Add(((ICopyable<IProjectTermbaseLanguageIndex>)(object)current).Copy());
			}
			return val;
		}

		protected override void ClearItems()
		{
			using (IEnumerator<IProjectTermbaseLanguageIndex> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IProjectTermbaseLanguageIndex current = enumerator.Current;
					if (current != null)
					{
						current.TermbaseIndexChanged -= languageIndex_TermbaseIndexChanged;
					}
				}
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, IProjectTermbaseLanguageIndex item)
		{
			base.InsertItem(index, item);
			if (item != null)
			{
				item.TermbaseIndexChanged += languageIndex_TermbaseIndexChanged;
			}
		}

		protected override void RemoveItem(int index)
		{
			IProjectTermbaseLanguageIndex val = base[index];
			if (val != null)
			{
				val.TermbaseIndexChanged -= languageIndex_TermbaseIndexChanged;
			}
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, IProjectTermbaseLanguageIndex item)
		{
			IProjectTermbaseLanguageIndex val = base[index];
			if (val != null)
			{
				val.TermbaseIndexChanged -= languageIndex_TermbaseIndexChanged;
			}
			base.SetItem(index, item);
			if (item != null)
			{
				item.TermbaseIndexChanged += languageIndex_TermbaseIndexChanged;
			}
		}

		private void languageIndex_TermbaseIndexChanged(object sender, ProjectTermbaseIndexChangedEventArgs eventArgs)
		{
			if (this.TermbaseIndexChanged != null)
			{
				this.TermbaseIndexChanged(sender, eventArgs);
			}
		}
	}
}
