namespace Sdl.ProjectApi.Implementation.Specification
{
	public class SaveToCloudSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			if ((int)project.ProjectType == 0 || (int)project.ProjectType == 2)
			{
				return (int)project.Status == 2;
			}
			return false;
		}
	}
}
