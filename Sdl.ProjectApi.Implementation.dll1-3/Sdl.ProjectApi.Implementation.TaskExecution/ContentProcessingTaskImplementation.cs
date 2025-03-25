using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Amib.Threading;
using Amib.Threading.Internal;
using Microsoft.Extensions.Logging;
using Sdl.BestMatchService.Common.Events;
using Sdl.BestMatchService.Common.Notifications;
using Sdl.Core.Globalization;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.Desktop.Platform.Services;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.Core.Utilities.BilingualApi;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.FileTypeSupport.Framework.NativeApi;
using Sdl.ProjectApi.TaskImplementation;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal class ContentProcessingTaskImplementation : ISimpleTaskImplementation, IAbstractTaskImplementation, IDisposable
	{
		private enum TaskFileExecuterStatus
		{
			NotStarted,
			InProgress,
			Completed,
			Canceled,
			ErrorOccured,
			Ignored
		}

		private class TaskFileExecuter
		{
			private const int DownloadFilesPercentage = 5;

			private const int UploadFilesPercentage = 5;

			private const string FileIndexId = "FileIndexId";

			private readonly IExecutingAutomaticTask executingTask;

			private readonly IExecutingTaskFile executingTaskFile;

			private readonly IContentProcessingTaskImplementation[] tasks;

			private readonly int fileIndex;

			private volatile bool _cancelRequested;

			private readonly object _lockObject = new object();

			public TaskFileExecuterStatus Status { get; private set; }

			public IExecutingTaskFile ExecutingTaskFile => executingTaskFile;

			public event EventHandler<TaskProgressEventArgs> Progress;

			public TaskFileExecuter(IExecutingAutomaticTask executingTask, IExecutingTaskFile executingTaskFile, int fileIndex, IContentProcessingTaskImplementation[] tasks)
			{
				this.executingTask = executingTask;
				this.executingTaskFile = executingTaskFile;
				this.tasks = GetTasksToExecute(tasks, executingTaskFile);
				this.fileIndex = fileIndex;
				Status = TaskFileExecuterStatus.NotStarted;
			}

			public void Execute()
			{
				string targetFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				if (Initialize(targetFilePath))
				{
					bool shouldUpload = Parse(targetFilePath);
					Complete(targetFilePath, shouldUpload);
				}
			}

			private bool Initialize(string targetFilePath)
			{
				try
				{
					if (executingTaskFile.TranslatableFile == null)
					{
						((IExecutingTaskEntity)executingTaskFile).Complete();
						Status = TaskFileExecuterStatus.Ignored;
						return false;
					}
					IFileRevision latestBilingualVersion = GetLatestBilingualVersion();
					if (latestBilingualVersion == null || !latestBilingualVersion.IsBilingual)
					{
						Status = TaskFileExecuterStatus.Ignored;
						return false;
					}
					if (tasks.Length == 0)
					{
						Status = TaskFileExecuterStatus.Ignored;
						return false;
					}
					ITranslatableFile translatableFile = executingTaskFile.TranslatableFile;
					OnProgress(0, $"Downloading '{((ILanguageFile)translatableFile).CurrentFilename}'");
					executingTaskFile.LocalFilePath = latestBilingualVersion.Download(Path.Combine(executingTask.WorkingFolder, ((LanguageBase)((IProjectFile)executingTaskFile.LanguageFile).Language).IsoAbbreviation));
					CheckCancel();
				}
				finally
				{
					if (_cancelRequested)
					{
						Cleanup(targetFilePath);
					}
				}
				return true;
			}

			private static IContentProcessingTaskImplementation[] GetTasksToExecute(IContentProcessingTaskImplementation[] allTasks, IExecutingTaskFile executingTaskFile)
			{
				return allTasks.Where((IContentProcessingTaskImplementation task) => task.ShouldProcessFile(executingTaskFile)).ToArray();
			}

			private bool Parse(string targetFilePath)
			{
				bool result = false;
				try
				{
					Status = TaskFileExecuterStatus.InProgress;
					OnProgress(5, $"Processing '{((ILanguageFile)executingTaskFile.TranslatableFile).CurrentFilename}'");
					IMultiFileConverter val = CreateMultiFileConverter((IProjectFile)(object)executingTaskFile.TranslatableFile, targetFilePath);
					try
					{
						val.Parse();
						result = FileComplete(val);
					}
					finally
					{
						DisposeConverterParsers(val);
					}
				}
				catch (CancelException)
				{
					Status = TaskFileExecuterStatus.Canceled;
				}
				catch (Exception ex)
				{
					((IExecutionMessageReporter)executingTaskFile).ReportMessage(((ITaskBase)executingTask).Name, ex.Message, (MessageLevel)2, ex);
					Status = TaskFileExecuterStatus.ErrorOccured;
				}
				finally
				{
					if (_cancelRequested)
					{
						Cleanup(targetFilePath);
					}
				}
				return result;
			}

			private void Complete(string targetFilePath, bool shouldUpload)
			{
				try
				{
					if (!((ITaskEntity)executingTaskFile).Result.HasErrors && shouldUpload)
					{
						File.Delete(executingTaskFile.LocalFilePath);
						File.Move(targetFilePath, executingTaskFile.LocalFilePath);
						((ILanguageFile)executingTaskFile.TranslatableFile).UploadNewVersion(executingTaskFile.LocalFilePath, $"Created by '{((ITaskBase)executingTask).Name}'");
					}
					OnProgress(100, $"Completed '{((ILanguageFile)executingTaskFile.TranslatableFile).CurrentFilename}'");
					Status = TaskFileExecuterStatus.Completed;
				}
				catch (CancelException)
				{
					Status = TaskFileExecuterStatus.Canceled;
				}
				catch (Exception ex)
				{
					((IExecutionMessageReporter)executingTaskFile).ReportMessage(((ITaskBase)executingTask).Name, ex.Message, (MessageLevel)2, ex);
					Status = TaskFileExecuterStatus.ErrorOccured;
				}
				finally
				{
					Cleanup(targetFilePath);
					((IExecutingTaskEntity)executingTaskFile).Complete();
				}
			}

			private static void DisposeConverterParsers(IMultiFileConverter converter)
			{
				IFileExtractor[] array = converter.Extractors.ToArray();
				IFileExtractor[] array2 = array;
				foreach (IFileExtractor val in array2)
				{
					if (val.NativeExtractor != null && val.NativeExtractor.Parser != null)
					{
						((IDisposable)val.NativeExtractor.Parser).Dispose();
					}
					if (val.BilingualParser != null)
					{
						((IDisposable)val.BilingualParser).Dispose();
					}
					converter.RemoveExtractor(val);
				}
			}

			private static void Cleanup(string targetFilePath)
			{
				if (!string.IsNullOrEmpty(targetFilePath) && File.Exists(targetFilePath))
				{
					File.Delete(targetFilePath);
				}
			}

			private void CheckCancel()
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				if (_cancelRequested)
				{
					throw new CancelException();
				}
			}

			private IMultiFileConverter CreateMultiFileConverter(IProjectFile tf, string filePath)
			{
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Expected O, but got Unknown
				IFileTypeManager filterManager = ((IProjectConfiguration)tf.Project).FileTypeConfiguration.FilterManager;
				filterManager.SettingsBundle = ((IObjectWithSettings)((IProjectFile)executingTaskFile.LanguageFile).Project).Settings;
				IMultiFileConverter converterToDefaultBilingual = filterManager.GetConverterToDefaultBilingual(executingTaskFile.LocalFilePath, filePath, (EventHandler<MessageEventArgs>)ConverterMessage);
				converterToDefaultBilingual.DependencyFileLocator = new DependencyFileLocator(ConverterFindDependencyFile);
				converterToDefaultBilingual.Progress += ConverterProgress;
				converterToDefaultBilingual.SharedObjects.PublishSharedObject("FileIndexId", (object)fileIndex, (IdConflictResolution)0);
				ProjectExtensions.AddEncryptionKeyToConverter(tf.Project, converterToDefaultBilingual);
				CheckCancel();
				ConfigureConverter(converterToDefaultBilingual, ((IProjectFile)executingTaskFile.LanguageFile).Language);
				return converterToDefaultBilingual;
			}

			private IFileRevision GetLatestBilingualVersion()
			{
				return executingTaskFile.TranslatableFile.GetMostRecentBilingualRevision() ?? ((ILanguageFile)executingTaskFile.TranslatableFile).LatestRevision;
			}

			private void ConfigureConverter(IMultiFileConverter multiFileConverter, Language targetLanguage)
			{
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				if (targetLanguage != null)
				{
					TargetLanguageSetterProcessor targetLanguageSetterProcessor = new TargetLanguageSetterProcessor(targetLanguage);
					((IBilingualProcessorContainer)multiFileConverter).AddBilingualProcessor((IBilingualContentProcessor)new BilingualContentHandlerAdapter((IBilingualContentHandler)(object)targetLanguageSetterProcessor));
				}
				for (int i = 0; i < tasks.Length; i++)
				{
					try
					{
						tasks[i].ConfigureConverter(executingTaskFile, multiFileConverter);
					}
					catch (CancelException)
					{
						throw;
					}
					catch (Exception ex)
					{
						throw new ProjectApiException($"Unexpected exception when configuring file multiFileConverter for task '{((ITaskBase)executingTask).TaskTemplates[i].Name}': {ex.Message}.", ex);
					}
				}
			}

			private bool FileComplete(IMultiFileConverter converter)
			{
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				bool result = false;
				for (int i = 0; i < tasks.Length; i++)
				{
					try
					{
						FileContentProcessingResult val = tasks[i].FileComplete(executingTaskFile, converter);
						if (val.UploadFile)
						{
							result = true;
						}
					}
					catch (CancelException)
					{
						throw;
					}
					catch (Exception ex)
					{
						throw new ProjectApiException($"Unexpected exception when completing file processing for task '{((ITaskBase)executingTask).TaskTemplates[i].Name}': {ex.Message}.", ex);
					}
				}
				return result;
			}

			private void ConverterProgress(object source, BatchProgressEventArgs e)
			{
				CheckCancel();
				int percentComplete = 5 + (int)((double)(int)e.FilePercentComplete * 90.0 / 100.0);
				OnProgress(percentComplete, null);
			}

			private void ConverterMessage(object source, MessageEventArgs args)
			{
				FileTypeSupportUtil.ReportMessage(executingTaskFile, args);
			}

			private void OnProgress(int percentComplete, string currentOperationDescription)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				if (this.Progress != null)
				{
					this.Progress(this, new TaskProgressEventArgs(percentComplete, currentOperationDescription));
				}
				CheckCancel();
			}

			private string ConverterFindDependencyFile(IDependencyFileProperties fileInfo)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				return FileTypeSupportUtil.FindDependencyFile(executingTaskFile, fileInfo, (IFile)new FileWrapper());
			}

			public void Cancel(bool doRollback)
			{
				_cancelRequested = true;
				lock (_lockObject)
				{
					IContentProcessingTaskImplementation[] array = tasks;
					foreach (IContentProcessingTaskImplementation val in array)
					{
						((IAbstractTaskImplementation)val).Cancel(doRollback);
					}
				}
			}
		}

		private static readonly ILogger _log = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ContentProcessingTaskImplementation>(LogProvider.GetLoggerFactory());

		private IExecutingAutomaticTask _task;

		private IContentProcessingTaskImplementation[] _taskImplementations;

		private bool _taskAllowsMultiThreading;

		private int _processedFilesCount;

		private int _filesCount;

		private bool _cancelRequested;

		private readonly ManualResetEvent _doneEvent = new ManualResetEvent(initialState: false);

		private IList<TaskFileExecuter> _taskFileExecutors;

		private SmartThreadPool _threadPool;

		private readonly object _lockObject = new object();

		private readonly IDisposable quotaReachedEventDisposable;

		private readonly NotificationManager _notificationManager;

		private readonly IEventAggregator _eventAggregator;

		private const string BatchTaskthreadCountKey = "BatchTaskthreadCount";

		private int _percentage = -1;

		public bool DidQuotaReachEventHappened { get; set; }

		private SmartThreadPool ThreadPool
		{
			get
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				if (_threadPool == null)
				{
					_threadPool = new SmartThreadPool
					{
						MaxThreads = GetMaxBatchTaskThreadCount()
					};
				}
				return _threadPool;
			}
		}

		public event EventHandler<TaskProgressEventArgs> Progress;

		public ContentProcessingTaskImplementation()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			_eventAggregator = GlobalServices.Context.GetService<IEventAggregator>();
			_notificationManager = new NotificationManager(_eventAggregator);
			quotaReachedEventDisposable = ObservableExtensions.Subscribe<MTQuotaReachedEvent>(_eventAggregator.GetEvent<MTQuotaReachedEvent>(), (Action<MTQuotaReachedEvent>)OnQuotaReachedEvent);
		}

		public void Execute()
		{
			if (_cancelRequested)
			{
				return;
			}
			CreateTaskImplementations();
			InitializeTaskImplementations();
			if (!_cancelRequested)
			{
				_doneEvent.Reset();
				CreateTaskFileExecutors();
				if (!_cancelRequested)
				{
					RunTaskFileExecutors();
					_doneEvent.WaitOne();
				}
			}
		}

		public void InitializeTask(IExecutingAutomaticTask task)
		{
			if (task == null)
			{
				throw new ArgumentNullException("task");
			}
			_task = task;
			_filesCount = _task.Files.Length;
			_processedFilesCount = 0;
		}

		public void Cancel(bool doRollback)
		{
			_cancelRequested = true;
			lock (_lockObject)
			{
				if (_taskFileExecutors == null)
				{
					return;
				}
				foreach (TaskFileExecuter taskFileExecutor in _taskFileExecutors)
				{
					taskFileExecutor.Cancel(doRollback);
				}
			}
		}

		public void TaskComplete()
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			IContentProcessingTaskImplementation[] taskImplementations = _taskImplementations;
			foreach (IContentProcessingTaskImplementation val in taskImplementations)
			{
				try
				{
					((IAbstractTaskImplementation)val).TaskComplete();
					num++;
				}
				catch (Exception ex)
				{
					throw new ProjectApiException($"Unexpected exception when completing task '{((ITaskBase)_task).TaskTemplates[num].Name}': {ex.Message}.", ex);
				}
			}
		}

		private static int GetMaxBatchTaskThreadCount()
		{
			string text = ConfigurationManager.AppSettings["BatchTaskthreadCount"];
			if (string.IsNullOrEmpty(text) || !int.TryParse(text, out var result))
			{
				result = 3;
			}
			LoggerExtensions.LogInformation(_log, $"Batch task max thread count = {result}", Array.Empty<object>());
			return result;
		}

		private void RunTaskFileExecutors()
		{
			if (_taskFileExecutors == null || !_taskFileExecutors.Any())
			{
				return;
			}
			int num = 0;
			try
			{
				foreach (TaskFileExecuter item in _taskFileExecutors.Where((TaskFileExecuter currentFileExecuter) => currentFileExecuter.Status == TaskFileExecuterStatus.NotStarted))
				{
					RunFileExecuter(item, _taskAllowsMultiThreading);
					num++;
				}
			}
			catch
			{
				_filesCount = num;
				throw;
			}
		}

		private void RunFileExecuter(TaskFileExecuter currentFileExecuter, bool allowMultiThreading)
		{
			if (allowMultiThreading)
			{
				((WorkItemsGroupBase)ThreadPool).QueueWorkItem<TaskFileExecuter>((Action<TaskFileExecuter>)RunFileExecuter, currentFileExecuter, (WorkItemPriority)2);
			}
			else
			{
				RunFileExecuter(currentFileExecuter);
			}
		}

		private void CreateTaskImplementations()
		{
			bool taskAllowsMultiThreading = ((ITaskBase)_task).TaskTemplates.Length != 0;
			IContentProcessingTaskImplementation[] array = (IContentProcessingTaskImplementation[])(object)new IContentProcessingTaskImplementation[((ITaskBase)_task).TaskTemplates.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = CreateTaskImplementation(((ITaskBase)_task).TaskTemplates[i], ref taskAllowsMultiThreading);
			}
			_taskImplementations = array;
			_taskAllowsMultiThreading = taskAllowsMultiThreading;
		}

		private void OnQuotaReachedEvent(MTQuotaReachedEvent mTQuotaReachedEvent)
		{
			if (!DidQuotaReachEventHappened)
			{
				DidQuotaReachEventHappened = true;
				((IExecutionMessageReporter)_task).ReportMessage(string.Empty, StringResources.MT_LimitReachedWarning, (MessageLevel)1);
				_notificationManager.ShowQuotaReachedNotification();
			}
		}

		private void CreateTaskFileExecutors()
		{
			List<TaskFileExecuter> list = new List<TaskFileExecuter>();
			for (int i = 0; i < _task.Files.Length; i++)
			{
				if (_cancelRequested)
				{
					break;
				}
				lock (_lockObject)
				{
					TaskFileExecuter taskFileExecuter = new TaskFileExecuter(_task, _task.Files[i], i, _taskImplementations);
					taskFileExecuter.Progress += CurrentFileExecuterProgress;
					list.Add(taskFileExecuter);
				}
			}
			_taskFileExecutors = list;
		}

		private void RunFileExecuter(object state)
		{
			try
			{
				TaskFileExecuter taskFileExecuter = (TaskFileExecuter)state;
				if (!_cancelRequested)
				{
					taskFileExecuter.Execute();
				}
			}
			finally
			{
				if (Interlocked.Increment(ref _processedFilesCount) == _filesCount)
				{
					_doneEvent.Set();
				}
			}
		}

		private void CurrentFileExecuterProgress(object sender, TaskProgressEventArgs e)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			CheckCancel();
			lock (_lockObject)
			{
				double num = 100.0 / (double)_filesCount;
				int val = (int)((double)_processedFilesCount * num + (double)e.PercentComplete / 100.0 * num);
				_percentage = Math.Max(val, _percentage);
			}
			OnProgress(new TaskProgressEventArgs(_percentage));
		}

		private void CheckCancel()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (_cancelRequested)
			{
				throw new CancelException();
			}
		}

		private static IContentProcessingTaskImplementation CreateTaskImplementation(ITaskTemplate template, ref bool taskAllowsMultiThreading)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			IAutomaticTaskTemplate val = (IAutomaticTaskTemplate)(object)((template is IAutomaticTaskTemplate) ? template : null);
			if (val == null)
			{
				throw new ProjectApiException($"The task template '{template.Name}' is not an automatic task template.");
			}
			IAbstractTaskImplementation val2 = val.CreateImplementation();
			IContentProcessingTaskImplementation val3 = (IContentProcessingTaskImplementation)(object)((val2 is IContentProcessingTaskImplementation) ? val2 : null);
			if (val3 == null)
			{
				throw new ProjectApiException($"The task implementation of template '{template.Name}' is not a content processing task implementation.");
			}
			taskAllowsMultiThreading &= val.AllowMultiThreading;
			return val3;
		}

		private void InitializeTaskImplementations()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			bool flag = _taskAllowsMultiThreading;
			IContentProcessingTaskImplementation[] taskImplementations = _taskImplementations;
			foreach (IContentProcessingTaskImplementation val in taskImplementations)
			{
				try
				{
					((IAbstractTaskImplementation)val).InitializeTask(_task);
					flag &= val.ShouldRunOnMultipleThreads;
					num++;
				}
				catch (Exception ex)
				{
					throw new ProjectApiException($"Unexpected exception when initializing task '{((ITaskBase)_task).TaskTemplates[num].Name}': {ex.Message}.", ex);
				}
			}
			_taskAllowsMultiThreading = flag;
		}

		private void OnProgress(TaskProgressEventArgs e)
		{
			if (this.Progress != null)
			{
				this.Progress(this, e);
			}
		}

		public void Dispose()
		{
			quotaReachedEventDisposable.Dispose();
		}
	}
}
