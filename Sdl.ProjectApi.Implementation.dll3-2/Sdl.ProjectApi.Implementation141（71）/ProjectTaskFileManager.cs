using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectTaskFileManager : IProjectTaskFileManager
	{
		private readonly Dictionary<Guid, ITaskFile> _taskFileIndex;

		private readonly Dictionary<Guid, List<ITaskFile>> _taskFileHistoryIndex;

		public ProjectTaskFileManager()
		{
			_taskFileHistoryIndex = new Dictionary<Guid, List<ITaskFile>>();
			_taskFileIndex = new Dictionary<Guid, ITaskFile>();
		}

		public List<ITaskFile> GetTaskFileHistory(Guid projectFileId)
		{
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				return value.FindAll((ITaskFile tf) => (int)tf.Purpose == 1);
			}
			return new List<ITaskFile>(1);
		}

		public IManualTaskFile GetLastTaskFileAssignedToMe(Guid projectFileId, IUser projectCurrentUser)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ITaskBase task = value[num].Task;
					IManualTask val = (IManualTask)(object)((task is IManualTask) ? task : null);
					if (val != null && !((object)projectCurrentUser).Equals((object)val.AssignedBy))
					{
						return (IManualTaskFile)value[num];
					}
				}
			}
			return null;
		}

		public IManualTaskFile GetLastLocalTaskFileAssignedToMe(Guid projectFileId, IUser projectCurrentUser)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ITaskBase task = value[num].Task;
					IManualTask val = (IManualTask)(object)((task is IManualTask) ? task : null);
					if (val != null && !((object)projectCurrentUser).Equals((object)val.AssignedBy) && !val.ManualTaskTemplate.IsImported)
					{
						return (IManualTaskFile)value[num];
					}
				}
			}
			return null;
		}

		public IManualTaskFile GetLastTaskFileAssignedByMe(Guid projectFileId, IUser projectCurrentUser)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ITaskBase task = value[num].Task;
					IManualTask val = (IManualTask)(object)((task is IManualTask) ? task : null);
					if (val != null && ((object)projectCurrentUser).Equals((object)val.AssignedBy))
					{
						return (IManualTaskFile)value[num];
					}
				}
			}
			return null;
		}

		public ITaskFile GetCurrentTaskFile(Guid projectFileId)
		{
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				ITaskFile result = null;
				DateTime dateTime = DateTime.MinValue;
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ITaskBase task = value[num].Task;
					bool flag = task.TaskTemplates.Length != 0 && !task.TaskTemplates[0].IsLocallyExecutable;
					if ((task is IManualTask || flag) && task.CreatedAt > dateTime)
					{
						result = value[num];
						dateTime = task.CreatedAt;
					}
				}
				return result;
			}
			return null;
		}

		public ITaskFile GetCurrentCloudTaskFile(Guid projectFileId, string projectOrigin)
		{
			if (_taskFileHistoryIndex.TryGetValue(projectFileId, out var value))
			{
				ITaskFile result = null;
				DateTime dateTime = DateTime.MinValue;
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ITaskBase task = value[num].Task;
					bool flag = !(task is IManualTask) && task.TaskTemplates.Length != 0 && !task.TaskTemplates[0].IsLocallyExecutable;
					IManualTask val = (IManualTask)(object)((task is IManualTask) ? task : null);
					bool flag2 = val != null && val.ManualTaskTemplate != null && val.ManualTaskTemplate.IsImported;
					IManualTask val2 = (IManualTask)(object)((task is IManualTask) ? task : null);
					bool flag3 = val2 != null && val2.ManualTaskTemplate != null && projectOrigin.Equals("Offline cloud package");
					if ((flag2 || flag || flag3) && task.CreatedAt > dateTime)
					{
						result = value[num];
						dateTime = task.CreatedAt;
					}
				}
				return result;
			}
			return null;
		}

		public ITaskFile GetTaskFile(Guid taskFileId)
		{
			_taskFileIndex.TryGetValue(taskFileId, out var value);
			return value;
		}

		public ITaskFile Add(ITaskFile taskFile)
		{
			AddToTaskFileIndex(taskFile);
			_taskFileIndex[taskFile.Id] = taskFile;
			return taskFile;
		}

		public void Remove(ITaskFile taskFile)
		{
			RemoveFromTaskFileIndex(taskFile);
			_taskFileIndex.Remove(taskFile.Id);
		}

		private void AddToTaskFileIndex(ITaskFile taskFile)
		{
			if (!_taskFileHistoryIndex.TryGetValue(taskFile.ProjectFileId, out var value))
			{
				value = new List<ITaskFile>(1);
				_taskFileHistoryIndex[taskFile.ProjectFileId] = value;
			}
			value.Add(taskFile);
		}

		private void RemoveFromTaskFileIndex(ITaskFile taskFile)
		{
			if (_taskFileHistoryIndex.TryGetValue(taskFile.ProjectFileId, out var value))
			{
				value.Remove(taskFile);
				if (value.Count == 0)
				{
					_taskFileHistoryIndex.Remove(taskFile.ProjectFileId);
				}
			}
		}
	}
}
