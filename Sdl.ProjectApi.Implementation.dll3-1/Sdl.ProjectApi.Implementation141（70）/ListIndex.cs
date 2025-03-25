using System.ComponentModel;

namespace Sdl.ProjectApi.Implementation
{
	public class ListIndex<T, V> where T : INotifyPropertyChanged
	{
		private readonly IndexedList<T> _list;

		private readonly string _propertyName;

		public ListIndex(IndexedList<T> list, GetListItemValueDelegate<T, V> getValueDelegate, string propertyName)
		{
			_list = list;
			_propertyName = propertyName;
		}

		public T[] GetItems(V value)
		{
			return null;
		}
	}
}
