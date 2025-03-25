namespace Sdl.ProjectApi.Implementation.Specification
{
	public class CompletedProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if (project == null)
			{
				return false;
			}
			return (int)project.Status == 3;
		}

		public override bool Equals(object obj)
		{
			return obj is CompletedProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 45540446;
		}
	}
}
