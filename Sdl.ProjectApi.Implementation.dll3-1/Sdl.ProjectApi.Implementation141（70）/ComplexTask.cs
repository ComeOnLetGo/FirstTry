using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	public class ComplexTask : AbstractProjectItem, IComplexTask, IAutomaticCollaborativeTask, IAutomaticTask, IScheduledTask, ITaskBase, ICollaborativeTask<ITaskFile>
	{
		private readonly List<AutomaticTask> _subTasks;

		private ITaskTemplate[] _lazySubTaskTemplates;

		private ComplexExecutionResult _lazyResult;

		private AutomaticTaskExecuter _currentAutomaticTaskExecuter;

		public TaskId Id { get; }

		public ITaskTemplate[] TaskTemplates
		{
			get
			{
				if (_lazySubTaskTemplates == null)
				{
					List<ITaskTemplate> list = new List<ITaskTemplate>();
					IScheduledTask[] subTasks = SubTasks;
					for (int i = 0; i < subTasks.Length; i++)
					{
						ScheduledTask scheduledTask = (ScheduledTask)(object)subTasks[i];
						list.AddRange(scheduledTask.TaskTemplates);
					}
					_lazySubTaskTemplates = list.ToArray();
				}
				return _lazySubTaskTemplates;
			}
		}

		public IReport[] Reports
		{
			get
			{
				List<IReport> list = new List<IReport>();
				foreach (AutomaticTask subTask in _subTasks)
				{
					list.AddRange(subTask.Reports);
				}
				return list.ToArray();
			}
		}

		public TaskStatus Status => IncrementalStatus.TaskStatus;

		public int PercentComplete => IncrementalStatus.PercentComplete;

		public string CurrentOperationDescription => null;

		public ITaskFile[] Files
		{
			get
			{
				Dictionary<string, ITaskFile> dictionary = new Dictionary<string, ITaskFile>();
				for (int num = _subTasks.Count - 1; num >= 0; num--)
				{
					IScheduledTask val = (IScheduledTask)(object)_subTasks[num];
					ITaskFile[] files = val.Files;
					foreach (ITaskFile val2 in files)
					{
						string text = val2.ProjectFile.FilenameInProject;
						IProjectFile projectFile = val2.ProjectFile;
						ITranslatableFile val3 = (ITranslatableFile)(object)((projectFile is ITranslatableFile) ? projectFile : null);
						if (val3 != null)
						{
							text = text + ":" + ((object)((IProjectFile)val3).Language).ToString();
						}
						text = text.ToLower();
						if (!dictionary.ContainsKey(text))
						{
							dictionary[text] = val2;
						}
					}
				}
				ITaskFile[] array = (ITaskFile[])(object)new ITaskFile[dictionary.Count];
				dictionary.Values.CopyTo(array, 0);
				return array;
			}
		}

		public IEnumerable<KeyValuePair<IProjectFile, FilePurpose>> OutputFiles => SubTasks[SubTasks.Length - 1].OutputFiles;

		public ITaskFile[] TranslatableFiles
		{
			get
			{
				List<ITaskFile> list = new List<ITaskFile>();
				ITaskFile[] files = Files;
				foreach (ITaskFile val in files)
				{
					IProjectFile projectFile = val.ProjectFile;
					ITranslatableFile val2 = (ITranslatableFile)(object)((projectFile is ITranslatableFile) ? projectFile : null);
					if (val2 != null)
					{
						list.Add(val);
					}
				}
				return list.ToArray();
			}
		}

		public string Name => TaskDetailMapper.GetTaskTemplateNames(TaskTemplates, null);

		public string Description => TaskDetailMapper.GetTaskTemplateDescriptions(TaskTemplates);

		public IUser CreatedBy => _subTasks[0].CreatedBy;

		public DateTime CreatedAt => _subTasks[0].CreatedAt;

		public IExecutionResult Result
		{
			get
			{
				if (_lazyResult == null)
				{
					IExecutionResult[] array = (IExecutionResult[])(object)new IExecutionResult[_subTasks.Count];
					for (int i = 0; i < _subTasks.Count; i++)
					{
						array[i] = _subTasks[i].Result;
					}
					_lazyResult = new ComplexExecutionResult(array);
				}
				return (IExecutionResult)(object)_lazyResult;
			}
		}

		public bool ForceProcessing
		{
			get
			{
				return _subTasks[0].ForceProcessing;
			}
			set
			{
				foreach (AutomaticTask subTask in _subTasks)
				{
					subTask.ForceProcessing = value;
				}
			}
		}

		public DateTime CompletedAt
		{
			get
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Invalid comparison between Unknown and I4
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Invalid comparison between Unknown and I4
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Invalid comparison between Unknown and I4
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Invalid comparison between Unknown and I4
				DateTime dateTime = DateTime.MinValue;
				IScheduledTask[] subTasks = SubTasks;
				for (int i = 0; i < subTasks.Length; i++)
				{
					AutomaticTask automaticTask = (AutomaticTask)(object)subTasks[i];
					if ((int)automaticTask.Status != 9 || (int)automaticTask.Status != 7 || (int)automaticTask.Status != 5 || (int)automaticTask.Status != 8)
					{
						return DateTime.MinValue;
					}
					if (automaticTask.CompletedAt > dateTime)
					{
						dateTime = automaticTask.CompletedAt;
					}
				}
				return dateTime;
			}
		}

		public ITaskLanguageDirection[] LanguageDirections
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IScheduledTask[] SubTasks => (IScheduledTask[])(object)_subTasks.ToArray();

		internal IncrementalStatus IncrementalStatus { get; private set; }

		public string ExternalId => _subTasks[0].ExternalId;

		public DateTime StartedAt
		{
			get
			{
				DateTime dateTime = DateTime.MinValue;
				IScheduledTask[] subTasks = SubTasks;
				for (int i = 0; i < subTasks.Length; i++)
				{
					AutomaticTask automaticTask = (AutomaticTask)(object)subTasks[i];
					if (automaticTask.StartedAt < dateTime)
					{
						dateTime = automaticTask.StartedAt;
					}
				}
				return dateTime;
			}
		}

		public string Comment
		{
			get
			{
				return _subTasks[0].Comment;
			}
			set
			{
				_subTasks[0].Comment = value;
			}
		}

		public event EventHandler StatusChanged;

		public event EventHandler<ExecutionMessageEventArgs> MessageReported;

		internal ComplexTask(IProject project, List<AutomaticTask> subTasks)
			: base(project)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			Guid[] array = new Guid[subTasks.Count];
			for (int i = 0; i < array.Length; i++)
			{
				int num = i;
				TaskId id = subTasks[i].Id;
				array[num] = ((TaskId)(ref id)).ToGuidArray()[0];
				subTasks[i].StatusChanged += ComplexTask_StatusChanged;
				subTasks[i].MessageReported += ComplexTask_MessageReported;
			}
			Id = new TaskId(array);
			_subTasks = subTasks;
			ComputeStatus();
		}

		private void ComplexTask_StatusChanged(object sender, EventArgs e)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			ComputeStatus();
			OnStatusChanged(IncrementalStatus.TaskStatus);
		}

		public void Cancel()
		{
			if (_currentAutomaticTaskExecuter != null)
			{
				_currentAutomaticTaskExecuter.Cancel();
				_currentAutomaticTaskExecuter = null;
			}
		}

		public void Start()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Invalid comparison between Unknown and I4
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Invalid comparison between Unknown and I4
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			TaskId id = Id;
			Guid[] array = ((TaskId)(ref id)).ToGuidArray();
			List<ExecutingAutomaticTask> list = new List<ExecutingAutomaticTask>();
			using (LanguageObjectsCache objectsCache = new LanguageObjectsCache())
			{
				for (int i = 0; i < array.Length; i++)
				{
					AutomaticTask automaticTask = base.Project.GetTask(new TaskId(array[i])) as AutomaticTask;
					if ((int)automaticTask.Status != 9)
					{
						if ((int)automaticTask.Status != 1 && (int)automaticTask.Status != 2)
						{
							automaticTask.ResetTaskStatus();
							automaticTask.ResetTaskFilesStatus();
						}
						list.Add(new ExecutingAutomaticTask(base.Project, automaticTask, (ILanguageObjectsCache)(object)objectsCache));
					}
				}
			}
			_currentAutomaticTaskExecuter = new AutomaticTaskExecuter(list.ToArray());
			_currentAutomaticTaskExecuter.Execute();
			_currentAutomaticTaskExecuter = null;
		}

		protected virtual void OnStatusChanged(TaskStatus status)
		{
			this.StatusChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ComplexTask_MessageReported(object sender, ExecutionMessageEventArgs e)
		{
			this.MessageReported?.Invoke(this, e);
		}

		private void ComputeStatus()
		{
			IncrementalStatus = GetTaskStatus(_subTasks);
		}

		internal IncrementalStatus GetTaskStatus(List<AutomaticTask> subTasks)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected I4, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			IncrementalStatus incrementalStatus = new IncrementalStatus();
			foreach (AutomaticTask subTask in subTasks)
			{
				TaskStatus status = subTask.Status;
				int percentComplete = 0;
				switch (status - 1)
				{
				case 2:
				case 5:
					percentComplete = subTask.PercentComplete;
					break;
				case 8:
					percentComplete = 100;
					break;
				case 3:
					percentComplete = 100;
					break;
				case 4:
					percentComplete = 100;
					break;
				case 6:
					percentComplete = 100;
					break;
				case 7:
					percentComplete = 100;
					break;
				default:
				{
					string arg = ((object)(TaskStatus)(ref status)).ToString();
					TaskId id = subTask.Id;
					throw new ProjectApiException($"Unexpected status value ({arg}) for task with ID {((object)(TaskId)(ref id)).ToString()}.");
				}
				case 0:
				case 1:
					break;
				}
				incrementalStatus.Increment(status, percentComplete);
			}
			return incrementalStatus;
		}

		public ITaskFile AddWorkFile(ILocalizableFile localizableFile)
		{
			return _subTasks[0].AddWorkFile(localizableFile);
		}

		public ITaskFile AddWorkFile(ILocalizableFile localizableFile, ITaskFile parentTaskFile)
		{
			return _subTasks[0].AddWorkFile(localizableFile, parentTaskFile);
		}

		public ITaskFile AddReferenceFile(IProjectFile projectFile)
		{
			return _subTasks[0].AddReferenceFile(projectFile);
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return _subTasks[0].AddFile(file, purpose);
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return _subTasks[0].AddFile(file, purpose, parentTaskFile);
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile, Guid taskFileId)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return _subTasks[0].AddFile(file, purpose, parentTaskFile, taskFileId);
		}

		public void RemoveFile(ITaskFile taskFile)
		{
			_subTasks[0].RemoveFile(taskFile);
		}

		public void RemoveFiles(IEnumerable<ITaskFile> taskFiles)
		{
			_subTasks[0].RemoveFiles(taskFiles);
		}

		public IReport AddReport(ITaskTemplate taskTemplate, string name, string description, string data, ILanguageDirection languageDirection, bool isCustomReport = false)
		{
			return null;
		}
	}
}
