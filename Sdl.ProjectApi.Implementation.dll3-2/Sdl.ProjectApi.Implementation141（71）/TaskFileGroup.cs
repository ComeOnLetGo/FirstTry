using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdl.ProjectApi.Implementation
{
	internal class TaskFileGroup
	{
		private readonly IManualTask _task;

		private readonly List<ITaskFile> _files = new List<ITaskFile>();

		public IManualTask Task => _task;

		public List<ITaskFile> Files => _files;

		public static IEnumerable<TaskFileGroup> GroupTaskFilesByTask(ITaskFile[] taskFiles)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<Guid, TaskFileGroup> dictionary = new Dictionary<Guid, TaskFileGroup>();
			foreach (ITaskFile val in taskFiles)
			{
				IManualTask val2 = (IManualTask)val.Task;
				TaskId id = ((ITaskBase)val2).Id;
				Guid key = ((TaskId)(ref id)).ToGuidArray()[0];
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new TaskFileGroup(val2));
				}
				value.Files.Add(val);
			}
			return dictionary.Values;
		}

		public static IEnumerable<TaskFileGroup> GroupImportOperationTaskFiles(IProjectPackageImport[] projectPackageImports, ITaskFile[] taskFiles)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<TaskId, TaskFileGroup> dictionary = new Dictionary<TaskId, TaskFileGroup>();
			foreach (IProjectPackageImport val in projectPackageImports)
			{
				IManualTask[] tasks = ((IPackageOperation)val).Tasks;
				foreach (IManualTask val2 in tasks)
				{
					if (!dictionary.TryGetValue(((ITaskBase)val2).Id, out var value))
					{
						value = new TaskFileGroup(val2);
						dictionary[((ITaskBase)val2).Id] = value;
					}
					value.Files.AddRange((IEnumerable<ITaskFile>)val2.Files.Where((IManualTaskFile file) => taskFiles.Any((ITaskFile taskFile) => taskFile.ProjectFileId.Equals(((ITaskFile)file).ProjectFileId))));
				}
			}
			return dictionary.Values;
		}

		public TaskFileGroup(IManualTask task)
		{
			_task = task;
		}
	}
}
