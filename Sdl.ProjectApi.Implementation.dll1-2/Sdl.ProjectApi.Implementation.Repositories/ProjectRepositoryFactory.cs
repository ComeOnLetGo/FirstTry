using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class ProjectRepositoryFactory : IProjectRepositoryFactory
	{
		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectListItem projectListItem)
		{
			return new ProjectRepository(application, projectPathUtil);
		}

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, string projectFilePath)
		{
			return new ProjectRepository(application, projectPathUtil);
		}
	}
}
