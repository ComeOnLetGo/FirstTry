using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	public interface IProjectTemplateRepositoryFactory
	{
		IProjectTemplateRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectTemplateListItem projectTemplateListItem);

		IProjectTemplateRepository Create(IApplication application, IProjectPathUtil projectPathUtil);
	}
}
