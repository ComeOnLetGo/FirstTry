using System;
using System.Collections.Generic;
using System.IO;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Reporting;
using Sdl.ProjectApi.Reporting.XmlReporting;

namespace Sdl.ProjectApi.Implementation
{
	public class ScheduledTask : AbstractProjectItem, IScheduledTask, ITaskBase, IObjectWithExecutionResult
	{
		private readonly TaskFileFactory _taskFileFactory;

		private readonly IProjectPathUtil _projectPathUtil;

		private List<Report> _lazyReports;

		private ITaskTemplate[] _lazyTaskTemplates;

		private ExecutionResult _lazyResult;

		private Dictionary<IProjectFile, FilePurpose> _outputFiles;

		private ITaskReportRenderingEngine _lazyTaskReportRenderingEngine;

		public virtual ITaskTemplate[] TaskTemplates
		{
			get
			{
				if (_lazyTaskTemplates == null)
				{
					ITaskTemplate[] array = (ITaskTemplate[])(object)new ITaskTemplate[XmlTask.TaskTemplateIds.Count];
					for (int i = 0; i < array.Length; i++)
					{
						ITaskTemplate taskTemplateById = ((IProjectConfiguration)base.Project).ProjectsProvider.Workflow.GetTaskTemplateById(XmlTask.TaskTemplateIds[i]);
						array[i] = (ITaskTemplate)(((object)taskTemplateById) ?? ((object)new ShadowAutomaticTaskTemplate(new Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate
						{
							Id = XmlTask.TaskTemplateIds[i],
							Name = XmlTask.TaskTemplateIds[i]
						})));
					}
					_lazyTaskTemplates = array;
				}
				return _lazyTaskTemplates;
			}
		}

		public TaskStatus Status
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return EnumConvert.ConvertTaskStatus(XmlTask.Status);
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				XmlTask.Status = EnumConvert.ConvertTaskStatus(value);
			}
		}

		public int PercentComplete => XmlTask.PercentComplete;

		public string Comment
		{
			get
			{
				return XmlTask.Comment;
			}
			set
			{
				XmlTask.Comment = value;
			}
		}

		public ITaskFile[] Files => TaskFileList.ToArray();

		public IEnumerable<KeyValuePair<IProjectFile, FilePurpose>> OutputFiles => _outputFiles;

		private ITaskReportRenderingEngine TaskReportRenderingEngine
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Expected O, but got Unknown
				//IL_0022: Expected O, but got Unknown
				ITaskReportRenderingEngine obj = _lazyTaskReportRenderingEngine;
				if (obj == null)
				{
					XmlTaskReportRenderingEngine val = new XmlTaskReportRenderingEngine
					{
						TypeManager = (ITaskReportTypeManager)(object)new TaskReportTypeManager()
					};
					ITaskReportRenderingEngine val2 = (ITaskReportRenderingEngine)val;
					_lazyTaskReportRenderingEngine = (ITaskReportRenderingEngine)val;
					obj = val2;
				}
				return obj;
			}
		}

		internal List<ITaskFile> TaskFileList { get; private set; }

		public ITaskFile[] TranslatableFiles
		{
			get
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Invalid comparison between Unknown and I4
				List<ITaskFile> list = new List<ITaskFile>();
				foreach (ITaskFile taskFile in TaskFileList)
				{
					if ((int)taskFile.ProjectFile.FileRole == 1)
					{
						list.Add(taskFile);
					}
				}
				return list.ToArray();
			}
		}

		public ITaskLanguageDirection[] LanguageDirections
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string Name
		{
			get
			{
				if (string.IsNullOrWhiteSpace(XmlTask.Name))
				{
					return TaskDetailMapper.GetTaskTemplateNames(TaskTemplates, XmlTask.TaskTemplateIds);
				}
				return XmlTask.Name;
			}
		}

		public string Description
		{
			get
			{
				if (string.IsNullOrWhiteSpace(XmlTask.Description))
				{
					return TaskDetailMapper.GetTaskTemplateDescriptions(TaskTemplates);
				}
				return XmlTask.Description;
			}
		}

		public DateTime CreatedAt => XmlTask.CreatedAt.ToLocalTime();

		public IUser CreatedBy
		{
			get
			{
				return base.ProjectImpl.GetUserById(XmlTask.CreatedBy);
			}
			set
			{
				XmlTask.CreatedBy = value.UserId;
			}
		}

		public DateTime CompletedAt
		{
			get
			{
				if (!XmlTask.CompletedAtSpecified)
				{
					return DateTime.MinValue;
				}
				return XmlTask.CompletedAt.ToLocalTime();
			}
		}

		public TaskId Id => new TaskId(XmlTask.Guid);

		public IExecutionResult Result => (IExecutionResult)(object)ResultImpl;

		public IReport[] Reports => (IReport[])(object)ReportsList.ToArray();

		public bool ForceProcessing { get; set; }

		internal Task XmlTask { get; }

		internal List<Report> ReportsList
		{
			get
			{
				if (_lazyReports == null)
				{
					_lazyReports = new List<Report>();
					for (int i = 0; i < XmlTask.Reports.Count; i++)
					{
						_lazyReports.Add(new Report(this, TaskReportRenderingEngine, XmlTask.Reports[i], (IReportRepository)(object)new ReportRepository(Path.Combine(base.Project.LocalDataFolder, XmlTask.Reports[i].PhysicalPath))));
					}
				}
				return _lazyReports;
			}
		}

		internal ExecutionResult ResultImpl
		{
			get
			{
				if (_lazyResult == null)
				{
					_lazyResult = new ExecutionResult(XmlTask.Result, this);
				}
				return _lazyResult;
			}
		}

		public string ExternalId => XmlTask.ExternalId;

		public DateTime StartedAt
		{
			get
			{
				if (!XmlTask.StartedAtSpecified)
				{
					return DateTime.MinValue;
				}
				return XmlTask.StartedAt.ToLocalTime();
			}
		}

		public event EventHandler StatusChanged;

		public event EventHandler<ExecutionMessageEventArgs> MessageReported;

		internal ScheduledTask(IProject project, Task xmlTask, TaskFileFactory taskFileBuilder, IProjectPathUtil projectPathUtil)
			: base(project)
		{
			XmlTask = xmlTask ?? throw new ArgumentNullException("xmlTask");
			_taskFileFactory = taskFileBuilder;
			_projectPathUtil = projectPathUtil;
			if (XmlTask.Result == null)
			{
				XmlTask.Result = new Sdl.ProjectApi.Implementation.Xml.ExecutionResult();
			}
			LoadTaskFiles();
		}

		public void AddOutputFile(IProjectFile projectFile, FilePurpose filePurpose)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			_outputFiles.Add(projectFile, filePurpose);
			OutputFile outputFile = new OutputFile();
			outputFile.LanguageFileGuid = projectFile.Guid;
			outputFile.Purpose = EnumConvert.ConvertFilePurpose(filePurpose);
			XmlTask.OutputFiles.Add(outputFile);
		}

		public void ClearOutputFiles()
		{
			_outputFiles.Clear();
			XmlTask.OutputFiles.Clear();
		}

		private void LoadTaskFiles()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			TaskFileList = new List<ITaskFile>();
			foreach (Sdl.ProjectApi.Implementation.Xml.TaskFile file in XmlTask.Files)
			{
				IProjectFile projectFile = base.Project.GetProjectFile(file.LanguageFileGuid);
				if (projectFile != null && (int)projectFile.FileRole != 4)
				{
					ITaskFile val = _taskFileFactory.CreateTaskFile(this, file);
					TaskFileList.Add(base.Project.ProjectTaskFileManager.Add(val));
				}
			}
			LoadOutputFiles();
		}

		private void LoadOutputFiles()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			_outputFiles = new Dictionary<IProjectFile, FilePurpose>();
			foreach (OutputFile outputFile in XmlTask.OutputFiles)
			{
				IProjectFile projectFile = base.Project.GetProjectFile(outputFile.LanguageFileGuid);
				if (projectFile != null && (int)projectFile.FileRole != 4)
				{
					FilePurpose value = EnumConvert.ConvertFilePurpose(outputFile.Purpose);
					_outputFiles.Add(projectFile, value);
				}
			}
		}

		public virtual void Cancel()
		{
			XmlTask.Status = TaskStatus.Cancelled;
		}

		public ITaskFile AddWorkFile(ILocalizableFile localizableFile)
		{
			return AddFile((IProjectFile)(object)localizableFile, (FilePurpose)1);
		}

		public ITaskFile AddWorkFile(ILocalizableFile localizableFile, ITaskFile parentTaskFile)
		{
			return AddFile((IProjectFile)(object)localizableFile, (FilePurpose)1, parentTaskFile);
		}

		public ITaskFile AddReferenceFile(IProjectFile projectFile)
		{
			return AddFile(projectFile, (FilePurpose)2);
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return AddFile(file, purpose, null);
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return AddFile(file, purpose, parentTaskFile, Guid.NewGuid());
		}

		public ITaskFile AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile, Guid taskFileId)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			if (!(file is LanguageFile languageFile))
			{
				throw new ProjectApiException("A task can only contain project files implementing ILanguageFile.");
			}
			if (!(file is ILocalizableFile))
			{
				if ((int)purpose != 2)
				{
					throw new ArgumentException("Reference files can only added to a task as reference files.", "purpose");
				}
				if (parentTaskFile != null)
				{
					throw new ArgumentException("Reference files can not have a parent task file.", "parentTaskFile");
				}
			}
			if (parentTaskFile != null && parentTaskFile.ProjectFile.Project.Guid != file.Project.Guid)
			{
				throw new ArgumentException("The parent task file should belong to the same project as the added task file.", "parentTaskFile");
			}
			Sdl.ProjectApi.Implementation.Xml.TaskFile taskFile = new Sdl.ProjectApi.Implementation.Xml.TaskFile();
			taskFile.Guid = taskFileId;
			taskFile.Completed = false;
			taskFile.Purpose = (TaskFilePurpose)Enum.Parse(typeof(TaskFilePurpose), ((object)(FilePurpose)(ref purpose)).ToString());
			taskFile.LanguageFileGuid = languageFile.XmlLanguageFile.Guid;
			if (parentTaskFile != null)
			{
				taskFile.ParentTaskFileGuid = parentTaskFile.Id;
			}
			XmlTask.Files.Add(taskFile);
			ITaskFile val = _taskFileFactory.CreateTaskFile(this, taskFile);
			ITaskFile val2 = base.Project.ProjectTaskFileManager.Add(val);
			TaskFileList.Add(val2);
			return val2;
		}

		public void RemoveFile(ITaskFile taskFile)
		{
			int num = XmlTask.Files.FindIndex((Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile) => taskFile.Id == xmlTaskFile.Guid);
			if (num != -1)
			{
				XmlTask.Files.RemoveAt(num);
				TaskFileList.Remove(taskFile);
				base.Project.ProjectTaskFileManager.Remove((ITaskFile)(object)(TaskFile)(object)taskFile);
			}
		}

		public void RemoveFiles(IEnumerable<ITaskFile> taskFiles)
		{
			foreach (ITaskFile taskFile in taskFiles)
			{
				RemoveFile(taskFile);
			}
		}

		protected virtual void OnStatusChanged()
		{
			if (this.StatusChanged != null)
			{
				this.StatusChanged(this, EventArgs.Empty);
			}
		}

		public IReport AddReport(ITaskTemplate taskTemplate, string name, string description, string data, ILanguageDirection languageDirection, bool isCustomReport = false)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			if (!Directory.Exists(base.ProjectImpl.GetProjectReportsDirectory()))
			{
				try
				{
					Directory.CreateDirectory(base.ProjectImpl.GetProjectReportsDirectory());
				}
				catch (Exception ex)
				{
					throw new ProjectApiException(ErrorMessages.Report_ErrorCreatingReportFolder, ex);
				}
			}
			TaskReport taskReport = new TaskReport();
			taskReport.AssignNewGuid();
			taskReport.Name = name;
			taskReport.Description = description;
			taskReport.TaskTemplateId = taskTemplate.Id;
			taskReport.IsCustomReport = isCustomReport;
			if (languageDirection != null)
			{
				taskReport.LanguageDirectionGuid = languageDirection.Guid;
			}
			string nextReportFilePath = base.ProjectImpl.GetNextReportFilePath(taskTemplate.Name, languageDirection);
			taskReport.PhysicalPath = _projectPathUtil.MakeRelativePath(base.Project, nextReportFilePath, false);
			Report report = new Report(this, TaskReportRenderingEngine, taskReport, (IReportRepository)(object)new ReportRepository(Path.Combine(base.Project.LocalDataFolder, taskReport.PhysicalPath)));
			try
			{
				report.SetReportXml(data);
			}
			catch (Exception ex2)
			{
				throw new ProjectApiException(ErrorMessages.Report_ErrorCreatingReport, ex2);
			}
			XmlTask.Reports.Add(taskReport);
			if (_lazyReports != null)
			{
				_lazyReports.Add(report);
			}
			return (IReport)(object)report;
		}

		internal void RemoveReport(Report report)
		{
			ReportsList.Remove(report);
			XmlTask.Reports.RemoveAll((TaskReport xmlTaskReport) => xmlTaskReport.Guid == report.Guid);
		}

		internal void SetStarted()
		{
			XmlTask.Status = TaskStatus.Started;
			XmlTask.StartedAtSpecified = true;
			XmlTask.StartedAt = DateTime.UtcNow;
			OnStatusChanged();
		}

		internal void SetPercentComplete(int percentCompleted)
		{
			XmlTask.PercentComplete = percentCompleted;
			OnStatusChanged();
		}

		internal void SetCompleted()
		{
			XmlTask.Status = TaskStatus.Completed;
			XmlTask.CompletedAtSpecified = true;
			XmlTask.CompletedAt = DateTime.UtcNow;
			XmlTask.PercentComplete = 100;
			OnStatusChanged();
		}

		internal void SetFailed()
		{
			XmlTask.Status = TaskStatus.Failed;
			OnStatusChanged();
		}

		internal void SetCancelling()
		{
			XmlTask.Status = TaskStatus.Cancelling;
			OnStatusChanged();
		}

		internal void SetCancelled()
		{
			XmlTask.Status = TaskStatus.Cancelled;
			OnStatusChanged();
		}

		internal void ResetTaskStatus()
		{
			XmlTask.Status = TaskStatus.Created;
		}

		internal void ResetTaskFilesStatus()
		{
			ResultImpl.Clear();
			ITaskFile[] files = Files;
			for (int i = 0; i < files.Length; i++)
			{
				TaskFile taskFile = (TaskFile)(object)files[i];
				taskFile.ResultImpl.Clear();
			}
		}

		internal void TransferTaskFiles(ExecutingAutomaticTask previousTask)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			TaskFileList.ForEach(delegate(ITaskFile tf)
			{
				base.Project.ProjectTaskFileManager.Remove((ITaskFile)(object)(TaskFile)(object)tf);
			});
			XmlTask.Files.Clear();
			TaskFileList.Clear();
			foreach (KeyValuePair<IProjectFile, FilePurpose> outputFile in previousTask.AutomaticTask.OutputFiles)
			{
				Sdl.ProjectApi.Implementation.Xml.TaskFile taskFile = new Sdl.ProjectApi.Implementation.Xml.TaskFile();
				taskFile.AssignNewGuid();
				taskFile.Completed = false;
				taskFile.Purpose = EnumConvert.ConvertFilePurpose(outputFile.Value);
				taskFile.LanguageFileGuid = outputFile.Key.Guid;
				XmlTask.Files.Add(taskFile);
			}
			LoadTaskFiles();
		}

		public void RaiseMessageReported(IExecutionMessage message)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if (this.MessageReported != null)
			{
				this.MessageReported(this, new ExecutionMessageEventArgs(message));
			}
		}

		public override bool Equals(object obj)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (obj is ScheduledTask scheduledTask)
			{
				TaskId id = Id;
				string text = ((object)(TaskId)(ref id)).ToString();
				id = scheduledTask.Id;
				return text.Equals(((object)(TaskId)(ref id)).ToString());
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			TaskId id = Id;
			return ((object)(TaskId)(ref id)).GetHashCode();
		}

		public ITaskFile GetTaskFile(Guid projectFileId)
		{
			return TaskFileList.Find((ITaskFile tf) => tf.ProjectFile.Guid == projectFileId);
		}
	}
}
