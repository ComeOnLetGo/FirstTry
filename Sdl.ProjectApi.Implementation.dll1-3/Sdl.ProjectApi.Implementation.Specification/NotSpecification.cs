using System;

namespace Sdl.ProjectApi.Implementation.Specification
{
	public class NotSpecification : IProjectOperationSpecification
	{
		private readonly IProjectOperationSpecification _specification;

		public NotSpecification(IProjectOperationSpecification specification)
		{
			_specification = specification ?? throw new ArgumentNullException();
		}

		public bool IsSatisfiedBy(IProject project)
		{
			return !_specification.IsSatisfiedBy(project);
		}

		public override bool Equals(object obj)
		{
			return obj is NotSpecification;
		}

		public override int GetHashCode()
		{
			return ((object)_specification).GetHashCode();
		}
	}
}
