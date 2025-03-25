using System;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseServer : IProjectTermbaseServer, ICopyable<IProjectTermbaseServer>
	{
		private readonly Uri _serverConnectionUri;

		public Uri ServerConnectionUri => _serverConnectionUri;

		public ProjectTermbaseServer(Uri serverConnectionUri)
		{
			_serverConnectionUri = serverConnectionUri;
		}

		public IProjectTermbaseServer Copy()
		{
			return (IProjectTermbaseServer)(object)new ProjectTermbaseServer(_serverConnectionUri);
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTermbaseServer projectTermbaseServer && projectTermbaseServer._serverConnectionUri != null == (_serverConnectionUri != null))
			{
				if (!(projectTermbaseServer._serverConnectionUri == null))
				{
					return object.Equals(projectTermbaseServer._serverConnectionUri, _serverConnectionUri);
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 13 + 117 * _serverConnectionUri.GetHashCode();
		}
	}
}
