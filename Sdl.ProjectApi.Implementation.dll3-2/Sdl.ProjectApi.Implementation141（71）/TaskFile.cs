using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class TaskFile : ITaskFile, ITaskEntity
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.TaskFile _xmlTaskFile;

		private ExecutionResult _lazyResult;

		private ITaskFile _lazyParentTaskFile;

		private readonly ScheduledTask _task;

		private readonly IProject _project;

		public Guid Id => _xmlTaskFile.Guid;

		public Guid ProjectFileId => _xmlTaskFile.LanguageFileGuid;

		public IProjectFile ProjectFile => _project.GetProjectFile(_xmlTaskFile.LanguageFileGuid);

		public FilePurpose Purpose => (FilePurpose)Enum.Parse(typeof(FilePurpose), _xmlTaskFile.Purpose.ToString());

		public ITaskFile ParentTaskFile
		{
			get
			{
				if (_xmlTaskFile.ParentTaskFileGuid == Guid.Empty)
				{
					return null;
				}
				if (_lazyParentTaskFile == null)
				{
					_lazyParentTaskFile = _project.GetTaskFile(_xmlTaskFile.ParentTaskFileGuid);
				}
				return _lazyParentTaskFile;
			}
		}

		ITaskBase ITaskFile.Task => (ITaskBase)(object)_task;

		public bool IsComplete => _xmlTaskFile.Completed;

		public IExecutionResult Result => (IExecutionResult)(object)ResultImpl;

		public bool IsFailed => Result.HasErrors;

		internal ExecutionResult ResultImpl
		{
			get
			{
				if (_lazyResult == null)
				{
					_lazyResult = new ExecutionResult(_xmlTaskFile.Result, _task);
				}
				return _lazyResult;
			}
		}

		internal TaskFile(ScheduledTask task, Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile)
		{
			_task = task;
			_project = task.Project;
			_xmlTaskFile = xmlTaskFile ?? throw new ArgumentNullException("xmlTaskFile");
			if (_xmlTaskFile.Result == null)
			{
				_xmlTaskFile.Result = new Sdl.ProjectApi.Implementation.Xml.ExecutionResult();
			}
		}

		public void SetCompleted()
		{
			_xmlTaskFile.Completed = true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TaskFile taskFile))
			{
				return false;
			}
			return taskFile.Id == Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
