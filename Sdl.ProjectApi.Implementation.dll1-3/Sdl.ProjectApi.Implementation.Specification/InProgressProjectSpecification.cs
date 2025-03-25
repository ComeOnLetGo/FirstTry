namespace Sdl.ProjectApi.Implementation.Specification
{
	public class InProgressProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if (project == null)
			{
				return false;
			}
			return (int)project.Status == 2;
		}

		public override bool Equals(object obj)
		{
			return obj is InProgressProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 58774695;
		}
	}
}
