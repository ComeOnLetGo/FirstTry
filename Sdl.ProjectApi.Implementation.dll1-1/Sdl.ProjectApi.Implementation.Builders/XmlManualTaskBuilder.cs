using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Builders
{
	internal class XmlManualTaskBuilder
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.ManualTask _manualTask;

		public XmlManualTaskBuilder(Guid taskId, string taskName, string taskDescription, DateTime createdAt)
		{
			_manualTask = new Sdl.ProjectApi.Implementation.Xml.ManualTask
			{
				Guid = taskId,
				CreatedAt = createdAt.ToUniversalTime(),
				Name = taskName,
				Description = taskDescription,
				PercentComplete = 0,
				Status = TaskStatus.Created
			};
		}

		public XmlManualTaskBuilder WithExternalId(string externalId)
		{
			_manualTask.ExternalId = externalId;
			return this;
		}

		public XmlManualTaskBuilder WithPercentComplete(int percentComplete)
		{
			_manualTask.PercentComplete = percentComplete;
			return this;
		}

		public XmlManualTaskBuilder WithDueDate(DateTime dueDate)
		{
			if (dueDate != DateTime.MaxValue)
			{
				_manualTask.DueDate = dueDate.ToUniversalTime();
				_manualTask.DueDateSpecified = true;
			}
			return this;
		}

		public XmlManualTaskBuilder WithStartedAt(DateTime startedAt)
		{
			if (startedAt > DateTime.MinValue)
			{
				_manualTask.StartedAt = startedAt.ToUniversalTime();
				_manualTask.StartedAtSpecified = true;
			}
			return this;
		}

		public XmlManualTaskBuilder WithCompletedAt(DateTime completedAt)
		{
			if (completedAt > DateTime.MinValue)
			{
				_manualTask.CompletedAt = completedAt.ToUniversalTime();
				_manualTask.CompletedAtSpecified = true;
			}
			return this;
		}

		public XmlManualTaskBuilder WithTemplate(IManualTaskTemplate template, ProjectConfiguration projectConfiguration)
		{
			_manualTask.TaskTemplateIds.Add(((ITaskTemplate)template).Id);
			AddManualTaskTemplate(template, projectConfiguration);
			return this;
		}

		protected void AddManualTaskTemplate(IManualTaskTemplate template, ProjectConfiguration projectConfiguration)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate manualTaskTemplate = projectConfiguration.ManualTaskTemplates.Find((Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate t) => t.Id == ((ITaskTemplate)template).Id);
			if (manualTaskTemplate == null)
			{
				manualTaskTemplate = new Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate();
				manualTaskTemplate.Id = ((ITaskTemplate)template).Id;
				projectConfiguration.ManualTaskTemplates.Add(manualTaskTemplate);
			}
			manualTaskTemplate.Name = ((ITaskTemplate)template).Name;
			manualTaskTemplate.Description = ((ITaskTemplate)template).Description;
			if ((int)((ITaskTemplate)template).GeneratedFileType != 0)
			{
				manualTaskTemplate.GeneratedFileType = EnumConvert.ConvertTaskFileType(((ITaskTemplate)template).GeneratedFileType);
				manualTaskTemplate.GeneratedFileTypeSpecified = true;
			}
			else
			{
				manualTaskTemplate.GeneratedFileTypeSpecified = false;
			}
			manualTaskTemplate.WorkflowStageType = EnumConvert.ConvertWorkflowStageType(template.WorkflowStage);
		}

		public XmlManualTaskBuilder WithCreatedBy(IUser createdBy)
		{
			if (createdBy != null)
			{
				_manualTask.CreatedBy = createdBy.UserId;
			}
			return this;
		}

		public Sdl.ProjectApi.Implementation.Xml.ManualTask Build()
		{
			return _manualTask;
		}
	}
}
