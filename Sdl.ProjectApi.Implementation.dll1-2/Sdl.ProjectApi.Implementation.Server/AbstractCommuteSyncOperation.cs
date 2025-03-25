using System;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;

namespace Sdl.ProjectApi.Implementation.Server
{
	public abstract class AbstractCommuteSyncOperation : ICommuteSyncOperation
	{
		public abstract string Description { get; }

		public abstract bool IsFullProjectUpdate { get; }

		public abstract bool ShouldExecute();

		public abstract void Execute();

		protected static bool IsServerAvailable(IProject project)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Invalid comparison between Unknown and I4
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			string absoluteUri = project.PublishProjectOperation.UnqualifiedServerUri.AbsoluteUri;
			if (!IdentityInfoCache.Default.ContainsKey(absoluteUri))
			{
				return false;
			}
			if (!project.PublishProjectOperation.OriginalServerUserName.Equals(project.PublishProjectOperation.ServerUserName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			ConnectionInfo connectionInfo = IdentityInfoCache.Default.GetConnectionInfo(absoluteUri);
			if ((int)connectionInfo.ConnectionStatus == 1)
			{
				return (int)connectionInfo.AuthenticationStatus == 2;
			}
			return false;
		}
	}
}
