using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectTemplateRepositoryFactory : IProjectTemplateRepositoryFactory
	{
		public IProjectTemplateRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectTemplateListItem projectTemplateListItem)
		{
			return (IProjectTemplateRepository)(object)new ProjectTemplateRepository(application, projectPathUtil, projectTemplateListItem);
		}

		public IProjectTemplateRepository Create(IApplication application, IProjectPathUtil projectPathUtil)
		{
			return (IProjectTemplateRepository)(object)new ProjectTemplateRepository(application, projectPathUtil);
		}
	}
}
