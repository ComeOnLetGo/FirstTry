using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectOperationFactory : IProjectOperationFactory
	{
		private readonly List<IProjectOperationFactory> _operationFactories;

		public ProjectOperationFactory(List<IProjectOperationFactory> operationFactories)
		{
			_operationFactories = operationFactories;
		}

		public IProjectOperation Resolve(string id, IProject project)
		{
			foreach (IProjectOperationFactory operationFactory in _operationFactories)
			{
				IProjectOperation val = operationFactory.Resolve(id, project);
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}
	}
}
