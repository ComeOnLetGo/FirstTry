using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Events
{
	public class SelectedFilesChangedEvent
	{
		public List<IProjectFile> SelectedFiles { get; }

		public bool IsSourceFileSelected { get; }

		public Guid ProjectGuid { get; }

		public SelectedFilesChangedEvent(Guid projectGuid, List<IProjectFile> selectedFiles, bool isSourceFileSelected = false)
		{
			ProjectGuid = projectGuid;
			SelectedFiles = selectedFiles;
			IsSourceFileSelected = isSourceFileSelected;
		}
	}
}
