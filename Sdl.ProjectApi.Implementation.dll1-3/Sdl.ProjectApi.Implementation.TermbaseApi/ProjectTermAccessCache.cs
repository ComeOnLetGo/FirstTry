using System;
using System.Collections.Generic;
using Sdl.MultiTerm.Client.TermAccess;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermAccessCache
	{
		private class TermAccessParameters
		{
			private readonly IList<string> _termbaseSettingsXml;

			private readonly Uri _serverConnectionUri;

			public TermAccessParameters(IList<string> termbaseSettingsXml, Uri serverConnectionUri)
			{
				_termbaseSettingsXml = termbaseSettingsXml;
				_serverConnectionUri = serverConnectionUri;
			}

			public override bool Equals(object obj)
			{
				if (obj is TermAccessParameters termAccessParameters && EqualsTermbaseSettingsXml(_termbaseSettingsXml, termAccessParameters._termbaseSettingsXml))
				{
					return object.Equals(_serverConnectionUri, termAccessParameters._serverConnectionUri);
				}
				return false;
			}

			private bool EqualsTermbaseSettingsXml(IList<string> termbaseSettingsXml0, IList<string> termbaseSettingsXml1)
			{
				if (termbaseSettingsXml0 == null || termbaseSettingsXml1 == null)
				{
					return termbaseSettingsXml0 == termbaseSettingsXml1;
				}
				if (termbaseSettingsXml0.Count != termbaseSettingsXml1.Count)
				{
					return false;
				}
				for (int i = 0; i < termbaseSettingsXml0.Count; i++)
				{
					if (!object.Equals(termbaseSettingsXml0[i], termbaseSettingsXml1[i]))
					{
						return false;
					}
				}
				return true;
			}

			public override int GetHashCode()
			{
				int num = 41;
				num += ((_termbaseSettingsXml != null) ? (371 * GetHashCodeTermbaseSettingsXml(_termbaseSettingsXml)) : 23);
				return num + ((_serverConnectionUri != null) ? (1093 * _serverConnectionUri.GetHashCode()) : 457);
			}

			private int GetHashCodeTermbaseSettingsXml(IList<string> termbaseSettingsXml)
			{
				int num = 17;
				for (int i = 0; i < termbaseSettingsXml.Count; i++)
				{
					string text = termbaseSettingsXml[i];
					num += (71 * i + 23) * text.GetHashCode();
				}
				return num;
			}
		}

		private static ProjectTermAccessCache _instance;

		private readonly IDictionary<TermAccessParameters, TermAccess> _termAccesses;

		private readonly IDictionary<TermAccessParameters, DateTime> _termAccessTimes;

		private readonly int _capacity;

		public static ProjectTermAccessCache Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ProjectTermAccessCache();
				}
				return _instance;
			}
		}

		private ProjectTermAccessCache()
		{
			_termAccesses = new Dictionary<TermAccessParameters, TermAccess>();
			_termAccessTimes = new Dictionary<TermAccessParameters, DateTime>();
			_capacity = 4;
		}

		public TermAccess GetTermAccess(IList<string> termbaseSettingsXml, Uri serverConnectionUri)
		{
			TermAccessParameters key = new TermAccessParameters(termbaseSettingsXml, serverConnectionUri);
			if (_termAccesses.ContainsKey(key))
			{
				return _termAccesses[key];
			}
			return null;
		}

		public void AddTermAccess(IList<string> termbaseSettingsXml, Uri serverConnectionUri, TermAccess termAccess)
		{
			if (_termAccesses.Count == _capacity)
			{
				RemoveTermAccess();
			}
			TermAccessParameters key = new TermAccessParameters(termbaseSettingsXml, serverConnectionUri);
			_termAccesses[key] = termAccess;
			_termAccessTimes[key] = DateTime.Now;
		}

		private void RemoveTermAccess()
		{
			TermAccessParameters termAccessParameters = null;
			DateTime dateTime = DateTime.MaxValue;
			foreach (KeyValuePair<TermAccessParameters, DateTime> termAccessTime in _termAccessTimes)
			{
				if (termAccessTime.Value < dateTime)
				{
					termAccessParameters = termAccessTime.Key;
					dateTime = termAccessTime.Value;
				}
			}
			if (termAccessParameters != null)
			{
				_termAccesses.Remove(termAccessParameters);
				_termAccessTimes.Remove(termAccessParameters);
			}
		}
	}
}
