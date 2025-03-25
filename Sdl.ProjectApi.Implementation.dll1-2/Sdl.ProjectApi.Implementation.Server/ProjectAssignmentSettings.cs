using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectAssignmentSettings
	{
		public string ProjectPhase { get; set; }

		public string LanguageIsoCode { get; set; }

		public DateTime? DueDate { get; set; }

		public List<Assignee> Assignees { get; set; }
	}
}
