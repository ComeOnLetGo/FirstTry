namespace Sdl.ProjectApi.Implementation.Events
{
	public class SelectedProjectChangedEvent
	{
		public IProject SelectedProject { get; set; }

		public SelectedProjectChangedEvent(IProject selectedProject)
		{
			SelectedProject = selectedProject;
		}
	}
}
