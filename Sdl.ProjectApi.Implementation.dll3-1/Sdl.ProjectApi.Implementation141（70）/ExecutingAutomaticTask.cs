using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ExecutingAutomaticTask : AbstractProjectItem, IExecutingAutomaticTask, ITaskBase, IExecutionMessageReporter
	{
		private class NewTaskFilesComparer : IComparer<IProjectFile>
		{
			public int Compare(IProjectFile x, IProjectFile y)
			{
				ILocalizableFile val = (ILocalizableFile)(object)((x is ILocalizableFile) ? x : null);
				if (val != null)
				{
					ILocalizableFile val2 = (ILocalizableFile)(object)((y is ILocalizableFile) ? y : null);
					if (val2 != null && !((object)((IProjectFile)val).Language).Equals((object)((IProjectFile)val2).Language))
					{
						return ((LanguageBase)((IProjectFile)val).Language).IsoAbbreviation.CompareTo(((LanguageBase)((IProjectFile)val2).Language).IsoAbbreviation);
					}
				}
				int num = x.PathInProject.CompareTo(y.PathInProject);
				if (num == 0)
				{
					return x.FilenameInProject.CompareTo(y.FilenameInProject);
				}
				return num;
			}
		}

		private ExecutingTaskFile[] _lazyExecutingTaskFiles;

		private readonly object _lockObject = new object();

		private string workingFolder;

		public ILanguageObjectsCache ObjectsCache { get; private set; }

		public IExecutingTaskFile[] Files
		{
			get
			{
				lock (_lockObject)
				{
					if (_lazyExecutingTaskFiles == null)
					{
						List<ExecutingTaskFile> list = new List<ExecutingTaskFile>();
						foreach (TaskFile taskFile in AutomaticTask.TaskFileList)
						{
							list.Add(new ExecutingTaskFile(base.Project, this, taskFile));
						}
						_lazyExecutingTaskFiles = list.ToArray();
					}
				}
				return (IExecutingTaskFile[])(object)_lazyExecutingTaskFiles;
			}
		}

		public string WorkingFolder
		{
			get
			{
				if (string.IsNullOrEmpty(workingFolder))
				{
					workingFolder = Util.GetTaskExecutionPath(AutomaticTask);
				}
				return workingFolder;
			}
		}

		public bool TransferInputFilesToFollowUpTask { get; set; } = true;


		public string Name => AutomaticTask.Name;

		public string Description => AutomaticTask.Description;

		public DateTime CreatedAt => AutomaticTask.CreatedAt.ToLocalTime();

		public IUser CreatedBy => base.ProjectImpl.GetUserById(AutomaticTask.CreatedBy.UserId);

		public TaskId Id => new TaskId(AutomaticTask.XmlTask.Guid);

		public ITaskTemplate[] TaskTemplates => AutomaticTask.TaskTemplates;

		public IReport[] Reports => AutomaticTask.Reports;

		public IExecutionResult Result => AutomaticTask.Result;

		public bool ForceProcessing
		{
			get
			{
				return AutomaticTask.ForceProcessing;
			}
			set
			{
				AutomaticTask.ForceProcessing = value;
			}
		}

		internal AutomaticTask AutomaticTask { get; }

		internal SortedDictionary<IProjectFile, FilePurpose> NewTaskFiles { get; } = new SortedDictionary<IProjectFile, FilePurpose>(new NewTaskFilesComparer());


		internal ExecutingAutomaticTask(IProject project, AutomaticTask automaticTask, ILanguageObjectsCache objectsCache)
			: base(project)
		{
			AutomaticTask = automaticTask;
			ObjectsCache = objectsCache;
		}

		public IReport CreateReport(ITaskTemplate taskTemplate, string name, string description, string data)
		{
			return AutomaticTask.AddReport(taskTemplate, name, description, data, null);
		}

		public IReport CreateReport(ITaskTemplate taskTemplate, string name, string description, string data, ILanguageDirection languageDirection)
		{
			return AutomaticTask.AddReport(taskTemplate, name, description, data, languageDirection);
		}

		public void AddFollowUpTaskFile(IProjectFile projectFile, FilePurpose filePurpose)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (!NewTaskFiles.ContainsKey(projectFile))
			{
				NewTaskFiles[projectFile] = filePurpose;
			}
		}

		public void ReportMessage(string source, string message, MessageLevel level, Exception exception)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			AutomaticTask.ResultImpl.ReportMessage(source, message, level, exception);
		}

		public void ReportMessage(string source, string message, MessageLevel level)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			AutomaticTask.ResultImpl.ReportMessage(source, message, level);
		}

		internal void TransferTaskFiles(ExecutingAutomaticTask previousTask)
		{
			AutomaticTask.TransferTaskFiles(previousTask);
			_lazyExecutingTaskFiles = null;
		}

		internal void PopulateOutputFiles()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			AutomaticTask.ClearOutputFiles();
			if (TransferInputFilesToFollowUpTask)
			{
				IExecutingTaskFile[] files = Files;
				for (int i = 0; i < files.Length; i++)
				{
					ExecutingTaskFile executingTaskFile = (ExecutingTaskFile)(object)files[i];
					LanguageFile languageFile = (LanguageFile)(object)executingTaskFile.ProjectFile;
					FilePurpose filePurpose = executingTaskFile.Purpose;
					if (languageFile.XmlProjectFile.Role == FileRole.Reference)
					{
						filePurpose = (FilePurpose)2;
					}
					AutomaticTask.AddOutputFile((IProjectFile)(object)languageFile, filePurpose);
				}
			}
			if (NewTaskFiles.Count <= 0)
			{
				return;
			}
			foreach (KeyValuePair<IProjectFile, FilePurpose> newTaskFile in NewTaskFiles)
			{
				AutomaticTask.AddOutputFile(newTaskFile.Key, newTaskFile.Value);
			}
		}
	}
}
