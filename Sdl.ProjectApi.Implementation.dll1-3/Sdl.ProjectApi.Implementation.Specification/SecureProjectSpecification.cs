namespace Sdl.ProjectApi.Implementation.Specification
{
	public class SecureProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			return project.IsSecure;
		}

		public override bool Equals(object obj)
		{
			return obj is SecureProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 91294932;
		}
	}
}
