namespace Sdl.ProjectApi.Implementation.Specification
{
	public class InCloudProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)project.ProjectType == 11)
			{
				return !project.IsProjectDisabled;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return obj is InCloudProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 16605681;
		}
	}
}
