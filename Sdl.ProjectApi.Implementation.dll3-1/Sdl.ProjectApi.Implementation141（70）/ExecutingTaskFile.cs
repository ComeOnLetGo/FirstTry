using System;
using Sdl.FileTypeSupport.Framework.IntegrationApi;

namespace Sdl.ProjectApi.Implementation
{
	public class ExecutingTaskFile : AbstractProjectItem, IExecutingTaskFile, ITaskFile, ITaskEntity, IExecutingTaskEntity, IExecutionMessageReporter, IFileExecutionMessageReporter
	{
		private readonly ExecutingAutomaticTask _executingAutomaticTask;

		private readonly TaskFile _taskFile;

		public ILanguageFile LanguageFile
		{
			get
			{
				IProjectFile projectFile = _taskFile.ProjectFile;
				return (ILanguageFile)(object)((projectFile is ILanguageFile) ? projectFile : null);
			}
		}

		public Guid ProjectFileId => _taskFile.ProjectFileId;

		public ILocalizableFile LocalizableFile
		{
			get
			{
				IProjectFile projectFile = _taskFile.ProjectFile;
				return (ILocalizableFile)(object)((projectFile is ILocalizableFile) ? projectFile : null);
			}
		}

		public ITranslatableFile TranslatableFile
		{
			get
			{
				IProjectFile projectFile = _taskFile.ProjectFile;
				return (ITranslatableFile)(object)((projectFile is ITranslatableFile) ? projectFile : null);
			}
		}

		public string LocalFilePath { get; set; }

		public Guid Id => _taskFile.Id;

		public ITaskFile ParentTaskFile => _taskFile.ParentTaskFile;

		public IProjectFile ProjectFile => _taskFile.ProjectFile;

		public FilePurpose Purpose => _taskFile.Purpose;

		ITaskBase ITaskFile.Task => (ITaskBase)(object)_executingAutomaticTask;

		public IExecutingAutomaticTask Task => (IExecutingAutomaticTask)(object)_executingAutomaticTask;

		public bool IsComplete => _taskFile.IsComplete;

		public IExecutionResult Result => _taskFile.Result;

		public bool IsFailed => _taskFile.IsFailed;

		internal ExecutingTaskFile(IProject project, ExecutingAutomaticTask executingAutomaticTask, TaskFile taskFile)
			: base(project)
		{
			_executingAutomaticTask = executingAutomaticTask;
			_taskFile = taskFile;
		}

		public void Complete()
		{
			_taskFile.SetCompleted();
		}

		public void ReportMessage(string source, string message, MessageLevel level, IMessageLocation fromLocation, IMessageLocation uptoLocation)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			_taskFile.ResultImpl.ReportMessage(source, message, level, fromLocation, uptoLocation);
		}

		public void ReportMessage(string source, string message, MessageLevel level, Exception exception)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			_taskFile.ResultImpl.ReportMessage(source, message, level, exception);
		}

		public void ReportMessage(string source, string message, MessageLevel level)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			_taskFile.ResultImpl.ReportMessage(source, message, level);
		}

		public override string ToString()
		{
			return ProjectFile.LocalFilePath;
		}
	}
}
