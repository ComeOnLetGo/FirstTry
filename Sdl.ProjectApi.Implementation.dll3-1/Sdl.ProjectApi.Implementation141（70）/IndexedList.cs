using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sdl.ProjectApi.Implementation
{
	public class IndexedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : INotifyPropertyChanged
	{
		private readonly List<T> _innerList;

		private readonly ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);

		private ListChangedEventHandler onListChanged;

		public T this[int index]
		{
			get
			{
				return _innerList[index];
			}
			set
			{
				_innerList[index] = value;
			}
		}

		public int Count => _innerList.Count;

		public bool IsReadOnly => false;

		public event ListChangedEventHandler ListChanged
		{
			add
			{
				onListChanged = (ListChangedEventHandler)Delegate.Combine(onListChanged, value);
			}
			remove
			{
				onListChanged = (ListChangedEventHandler)Delegate.Remove(onListChanged, value);
			}
		}

		public IndexedList()
		{
			_innerList = new List<T>();
		}

		protected virtual void OnListChanged(ListChangedEventArgs ev)
		{
			if (onListChanged != null)
			{
				onListChanged(this, ev);
			}
		}

		protected void OnClearComplete()
		{
			OnListChanged(resetEvent);
		}

		protected void OnInsertComplete(int index, object value)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}

		protected void OnRemoveComplete(int index, object value)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		}

		protected void OnSetComplete(int index, object oldValue, object newValue)
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
		}

		public int IndexOf(T item)
		{
			return _innerList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_innerList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
		}

		public void Add(T item)
		{
			_innerList.Add(item);
		}

		public void Clear()
		{
			_innerList.Clear();
		}

		public bool Contains(T item)
		{
			return _innerList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_innerList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return _innerList.Remove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}
	}
}
