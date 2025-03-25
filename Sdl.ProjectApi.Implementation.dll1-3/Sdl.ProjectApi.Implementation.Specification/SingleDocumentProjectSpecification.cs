namespace Sdl.ProjectApi.Implementation.Specification
{
	public class SingleDocumentProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			if (project == null)
			{
				return false;
			}
			return project.IsInPlace;
		}

		public override bool Equals(object obj)
		{
			return obj is SingleDocumentProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 90765575;
		}
	}
}
