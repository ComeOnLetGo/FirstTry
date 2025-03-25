namespace Sdl.ProjectApi.Implementation.Events
{
	public class ProjectTypeChangedEvent
	{
		public IProject Project { get; set; }

		public ProjectTypeChangedEvent(IProject project)
		{
			Project = project;
		}
	}
}
