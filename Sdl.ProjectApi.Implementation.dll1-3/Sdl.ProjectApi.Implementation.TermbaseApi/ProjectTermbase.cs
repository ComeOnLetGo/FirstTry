using System;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbase : IProjectTermbase, ICopyable<IProjectTermbase>
	{
		private bool _enabled;

		public string Name { get; }

		public string SettingsXml { get; }

		public IProjectTermbaseFilter Filter { get; set; }

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				bool flag = value != _enabled;
				_enabled = value;
				if (flag)
				{
					this.EnabledChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler EnabledChanged;

		public ProjectTermbase(string name, string settingsXml, IProjectTermbaseFilter filter, bool enabled)
		{
			Name = name;
			SettingsXml = settingsXml;
			Filter = filter;
			Enabled = enabled;
		}

		public bool IsServerTermbase()
		{
			TermbaseSettings val = TermbaseSettings.FromXml(SettingsXml);
			if (!val.Local)
			{
				return !val.IsCustom;
			}
			return false;
		}

		public bool IsLocalTermbase()
		{
			TermbaseSettings val = TermbaseSettings.FromXml(SettingsXml);
			return val.Local;
		}

		public IProjectTermbase Copy()
		{
			return (IProjectTermbase)(object)new ProjectTermbase(Name, SettingsXml, Filter, Enabled);
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTermbase projectTermbase && object.Equals(Name, projectTermbase.Name) && object.Equals(SettingsXml, projectTermbase.SettingsXml) && object.Equals(Filter, projectTermbase.Filter))
			{
				return object.Equals(Enabled, projectTermbase.Enabled);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = 71;
			num += ((Name != null) ? (313 * Name.GetHashCode()) : 0);
			num += ((SettingsXml != null) ? (937 * SettingsXml.GetHashCode()) : 0);
			return num + ((Filter != null) ? (1971 * ((object)Filter).GetHashCode()) : 0);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
