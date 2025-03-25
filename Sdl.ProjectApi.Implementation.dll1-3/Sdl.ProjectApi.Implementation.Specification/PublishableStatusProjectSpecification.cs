using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation.Specification
{
	public class PublishableStatusProjectSpecification : IProjectOperationSpecification
	{
		public bool IsSatisfiedBy(IProject project)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Invalid comparison between Unknown and I4
			IPublishProjectOperation publishProjectOperation = project.PublishProjectOperation;
			if (publishProjectOperation == null)
			{
				return true;
			}
			return (int)publishProjectOperation.Status == 7 || (int)publishProjectOperation.Status == 5 || (int)publishProjectOperation.Status == 0;
		}

		public override bool Equals(object obj)
		{
			return obj is PublishableStatusProjectSpecification;
		}

		public override int GetHashCode()
		{
			return 2016755;
		}
	}
}
