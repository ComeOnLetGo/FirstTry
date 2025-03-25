using System;

namespace Sdl.ProjectApi.Implementation.Events
{
	public class UpdateFileDetailsEvent
	{
		public Guid ProjectId { get; }

		public UpdateFileDetailsEvent(Guid projectId)
		{
			ProjectId = projectId;
		}
	}
}
