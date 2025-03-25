using System.Threading.Tasks;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class ConditionalProjectOperation : IProjectOperation
	{
		public IProjectOperationSpecification Specification;

		public IProjectOperation TruthProcessor;

		public IProjectOperationResult Execute(IProject project, string operationId, object[] args)
		{
			if (Specification == null)
			{
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
			}
			if (TruthProcessor == null)
			{
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
			}
			if (Specification.IsSatisfiedBy(project))
			{
				return TruthProcessor.Execute(project, operationId, args);
			}
			return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
		}

		public async Task<IProjectOperationResult> ExecuteAsync(IProject project, string operationId, object[] args)
		{
			if (Specification == null)
			{
				await Task.FromResult(new SimpleBooleanProjectOperationResult(result: true));
			}
			if (TruthProcessor == null)
			{
				await Task.FromResult(new SimpleBooleanProjectOperationResult(result: true));
			}
			if (Specification.IsSatisfiedBy(project))
			{
				return await TruthProcessor.ExecuteAsync(project, operationId, args);
			}
			return (IProjectOperationResult)(object)(await Task.FromResult(new SimpleBooleanProjectOperationResult(result: true)));
		}

		public bool IsAllowed(IProject project, string operationId)
		{
			if (TruthProcessor == null)
			{
				return false;
			}
			return TruthProcessor.IsAllowed(project, operationId);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ConditionalProjectOperation conditionalProjectOperation))
			{
				return base.Equals(obj);
			}
			if (object.Equals(Specification, conditionalProjectOperation.Specification))
			{
				return object.Equals(TruthProcessor, conditionalProjectOperation.TruthProcessor);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((object)Specification).GetHashCode() ^ ((object)TruthProcessor).GetHashCode();
		}
	}
}
