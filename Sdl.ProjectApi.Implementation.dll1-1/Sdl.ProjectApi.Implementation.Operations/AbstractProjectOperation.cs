using System.Threading.Tasks;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public abstract class AbstractProjectOperation : IProjectOperation
	{
		protected readonly IProjectOperationSpecification ProjectOperationSpecification;

		protected AbstractProjectOperation(IProjectOperationSpecification projectOperationSpecification)
		{
			ProjectOperationSpecification = projectOperationSpecification;
		}

		public abstract IProjectOperationResult Execute(IProject project, string operationId, object[] args);

		public virtual async Task<IProjectOperationResult> ExecuteAsync(IProject project, string operationId, object[] args)
		{
			return await Task.Run(() => Execute(project, operationId, args));
		}

		public virtual bool IsAllowed(IProject project, string operationId)
		{
			return ProjectOperationSpecification.IsSatisfiedBy(project);
		}
	}
}
