using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbases : ItemCollection<IProjectTermbase>, IProjectTermbases, IItemCollection<IProjectTermbase>, IList<IProjectTermbase>, ICollection<IProjectTermbase>, IEnumerable<IProjectTermbase>, IEnumerable, ICopyable<IProjectTermbases>
	{
		private readonly TerminologyProviderSettings _settings;

		public event EventHandler<ItemsMovedEventArgs<IProjectTermbase>> ItemsMoved;

		public event EventHandler<ItemEnabledEventArgs<IProjectTermbase>> ItemEnabled;

		public ProjectTermbases(TerminologyProviderSettings settings)
		{
			_settings = settings;
			base.ItemAdded += ProjectTermbases_ItemAdded;
			base.ItemRemoved += ProjectTermbases_ItemRemoved;
		}

		public IProjectTermbase GetDefaultTermbase()
		{
			if (base.Count == 0)
			{
				return null;
			}
			return base[0];
		}

		public bool SetDefaultTermbase(IProjectTermbase termbase)
		{
			if (CanSetDefaultTermbase(termbase))
			{
				try
				{
					_suspendAddedRemovedEvents = true;
					int num = IndexOf(termbase);
					if (num > 0)
					{
						RemoveAt(num);
						Insert(0, termbase);
						IList<IProjectTermbase> list = new List<IProjectTermbase>();
						for (int i = 0; i <= num; i++)
						{
							list.Add(base[i]);
						}
						OnItemsMoved(list);
						return true;
					}
				}
				finally
				{
					_suspendAddedRemovedEvents = false;
				}
			}
			return false;
		}

		public bool CanSetDefaultTermbase(IProjectTermbase termbase)
		{
			if (termbase == null)
			{
				return false;
			}
			if (!Contains(termbase))
			{
				return false;
			}
			if (termbase == GetDefaultTermbase())
			{
				return false;
			}
			if (!termbase.Enabled)
			{
				return false;
			}
			return true;
		}

		public bool MoveUp(IProjectTermbase termbase)
		{
			if (CanMoveUp(termbase))
			{
				int num = IndexOf(termbase);
				Swap(num, num - 1);
				return true;
			}
			return false;
		}

		public bool MoveDown(IProjectTermbase termbase)
		{
			if (CanMoveDown(termbase))
			{
				int num = IndexOf(termbase);
				Swap(num, num + 1);
				return true;
			}
			return false;
		}

		public bool CanMoveUp(IProjectTermbase termbase)
		{
			return IndexOf(termbase) > ((!termbase.Enabled) ? 1 : 0);
		}

		public bool CanMoveDown(IProjectTermbase termbase)
		{
			int num = IndexOf(termbase);
			if (num > -1)
			{
				return num < base.Count - 1;
			}
			return false;
		}

		private void Swap(int index0, int index1)
		{
			try
			{
				_suspendAddedRemovedEvents = true;
				IProjectTermbase val = base[index0];
				IProjectTermbase val2 = base[index1];
				base[index1] = val;
				base[index0] = val2;
				UpdateOrder();
				OnItemsMoved(new List<IProjectTermbase> { val, val2 });
			}
			finally
			{
				_suspendAddedRemovedEvents = false;
			}
		}

		private void UpdateOrder()
		{
			if (_settings != null)
			{
				_settings.TermbasesOrder.Value = this.Select((IProjectTermbase termbaseXml) => termbaseXml.Name).ToList();
			}
		}

		private void OnItemsMoved(IList<IProjectTermbase> items)
		{
			if (this.ItemsMoved != null)
			{
				this.ItemsMoved(this, new ItemsMovedEventArgs<IProjectTermbase>(items));
			}
		}

		protected override void InsertItem(int index, IProjectTermbase item)
		{
			if (!CanAdd(item))
			{
				throw new ArgumentException("Cannot add item; a termbase with the same name or the same settings already exists.", "item");
			}
			base.InsertItem(index, item);
			UpdateOrder();
		}

		public bool CanAdd(IProjectTermbase termbase)
		{
			using (IEnumerator<IProjectTermbase> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IProjectTermbase current = enumerator.Current;
					if (object.Equals(current.Name, termbase.Name) || object.Equals(current.SettingsXml, termbase.SettingsXml))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool HasServerTermbase()
		{
			using (IEnumerator<IProjectTermbase> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IProjectTermbase current = enumerator.Current;
					if (current.IsServerTermbase())
					{
						return true;
					}
				}
			}
			return false;
		}

		public IProjectTermbases Copy()
		{
			IProjectTermbases val = (IProjectTermbases)(object)new ProjectTermbases(_settings);
			using (IEnumerator<IProjectTermbase> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IProjectTermbase current = enumerator.Current;
					((ICollection<IProjectTermbase>)val).Add(((ICopyable<IProjectTermbase>)(object)current).Copy());
				}
			}
			UpdateOrder();
			return val;
		}

		private void ProjectTermbases_ItemRemoved(object sender, ItemCollectionRemovedEventArgs<IProjectTermbase> e)
		{
			((ItemCollectionEventArgs<IProjectTermbase>)(object)e).Item.EnabledChanged -= Item_EnabledChanged;
		}

		private void ProjectTermbases_ItemAdded(object sender, ItemCollectionAddedEventArgs<IProjectTermbase> e)
		{
			((ItemCollectionEventArgs<IProjectTermbase>)(object)e).Item.EnabledChanged += Item_EnabledChanged;
		}

		private void Item_EnabledChanged(object sender, EventArgs e)
		{
			IProjectTermbase val = (IProjectTermbase)((sender is IProjectTermbase) ? sender : null);
			if (val == null)
			{
				return;
			}
			IProjectTermbase defaultTermbase = GetDefaultTermbase();
			if (((object)val).Equals((object)defaultTermbase))
			{
				using IEnumerator<IProjectTermbase> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					IProjectTermbase current = enumerator.Current;
					if (CanSetDefaultTermbase(current))
					{
						SetDefaultTermbase(current);
						break;
					}
				}
			}
			else if (!defaultTermbase.Enabled)
			{
				SetDefaultTermbase(val);
			}
			if (this.ItemEnabled != null)
			{
				this.ItemEnabled(this, new ItemEnabledEventArgs<IProjectTermbase>(val));
			}
		}
	}
}
