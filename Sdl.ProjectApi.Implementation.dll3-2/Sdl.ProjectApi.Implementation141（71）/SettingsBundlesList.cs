using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class SettingsBundlesList : ISettingsBundlesList
	{
		private readonly List<SettingsBundle> _xmlSettingsBundles;

		private readonly Dictionary<Guid, SettingsBundle> _xmlSettingsBundlesDictionary;

		private readonly ConcurrentDictionary<Guid, ISettingsBundle> _settingsBundles;

		public List<SettingsBundle> XmlSettingsBundles
		{
			get
			{
				Save();
				return _xmlSettingsBundles;
			}
		}

		public SettingsBundlesList(List<SettingsBundle> xmlSettingsBundles)
		{
			_xmlSettingsBundles = xmlSettingsBundles;
			_xmlSettingsBundlesDictionary = new Dictionary<Guid, SettingsBundle>(_xmlSettingsBundles.Count);
			foreach (SettingsBundle xmlSettingsBundle in xmlSettingsBundles)
			{
				_xmlSettingsBundlesDictionary[xmlSettingsBundle.Guid] = xmlSettingsBundle;
			}
			_settingsBundles = new ConcurrentDictionary<Guid, ISettingsBundle>();
		}

		public ISettingsBundle GetSettingsBundle(Guid guid, ISettingsBundle parent)
		{
			if (_settingsBundles.TryGetValue(guid, out var settingsBundle))
			{
				return settingsBundle;
			}
			if (_xmlSettingsBundlesDictionary.TryGetValue(guid, out var value))
			{
				settingsBundle = value.LoadSettingsBundle(parent);
				_settingsBundles.AddOrUpdate(guid, settingsBundle, (Guid k, ISettingsBundle v) => settingsBundle);
				return settingsBundle;
			}
			settingsBundle = SettingsUtil.CreateSettingsBundle(parent);
			_settingsBundles.AddOrUpdate(guid, settingsBundle, (Guid k, ISettingsBundle v) => settingsBundle);
			return settingsBundle;
		}

		public bool ContainsSettingsBundle(Guid guid)
		{
			if (!_settingsBundles.ContainsKey(guid))
			{
				return _xmlSettingsBundlesDictionary.ContainsKey(guid);
			}
			return true;
		}

		public void AddSettingsBundle(Guid guid, ISettingsBundle settingsBundle)
		{
			_settingsBundles[guid] = settingsBundle;
		}

		public void ImportSettingsBundle(Guid guid, ISettingsBundle settingsBundle)
		{
			_settingsBundles.TryRemove(guid, out var _);
			SerializeSettingsBundle(guid, settingsBundle);
		}

		public void DiscardCachedSettingsBundle(Guid guid)
		{
			if (_settingsBundles.TryGetValue(guid, out var value))
			{
				SerializeSettingsBundle(guid, value);
				_settingsBundles.TryRemove(guid, out var _);
			}
		}

		private void SerializeSettingsBundle(Guid guid, ISettingsBundle settingsBundle)
		{
			_xmlSettingsBundlesDictionary.TryGetValue(guid, out var value);
			if (settingsBundle != null && !settingsBundle.IsEmpty)
			{
				if (value == null)
				{
					value = new SettingsBundle();
					value.Guid = guid;
					_xmlSettingsBundles.Add(value);
					_xmlSettingsBundlesDictionary[guid] = value;
				}
				value.SaveSettingsBundle(settingsBundle);
			}
			else
			{
				if (value == null)
				{
					return;
				}
				for (int i = 0; i < _xmlSettingsBundles.Count; i++)
				{
					if (_xmlSettingsBundles[i].Guid == guid)
					{
						_xmlSettingsBundles.RemoveAt(i);
						break;
					}
				}
				_xmlSettingsBundlesDictionary.Remove(guid);
			}
		}

		public void RemoveSettingsBundle(Guid guid)
		{
			_settingsBundles.TryRemove(guid, out var _);
			for (int i = 0; i < _xmlSettingsBundles.Count; i++)
			{
				if (_xmlSettingsBundles[i].Guid == guid)
				{
					_xmlSettingsBundles.RemoveAt(i);
					break;
				}
			}
			_xmlSettingsBundlesDictionary.Remove(guid);
		}

		public void Save()
		{
			foreach (KeyValuePair<Guid, ISettingsBundle> settingsBundle in _settingsBundles)
			{
				SerializeSettingsBundle(settingsBundle.Key, settingsBundle.Value);
			}
		}

		public void SaveAndClearCache()
		{
			Save();
			_settingsBundles.Clear();
		}

		public ISettingsBundlesList Copy(List<Guid> settingsBundleGuids, Dictionary<Guid, Guid> mappedGuids)
		{
			Save();
			List<SettingsBundle> list = new List<SettingsBundle>();
			foreach (Guid settingsBundleGuid in settingsBundleGuids)
			{
				if (!(settingsBundleGuid == Guid.Empty) && _xmlSettingsBundlesDictionary.TryGetValue(settingsBundleGuid, out var value))
				{
					SettingsBundle settingsBundle = value.Copy();
					Guid value2 = Guid.Empty;
					if (mappedGuids == null || mappedGuids.TryGetValue(settingsBundleGuid, out value2))
					{
						settingsBundle.Guid = value2;
					}
					list.Add(settingsBundle);
				}
			}
			return (ISettingsBundlesList)(object)new SettingsBundlesList(list);
		}
	}
}
