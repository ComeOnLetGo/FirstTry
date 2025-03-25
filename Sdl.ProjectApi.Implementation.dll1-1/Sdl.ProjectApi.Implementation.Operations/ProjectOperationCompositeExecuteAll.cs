using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class ProjectOperationCompositeExecuteAll : IProjectOperation
	{
		public List<IProjectOperation> Nodes;

		public IProjectOperationResult Execute(IProject project, string operationId, object[] args)
		{
			IProject val = project;
			if (Nodes == null)
			{
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
			}
			foreach (IProjectOperation node in Nodes)
			{
				IProjectOperationResult val2 = node.Execute(val, operationId, args);
				if (!val2.IsSuccesful)
				{
					return val2;
				}
				if (val2.Result is IProject)
				{
					object result = val2.Result;
					val = (IProject)((result is IProject) ? result : null);
				}
			}
			return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
		}

		public async Task<IProjectOperationResult> ExecuteAsync(IProject project, string operationId, object[] args)
		{
			IProject projectParam = project;
			if (Nodes == null)
			{
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
			}
			foreach (IProjectOperation node in Nodes)
			{
				IProjectOperationResult val = await node.ExecuteAsync(projectParam, operationId, args);
				if (!val.IsSuccesful)
				{
					return val;
				}
				if (val.Result is IProject)
				{
					object result = val.Result;
					projectParam = (IProject)((result is IProject) ? result : null);
				}
			}
			return (IProjectOperationResult)(object)(await Task.FromResult(new SimpleBooleanProjectOperationResult(result: true)));
		}

		public bool IsAllowed(IProject project, string operationId)
		{
			return Nodes.Any((IProjectOperation o) => o.IsAllowed(project, operationId));
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ProjectOperationCompositeExecuteAll projectOperationCompositeExecuteAll))
			{
				return base.Equals(obj);
			}
			return Nodes.SequenceEqual(projectOperationCompositeExecuteAll.Nodes);
		}

		public override int GetHashCode()
		{
			return Nodes.Select((IProjectOperation n) => ((object)n).GetHashCode()).Aggregate((int x, int y) => x ^ y);
		}
	}
}
