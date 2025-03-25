using System;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal class DefaultBackgroundProjectUpdater : IBackgroundProjectUpdater
	{
		public void PerformProjectUpdate(IProject project, Action updateAction)
		{
			updateAction();
		}
	}
}
