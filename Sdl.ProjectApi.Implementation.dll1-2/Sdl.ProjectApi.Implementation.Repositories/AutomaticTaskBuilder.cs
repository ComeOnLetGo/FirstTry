using System;
using System.Collections.Generic;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.ProjectApi.Implementation.LanguageCloud.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class AutomaticTaskBuilder : TaskBuilderBase
	{
		public void UpdateAutomaticTask(Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask, Task projectTask)
		{
			automaticTask.Status = projectTask.GetTaskStatus();
			if (projectTask.StartedAtDateTime == null)
			{
				automaticTask.StartedAtSpecified = false;
			}
			else
			{
				automaticTask.StartedAtSpecified = true;
				automaticTask.StartedAt = DateTime.Parse(projectTask.StartedAtDateTime);
			}
			if (projectTask.CompletedAtDateTime == null)
			{
				automaticTask.CompletedAtSpecified = false;
			}
			else
			{
				automaticTask.CompletedAtSpecified = true;
				automaticTask.CompletedAt = DateTime.Parse(projectTask.CompletedAtDateTime);
			}
			TaskDetailsModel taskDetails = projectTask.TaskDetails;
			automaticTask.Name = ((taskDetails != null) ? taskDetails.TaskName : null);
			TaskDetailsModel taskDetails2 = projectTask.TaskDetails;
			automaticTask.Description = ((taskDetails2 != null) ? taskDetails2.TaskDescription : null);
			automaticTask.Comment = projectTask.Comment;
			AddOrUpdateTaskFile(automaticTask, projectTask);
		}

		public Sdl.ProjectApi.Implementation.Xml.AutomaticTask CreateAutomaticTask(Task task)
		{
			Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask = new Sdl.ProjectApi.Implementation.Xml.AutomaticTask
			{
				Guid = Guid.Parse(task.Id),
				CreatedAt = ((task.CreatedAtDateTime == null) ? DateTime.UtcNow : DateTime.Parse(task.CreatedAtDateTime)),
				CreatedBy = task.CreatedBy,
				PercentComplete = 0,
				TaskTemplateIds = new List<string> { task.Type }
			};
			UpdateAutomaticTask(automaticTask, task);
			AddOrUpdateTaskFile(automaticTask, task);
			return automaticTask;
		}
	}
}
