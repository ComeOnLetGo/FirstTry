using Sdl.BestMatchServiceStudioIntegration.Common;

namespace Sdl.ProjectApi.Implementation.Specification
{
	public class RefreshableProjectSpecification : IProjectOperationSpecification
	{
		private readonly ILanguageCloudService _languageCloudService;

		public RefreshableProjectSpecification(ILanguageCloudService languageCloudService)
		{
			_languageCloudService = languageCloudService;
		}

		public bool IsSatisfiedBy(IProject project)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			if (project.IsLCProject && ((int)project.Status == 1 || (int)project.Status == 2) && !string.IsNullOrEmpty(project.AccountId))
			{
				return _languageCloudService.ApiContext.SelectedTenantId.Equals(project.AccountId);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return obj is RefreshableProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 55076389;
		}
	}
}
