using System;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseConfigurationFactory : IProjectTermbaseConfigurationFactory
	{
		public IProjectTermbaseServer CreateTermbaseServer(Uri serverConnectionUri)
		{
			return (IProjectTermbaseServer)(object)new ProjectTermbaseServer(serverConnectionUri);
		}

		public IProjectTermbase CreateTermbase(string name, string settingsXml, IProjectTermbaseFilter filter, bool enabled)
		{
			return (IProjectTermbase)(object)new ProjectTermbase(name, settingsXml, filter, enabled);
		}

		public IProjectTermbaseIndexes CreateTermbaseIndexes()
		{
			return (IProjectTermbaseIndexes)(object)new ProjectTermbaseIndexes();
		}

		public IProjectTermbaseIndex CreateTermbaseIndex(string name)
		{
			return (IProjectTermbaseIndex)(object)new ProjectTermbaseIndex(name);
		}

		public IProjectTermbaseLanguageIndex CreateTermbaseLanguageIndex(Language language, IProjectTermbaseIndex termbaseIndex)
		{
			return (IProjectTermbaseLanguageIndex)(object)new ProjectTermbaseLanguageIndex(language, termbaseIndex);
		}

		public IProjectTermbaseFilter CreateTermbaseFilter(int id, string name)
		{
			return (IProjectTermbaseFilter)(object)new ProjectTermbaseFilter(id, name);
		}
	}
}
