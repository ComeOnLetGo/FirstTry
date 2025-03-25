namespace Sdl.ProjectApi.Implementation.Specification
{
	public class CloudDownloadedProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			return project.IsLCProject;
		}

		public override bool Equals(object obj)
		{
			return obj is CloudDownloadedProjectSpecification;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
