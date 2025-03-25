using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	public class FileAssignmentInfo : IFileAssignmentInfo
	{
		public string PhaseName { get; set; }

		public bool IsCurrentAssignment { get; set; }

		public DateTime? AssignedAt { get; set; }

		public DateTime? DueDate { get; set; }

		public IUser AssignedBy { get; set; }

		public List<IUser> Assignees { get; set; }
	}
}
