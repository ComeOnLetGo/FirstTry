namespace Sdl.ProjectApi.Implementation.Specification
{
	public class DisabledProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			return project.IsProjectDisabled;
		}
	}
}
