using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal interface IGroupshareSyncOperation
	{
		void SyncData(IProject project, Sdl.ProjectApi.Implementation.Xml.Project xmlProject, IProjectRepository projectRepository);
	}
}
