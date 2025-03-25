using System.Collections.Generic;
using System.Threading.Tasks;
using Sdl.ProjectApi.Implementation.Specification;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class BasicOperationComposite : IProjectOperation
	{
		private readonly IApplication _application;

		private readonly IProjectPathUtil _projectPathUtil;

		private readonly ProjectOperationComposite _operationComposite;

		public BasicOperationComposite(IApplication application, IProjectPathUtil projectPathUtil)
		{
			_application = application;
			_projectPathUtil = projectPathUtil;
			_operationComposite = new ProjectOperationComposite(RegisterBasicOperations());
		}

		private List<IProjectOperation> RegisterBasicOperations()
		{
			NotSpecification notSpecification = new NotSpecification((IProjectOperationSpecification)(object)new LanguageCloudProjectSpecification());
			NotSpecification item = new NotSpecification((IProjectOperationSpecification)(object)new SecureProjectSpecification());
			EveryProjectSpecification specification = new EveryProjectSpecification(new List<IProjectOperationSpecification>
			{
				(IProjectOperationSpecification)(object)item,
				(IProjectOperationSpecification)(object)notSpecification
			});
			List<IProjectOperation> list = new List<IProjectOperation>
			{
				(IProjectOperation)(object)new RevertFilesToSdlXliffProjectOperation((IProjectOperationSpecification)(object)notSpecification),
				(IProjectOperation)(object)new CreateProjectPackageOperation((IProjectOperationSpecification)(object)specification, _projectPathUtil)
			};
			List<IProjectOperation> serverProjectOperations = _application.CommuteClientManager.GetServerProjectOperations(_projectPathUtil);
			list.AddRange(serverProjectOperations);
			return list;
		}

		public IProjectOperationResult Execute(IProject project, string operationId, object[] args)
		{
			return _operationComposite.Execute(project, operationId, args);
		}

		public async Task<IProjectOperationResult> ExecuteAsync(IProject project, string operationId, object[] args)
		{
			return await _operationComposite.ExecuteAsync(project, operationId, args);
		}

		public bool IsAllowed(IProject project, string operationId)
		{
			return _operationComposite.IsAllowed(project, operationId);
		}
	}
}
