using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ItemCollection<T> : Collection<T>, IItemCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		protected bool _suspendAddedRemovedEvents;

		public event EventHandler<ItemCollectionAddedEventArgs<T>> ItemAdded;

		public event EventHandler<ItemCollectionRemovedEventArgs<T>> ItemRemoved;

		protected override void ClearItems()
		{
			IList<T> list = new List<T>(this);
			base.ClearItems();
			foreach (T item in list)
			{
				OnItemRemoved(item);
			}
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			OnItemAdded(item);
		}

		protected override void RemoveItem(int index)
		{
			T item = base[index];
			base.RemoveItem(index);
			OnItemRemoved(item);
		}

		protected override void SetItem(int index, T item)
		{
			T item2 = base[index];
			base.SetItem(index, item);
			OnItemRemoved(item2);
			OnItemAdded(item);
		}

		private void OnItemAdded(T item)
		{
			if (this.ItemAdded != null && !_suspendAddedRemovedEvents)
			{
				this.ItemAdded(this, new ItemCollectionAddedEventArgs<T>(item));
			}
		}

		private void OnItemRemoved(T item)
		{
			if (this.ItemRemoved != null && !_suspendAddedRemovedEvents)
			{
				this.ItemRemoved(this, new ItemCollectionRemovedEventArgs<T>(item));
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ItemCollection<T> itemCollection))
			{
				return false;
			}
			if (base.Count != itemCollection.Count)
			{
				return false;
			}
			for (int i = 0; i < base.Count; i++)
			{
				if (!base[i].Equals(itemCollection[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
