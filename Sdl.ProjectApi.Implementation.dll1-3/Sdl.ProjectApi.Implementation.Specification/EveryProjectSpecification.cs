using System.Collections.Generic;
using System.Linq;

namespace Sdl.ProjectApi.Implementation.Specification
{
	public class EveryProjectSpecification : IProjectOperationSpecification
	{
		public List<IProjectOperationSpecification> Specifications { get; set; }

		public EveryProjectSpecification(List<IProjectOperationSpecification> specifications)
		{
			Specifications = specifications;
		}

		public bool IsSatisfiedBy(IProject project)
		{
			if (Specifications != null && Specifications.Count > 0)
			{
				return Specifications.TrueForAll((IProjectOperationSpecification s) => s.IsSatisfiedBy(project));
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is EveryProjectSpecification everyProjectSpecification))
			{
				return base.Equals(obj);
			}
			return Specifications.SequenceEqual(everyProjectSpecification.Specifications);
		}

		public override int GetHashCode()
		{
			return 667;
		}
	}
}
