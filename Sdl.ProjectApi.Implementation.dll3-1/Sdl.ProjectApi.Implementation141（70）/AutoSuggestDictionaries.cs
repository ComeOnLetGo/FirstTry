using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sdl.ProjectApi.Implementation
{
	public class AutoSuggestDictionaries : Collection<IAutoSuggestDictionary>, IAutoSuggestDictionaries, IList<IAutoSuggestDictionary>, ICollection<IAutoSuggestDictionary>, IEnumerable<IAutoSuggestDictionary>, IEnumerable
	{
		private bool _suspendEvents;

		private bool _autoSuggestDictionariesChanged;

		public event EventHandler<AutoSuggestDictionaryAddedEventArgs> AutoSuggestDictionaryAdded;

		public event EventHandler<AutoSuggestDictionaryRemovedEventArgs> AutoSuggestDictionaryRemoved;

		public event EventHandler<AutoSuggestDictionariesChangedEventArgs> AutoSuggestDictionariesChanged;

		public IAutoSuggestDictionary CreateAutoSuggestDictionary(string filePath)
		{
			return (IAutoSuggestDictionary)(object)new AutoSuggestDictionary(filePath);
		}

		public void SuspendAutoSuggestDictionaryEvents()
		{
			if (_suspendEvents)
			{
				throw new InvalidOperationException();
			}
			_suspendEvents = true;
		}

		public void ResumeAutoSuggestDictionaryEvents()
		{
			if (!_suspendEvents)
			{
				throw new InvalidOperationException();
			}
			_suspendEvents = false;
			if (_autoSuggestDictionariesChanged)
			{
				OnAutoSuggestDictionariesChanged();
			}
		}

		protected override void ClearItems()
		{
			IList<IAutoSuggestDictionary> list = new List<IAutoSuggestDictionary>(this);
			base.ClearItems();
			foreach (IAutoSuggestDictionary item in list)
			{
				OnAutoSuggestDictionaryRemoved(item);
			}
		}

		protected override void InsertItem(int index, IAutoSuggestDictionary item)
		{
			base.InsertItem(index, item);
			OnAutoSuggestDictionaryAdded(item);
		}

		protected override void RemoveItem(int index)
		{
			IAutoSuggestDictionary autoSuggestDictionaryRemoved = base[index];
			base.RemoveItem(index);
			OnAutoSuggestDictionaryRemoved(autoSuggestDictionaryRemoved);
		}

		protected override void SetItem(int index, IAutoSuggestDictionary item)
		{
			IAutoSuggestDictionary autoSuggestDictionaryRemoved = base[index];
			base.SetItem(index, item);
			OnAutoSuggestDictionaryRemoved(autoSuggestDictionaryRemoved);
			OnAutoSuggestDictionaryAdded(item);
		}

		private void OnAutoSuggestDictionaryAdded(IAutoSuggestDictionary autoSuggestDictionaryAdded)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			_autoSuggestDictionariesChanged = true;
			if (!_suspendEvents && this.AutoSuggestDictionaryAdded != null)
			{
				this.AutoSuggestDictionaryAdded(this, new AutoSuggestDictionaryAddedEventArgs(autoSuggestDictionaryAdded));
				OnAutoSuggestDictionariesChanged();
			}
		}

		private void OnAutoSuggestDictionaryRemoved(IAutoSuggestDictionary autoSuggestDictionaryRemoved)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			_autoSuggestDictionariesChanged = true;
			if (!_suspendEvents && this.AutoSuggestDictionaryRemoved != null)
			{
				this.AutoSuggestDictionaryRemoved(this, new AutoSuggestDictionaryRemovedEventArgs(autoSuggestDictionaryRemoved));
				OnAutoSuggestDictionariesChanged();
			}
		}

		private void OnAutoSuggestDictionariesChanged()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			if (!_suspendEvents && this.AutoSuggestDictionariesChanged != null)
			{
				this.AutoSuggestDictionariesChanged(this, new AutoSuggestDictionariesChangedEventArgs());
				_autoSuggestDictionariesChanged = false;
			}
		}
	}
}
