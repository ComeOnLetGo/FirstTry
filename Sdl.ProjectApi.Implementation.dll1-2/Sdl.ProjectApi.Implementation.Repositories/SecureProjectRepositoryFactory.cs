using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.SecureProjects;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class SecureProjectRepositoryFactory : IProjectRepositoryFactory
	{
		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectListItem projectListItem)
		{
			if (projectListItem?.ProjectInfo == null)
			{
				return null;
			}
			if (!projectListItem.ProjectInfo.IsSecure)
			{
				return null;
			}
			return new SecureProjectRepository(application, projectPathUtil);
		}

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, string projectFilePath)
		{
			if (SecureProjectUtil.IsSecureProject(projectFilePath))
			{
				return new SecureProjectRepository(application, projectPathUtil);
			}
			return null;
		}
	}
}
