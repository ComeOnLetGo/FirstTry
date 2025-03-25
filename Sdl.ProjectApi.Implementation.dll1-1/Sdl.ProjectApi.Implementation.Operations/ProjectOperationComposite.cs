using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class ProjectOperationComposite : IProjectOperation
	{
		public List<IProjectOperation> ProjectOperations { get; }

		public ProjectOperationComposite(List<IProjectOperation> operations)
		{
			ProjectOperations = operations ?? throw new ArgumentNullException();
		}

		public IProjectOperationResult Execute(IProject project, string operationId, object[] args)
		{
			IProjectOperation val = ProjectOperations.FirstOrDefault((IProjectOperation o) => o.IsAllowed(project, operationId));
			if (val == null)
			{
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: false);
			}
			return val.Execute(project, operationId, args);
		}

		public async Task<IProjectOperationResult> ExecuteAsync(IProject project, string operationId, object[] args)
		{
			IProjectOperation val = ProjectOperations.FirstOrDefault((IProjectOperation o) => o.IsAllowed(project, operationId));
			return (IProjectOperationResult)(object)((val == null) ? (await Task.FromResult(new SimpleBooleanProjectOperationResult(result: false))) : ((SimpleBooleanProjectOperationResult)(object)(await val.ExecuteAsync(project, operationId, args))));
		}

		public bool IsAllowed(IProject project, string operationId)
		{
			return ProjectOperations.Any((IProjectOperation o) => o.IsAllowed(project, operationId));
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ProjectOperationComposite projectOperationComposite))
			{
				return base.Equals(obj);
			}
			return ProjectOperations.SequenceEqual(projectOperationComposite.ProjectOperations);
		}

		public override int GetHashCode()
		{
			return ProjectOperations.Select((IProjectOperation n) => ((object)n).GetHashCode()).Aggregate((int x, int y) => x ^ y);
		}
	}
}
