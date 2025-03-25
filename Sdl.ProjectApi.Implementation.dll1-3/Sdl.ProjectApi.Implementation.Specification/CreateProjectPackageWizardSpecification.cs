namespace Sdl.ProjectApi.Implementation.Specification
{
	public class CreateProjectPackageWizardSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)project.Status == 2)
			{
				return project.AllowsOperation("CreateProjectPackageOperation");
			}
			return false;
		}
	}
}
