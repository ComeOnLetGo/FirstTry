using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.ProjectApi.Implementation.TaskExecution;
using Sdl.ProjectApi.TaskImplementation;

namespace Sdl.ProjectApi.Implementation
{
	internal class AutomaticTaskExecuter
	{
		private static readonly ILogger _log = LogProvider.GetLoggerFactory().CreateLogger("ProjectApi.AutomaticTaskExecuter");

		private readonly ExecutingAutomaticTask[] _executingAutomaticTasks;

		private int _currentExecutingAutomaticTaskIndex = -1;

		private ExecutingAutomaticTask _currentExecutingAutomaticTask;

		private AutomaticTask _currentAutomaticTask;

		private ISimpleTaskImplementation _currentAutomaticTaskImplementation;

		private readonly object _lockObject = new object();

		private bool _isCompleted;

		public AutomaticTaskExecuter(ExecutingAutomaticTask[] executingAutomaticTasks)
		{
			_executingAutomaticTasks = executingAutomaticTasks;
		}

		public void Execute()
		{
			//IL_0235: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Invalid comparison between Unknown and I4
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Invalid comparison between Unknown and I4
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Invalid comparison between Unknown and I4
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Invalid comparison between Unknown and I4
			for (int i = 0; i < _executingAutomaticTasks.Length; i++)
			{
				try
				{
					lock (_lockObject)
					{
						_currentExecutingAutomaticTaskIndex = i;
						_currentExecutingAutomaticTask = _executingAutomaticTasks[i];
						_currentAutomaticTask = _currentExecutingAutomaticTask.AutomaticTask;
						if (_currentAutomaticTask.Files == null || _currentAutomaticTask.Files.Length == 0)
						{
							_currentAutomaticTask.SetStarted();
							_currentAutomaticTask.SetCompleted();
							continue;
						}
						RobustCreateDirectory(_currentExecutingAutomaticTask.WorkingFolder);
						_currentAutomaticTaskImplementation = TaskImplementationFactory.CreateTaskImplementation((IExecutingAutomaticTask)(object)_currentExecutingAutomaticTask);
						_currentAutomaticTaskImplementation.Progress += _currentAutomaticTaskImplementation_Progress;
						_currentAutomaticTask.SetStarted();
					}
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					_currentAutomaticTaskImplementation.Execute();
					if ((int)_currentAutomaticTask.Status != 6)
					{
						((IAbstractTaskImplementation)_currentAutomaticTaskImplementation).TaskComplete();
					}
					stopwatch.Stop();
					LoggerExtensions.LogDebug(_log, "Executed: project=" + _currentAutomaticTask.Project.Name + ", " + $"task={_currentAutomaticTask.Name}, duration={stopwatch.ElapsedMilliseconds / 1000}s", Array.Empty<object>());
					stopwatch.Reset();
					lock (_lockObject)
					{
						if ((int)_currentAutomaticTask.Status == 3 || (int)_currentAutomaticTask.Status == 9)
						{
							if (_currentAutomaticTask.Result.HasErrors || GetFailedFiles((IAutomaticTask)(object)_currentAutomaticTask) > 0)
							{
								_currentAutomaticTask.SetFailed();
								goto end_IL_0161;
							}
							_currentAutomaticTask.SetCompleted();
							_currentExecutingAutomaticTask.PopulateOutputFiles();
							if (i < _executingAutomaticTasks.Length - 1)
							{
								ExecutingAutomaticTask executingAutomaticTask = _executingAutomaticTasks[_currentExecutingAutomaticTaskIndex + 1];
								executingAutomaticTask.TransferTaskFiles(_currentExecutingAutomaticTask);
							}
							continue;
						}
						if ((int)_currentAutomaticTask.Status == 6)
						{
							OnCancelled();
						}
						end_IL_0161:;
					}
					break;
				}
				catch (CancelException)
				{
					OnCancelled();
					break;
				}
				catch (ProjectApiException val2)
				{
					ProjectApiException val3 = val2;
					_currentExecutingAutomaticTask.ReportMessage("AutomaticTaskExecuter", ((Exception)(object)val3).Message, (MessageLevel)2, (Exception)(object)val3);
					_currentAutomaticTask.SetFailed();
					break;
				}
				catch (Exception ex)
				{
					_currentExecutingAutomaticTask.ReportMessage("AutomaticTaskExecuter", ex.Message, (MessageLevel)2, ex);
					_currentAutomaticTask.SetFailed();
					break;
				}
				finally
				{
					if (_currentAutomaticTaskImplementation != null)
					{
						_currentAutomaticTaskImplementation.Progress -= _currentAutomaticTaskImplementation_Progress;
					}
					try
					{
						if (Directory.Exists(_currentExecutingAutomaticTask.WorkingFolder))
						{
							RobustDeleteDirectory(_currentExecutingAutomaticTask.WorkingFolder);
						}
						string localDataFolder = _currentAutomaticTask.Project.LocalDataFolder;
						if (Directory.Exists(localDataFolder) && Directory.GetFiles(localDataFolder).Length == 0 && Directory.GetDirectories(localDataFolder).Length == 0)
						{
							RobustDeleteDirectory(localDataFolder);
						}
					}
					catch
					{
					}
					if (_currentAutomaticTaskImplementation is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			_isCompleted = true;
		}

		private void _currentAutomaticTaskImplementation_Progress(object sender, TaskProgressEventArgs e)
		{
			if (e.CurrentOperationDescription != null)
			{
				_currentAutomaticTask.SetPercentComplete(e.PercentComplete);
			}
			else
			{
				_currentAutomaticTask.SetPercentComplete(e.PercentComplete, e.CurrentOperationDescription);
			}
		}

		private void RobustCreateDirectory(string dir)
		{
			int num = 0;
			while (true)
			{
				try
				{
					Directory.CreateDirectory(dir);
					break;
				}
				catch (Exception ex)
				{
					if (num++ < 5)
					{
						LoggerExtensions.LogError(_log, ex, "Failed to create directory '" + dir + "', retrying...", Array.Empty<object>());
						Thread.Sleep(500);
						continue;
					}
					LoggerExtensions.LogError(_log, ex, "Failed to create directory '" + dir + "', max retry count reached.", new object[1] { dir });
					throw ex;
				}
			}
		}

		private void RobustDeleteDirectory(string dir)
		{
			int num = 0;
			while (true)
			{
				try
				{
					Directory.Delete(dir, recursive: true);
					break;
				}
				catch (Exception ex)
				{
					if (num++ < 5)
					{
						LoggerExtensions.LogError(_log, ex, "Failed to delete directory '" + dir + "', retrying...", Array.Empty<object>());
						Thread.Sleep(500);
						continue;
					}
					LoggerExtensions.LogError(_log, ex, "Failed to delete directory '" + dir + "', max retry count reached.", new object[1] { dir });
					throw ex;
				}
			}
		}

		private int GetFailedFiles(IAutomaticTask t)
		{
			int num = 0;
			ITaskFile[] files = ((IScheduledTask)t).Files;
			foreach (ITaskFile val in files)
			{
				if (((ITaskEntity)val).IsFailed)
				{
					num++;
				}
			}
			return num;
		}

		public void Cancel()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			lock (_lockObject)
			{
				if (!_isCompleted && _currentAutomaticTaskImplementation != null && (int)_currentAutomaticTask.Status == 3)
				{
					_currentAutomaticTask.SetCancelling();
					((IAbstractTaskImplementation)_currentAutomaticTaskImplementation).Cancel(true);
				}
			}
		}

		private void OnCancelled()
		{
			for (int i = _currentExecutingAutomaticTaskIndex; i < _executingAutomaticTasks.Length; i++)
			{
				AutomaticTask automaticTask = _executingAutomaticTasks[i].AutomaticTask;
				automaticTask.SetCancelled();
			}
		}
	}
}
