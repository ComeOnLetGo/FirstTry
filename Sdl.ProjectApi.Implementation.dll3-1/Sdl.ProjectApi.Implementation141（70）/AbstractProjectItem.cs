namespace Sdl.ProjectApi.Implementation
{
	public class AbstractProjectItem
	{
		private Project _castCache;

		public IProject Project { get; set; }

		internal Project ProjectImpl => _castCache ?? (_castCache = Project as Project);

		protected AbstractProjectItem(IProject project)
		{
			Project = project;
		}
	}
}
