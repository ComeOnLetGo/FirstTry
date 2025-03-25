using System;
using System.Collections.Generic;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.ProjectApi.Implementation.LanguageCloud.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ManualTaskBuilder : TaskBuilderBase
	{
		private string GetManualTaskComment(Task task)
		{
			if (task.ErrorDetail != null)
			{
				return task.ErrorDetail.ErrorType + " (" + task.ErrorDetail.FileAffected + ").";
			}
			return string.Empty;
		}

		public Sdl.ProjectApi.Implementation.Xml.ManualTask CreateManualTask(Task task, string dueDate)
		{
			DateTime result;
			bool flag = DateTime.TryParse(task.CreatedAtDateTime, out result);
			DateTime result2;
			bool flag2 = DateTime.TryParse(dueDate, out result2);
			Sdl.ProjectApi.Implementation.Xml.ManualTask obj = new Sdl.ProjectApi.Implementation.Xml.ManualTask
			{
				Guid = Guid.Parse(task.Id),
				CreatedBy = task.CreatedBy,
				CreatedAt = (flag ? result : DateTime.UtcNow),
				AssignedBy = task.AssignedBy,
				AssignedTo = GetAssignedTo(task),
				DueDateSpecified = false,
				PercentComplete = 0,
				Status = task.GetTaskStatus(),
				TaskTemplateIds = new List<string> { task.Type },
				Comment = GetManualTaskComment(task)
			};
			TaskDetailsModel taskDetails = task.TaskDetails;
			obj.Name = ((taskDetails != null) ? taskDetails.TaskName : null);
			TaskDetailsModel taskDetails2 = task.TaskDetails;
			obj.Description = ((taskDetails2 != null) ? taskDetails2.TaskDescription : null);
			Sdl.ProjectApi.Implementation.Xml.ManualTask manualTask = obj;
			if (flag2)
			{
				manualTask.DueDate = result2;
				manualTask.DueDateSpecified = true;
			}
			AddOrUpdateTaskFile(manualTask, task);
			return manualTask;
		}

		public void UpdateManualTask(Sdl.ProjectApi.Implementation.Xml.ManualTask manualTask, Task projectTask, string dueDate)
		{
			manualTask.AssignedBy = projectTask.AssignedBy;
			manualTask.AssignedTo = GetAssignedTo(projectTask);
			if (!string.IsNullOrWhiteSpace(dueDate))
			{
				manualTask.DueDateSpecified = true;
				manualTask.DueDate = DateTime.Parse(dueDate);
			}
			else
			{
				manualTask.DueDateSpecified = false;
			}
			manualTask.Status = projectTask.GetTaskStatus();
			if (projectTask.StartedAtDateTime == null)
			{
				manualTask.StartedAtSpecified = false;
			}
			else
			{
				manualTask.StartedAtSpecified = true;
				manualTask.StartedAt = DateTime.Parse(projectTask.StartedAtDateTime);
			}
			if (projectTask.CompletedAtDateTime == null)
			{
				manualTask.CompletedAtSpecified = false;
			}
			else
			{
				manualTask.CompletedAtSpecified = true;
				manualTask.CompletedAt = DateTime.Parse(projectTask.CompletedAtDateTime);
			}
			TaskDetailsModel taskDetails = projectTask.TaskDetails;
			manualTask.Name = ((taskDetails != null) ? taskDetails.TaskName : null);
			TaskDetailsModel taskDetails2 = projectTask.TaskDetails;
			manualTask.Description = ((taskDetails2 != null) ? taskDetails2.TaskDescription : null);
			manualTask.Comment = GetManualTaskComment(projectTask);
			AddOrUpdateTaskFile(manualTask, projectTask);
		}

		private string GetAssignedTo(Task task)
		{
			if (task.Owner != null)
			{
				return task.Owner.Id;
			}
			return task.AssignedTo;
		}
	}
}
