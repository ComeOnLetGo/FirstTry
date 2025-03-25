namespace Sdl.ProjectApi.Implementation.Specification
{
	public class LanguageCloudProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			if ((int)project.ProjectType != 8 && (int)project.ProjectType != 11)
			{
				return (int)project.ProjectType == 12;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			return obj is LanguageCloudProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 39433575;
		}
	}
}
