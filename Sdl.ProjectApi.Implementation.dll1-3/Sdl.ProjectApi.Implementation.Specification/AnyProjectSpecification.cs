using System.Collections.Generic;
using System.Linq;

namespace Sdl.ProjectApi.Implementation.Specification
{
	public class AnyProjectSpecification : IProjectOperationSpecification
	{
		private readonly List<IProjectOperationSpecification> _specifications;

		public AnyProjectSpecification()
		{
			_specifications = new List<IProjectOperationSpecification>();
		}

		public AnyProjectSpecification(List<IProjectOperationSpecification> specifications)
		{
			_specifications = specifications;
		}

		public bool IsSatisfiedBy(IProject project)
		{
			if (_specifications != null && _specifications.Count != 0)
			{
				return _specifications.Any((IProjectOperationSpecification s) => s.IsSatisfiedBy(project));
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			return obj is AnyProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 49517017;
		}
	}
}
