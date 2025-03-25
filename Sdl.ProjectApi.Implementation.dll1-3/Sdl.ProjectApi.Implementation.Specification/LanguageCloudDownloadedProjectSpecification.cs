namespace Sdl.ProjectApi.Implementation.Specification
{
	public class LanguageCloudDownloadedProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			return !project.IsCloudBased;
		}

		public override bool Equals(object obj)
		{
			return obj is LanguageCloudDownloadedProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 37224365;
		}
	}
}
