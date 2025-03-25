using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sdl.MultiTerm.Client.TermAccess;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermAccessFactory
	{
		private bool IsValid(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			if (termbaseConfiguration == null)
			{
				throw new ArgumentNullException("termbaseConfiguration");
			}
			if (termbaseConfiguration.Termbases == null)
			{
				return false;
			}
			if (((ICollection<IProjectTermbase>)termbaseConfiguration.Termbases).Count == 0)
			{
				return false;
			}
			if (termbaseConfiguration.Termbases.GetDefaultTermbase() == null)
			{
				return false;
			}
			return true;
		}

		private IList<string> GetTermbaseSettingsXml(IProjectTermbases termbases, bool defaultOnly)
		{
			IList<string> list = new List<string>();
			if (!defaultOnly)
			{
				foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbases)
				{
					if (item.Enabled)
					{
						list.Add(item.SettingsXml);
					}
				}
			}
			else
			{
				IProjectTermbase defaultTermbase = termbases.GetDefaultTermbase();
				if (defaultTermbase != null && defaultTermbase.Enabled)
				{
					list.Add(defaultTermbase.SettingsXml);
				}
			}
			return list;
		}

		public TermAccess GetCachedTermAccess(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			if (!IsValid(termbaseConfiguration))
			{
				return null;
			}
			try
			{
				IList<string> termbaseSettingsXml = GetTermbaseSettingsXml(termbaseConfiguration.Termbases, defaultOnly: false);
				TermAccess val = ProjectTermAccessCache.Instance.GetTermAccess(termbaseSettingsXml, GetServerConnectionUri(termbaseConfiguration));
				if (val == null)
				{
					val = CreateCachedTermAccess(termbaseConfiguration);
					if (val == null)
					{
						return null;
					}
					ProjectTermAccessCache.Instance.AddTermAccess(termbaseSettingsXml, GetServerConnectionUri(termbaseConfiguration), val);
				}
				return val;
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError(Logging.DefaultLog, ex, "An error occured whilst getting the cached term access object.", Array.Empty<object>());
				return null;
			}
		}

		private Uri GetServerConnectionUri(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			if (termbaseConfiguration == null || termbaseConfiguration.TermbaseServer == null)
			{
				return null;
			}
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbaseConfiguration.Termbases)
			{
				if (item.IsServerTermbase() && item.Enabled)
				{
					return termbaseConfiguration.TermbaseServer.ServerConnectionUri;
				}
			}
			return null;
		}

		private TermAccess CreateCachedTermAccess(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			IList<string> termbaseSettingsXml = GetTermbaseSettingsXml(termbaseConfiguration.Termbases, defaultOnly: false);
			TermAccess val = new TermAccess((IEnumerable<string>)termbaseSettingsXml, GetServerConnectionUri(termbaseConfiguration));
			if (val.HasFailedLogin)
			{
				foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbaseConfiguration.Termbases)
				{
					if (item.IsServerTermbase())
					{
						item.Enabled = false;
					}
				}
				return null;
			}
			return val;
		}
	}
}
