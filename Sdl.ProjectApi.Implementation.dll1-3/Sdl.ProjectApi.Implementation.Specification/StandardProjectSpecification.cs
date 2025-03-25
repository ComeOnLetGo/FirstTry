namespace Sdl.ProjectApi.Implementation.Specification
{
	public class StandardProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			if (project == null)
			{
				return true;
			}
			return !project.IsInPlace;
		}

		public override bool Equals(object obj)
		{
			return obj is StandardProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 7982215;
		}
	}
}
