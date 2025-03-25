using System;
using System.Collections.Generic;
using Sdl.Core.PluginFramework;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class WorkflowProviderRepository : IWorkflowProviderRepository
	{
		private readonly IMainRepository _mainRepository;

		public WorkflowProviderRepository(IMainRepository mainRepository)
		{
			_mainRepository = mainRepository;
		}

		public IWorkflow GetWorkflow(IPluginRegistry pluginRegistry)
		{
			return (IWorkflow)(object)new Workflow(_mainRepository.XmlProjectServer.Workflow, pluginRegistry);
		}

		public void AddOrUpdateManualTask(IManualTask task)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			TaskId id = ((ITaskBase)task).Id;
			Guid taskGuid = ((TaskId)(ref id)).ToGuidArray()[0];
			ManualTaskIndexEntry manualTaskIndexEntry = _mainRepository.XmlProjectServer.ActiveManualTaskIndex.Find((ManualTaskIndexEntry e) => e.TaskGuid == taskGuid);
			if (manualTaskIndexEntry == null)
			{
				manualTaskIndexEntry = new ManualTaskIndexEntry();
				manualTaskIndexEntry.TaskGuid = taskGuid;
				manualTaskIndexEntry.ProjectGuid = task.Project.Guid;
				_mainRepository.XmlProjectServer.ActiveManualTaskIndex.Add(manualTaskIndexEntry);
			}
			if (task.DueDate != DateTime.MinValue && task.DueDate != DateTime.MaxValue)
			{
				manualTaskIndexEntry.TaskDueDateSpecified = true;
				manualTaskIndexEntry.TaskDueDate = task.DueDate.ToUniversalTime();
			}
			else
			{
				manualTaskIndexEntry.TaskDueDateSpecified = false;
			}
		}

		public List<KeyValuePair<Guid, Guid>> GetManualTaskProjectAssociations()
		{
			List<KeyValuePair<Guid, Guid>> association = new List<KeyValuePair<Guid, Guid>>();
			_mainRepository.XmlProjectServer.ActiveManualTaskIndex.ForEach(delegate(ManualTaskIndexEntry entry)
			{
				association.Add(new KeyValuePair<Guid, Guid>(entry.TaskGuid, entry.ProjectGuid));
			});
			return association;
		}

		public void RemoveManualTask(IManualTask task)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			TaskId id = ((ITaskBase)task).Id;
			Guid taskGuid = ((TaskId)(ref id)).ToGuidArray()[0];
			int num = _mainRepository.XmlProjectServer.ActiveManualTaskIndex.FindIndex((ManualTaskIndexEntry e) => e.TaskGuid == taskGuid);
			if (num != -1)
			{
				_mainRepository.XmlProjectServer.ActiveManualTaskIndex.RemoveAt(num);
			}
		}

		public void RemoveProjectManualTask(Guid projectGuid)
		{
			for (int i = 0; i < _mainRepository.XmlProjectServer.ActiveManualTaskIndex.Count; i++)
			{
				ManualTaskIndexEntry manualTaskIndexEntry = _mainRepository.XmlProjectServer.ActiveManualTaskIndex[i];
				if (manualTaskIndexEntry.ProjectGuid == projectGuid)
				{
					_mainRepository.XmlProjectServer.ActiveManualTaskIndex.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
