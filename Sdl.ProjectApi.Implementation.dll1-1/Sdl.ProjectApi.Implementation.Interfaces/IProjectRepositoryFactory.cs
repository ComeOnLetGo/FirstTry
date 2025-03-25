using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	public interface IProjectRepositoryFactory
	{
		IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectListItem projectListItem);

		IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, string projectFilePath);
	}
}
