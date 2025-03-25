using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class CommuteServerSyncDaemon
	{
		private readonly ILogger _log = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<CommuteServerSyncDaemon>(LogProvider.GetLoggerFactory());

		private readonly object _syncObject = new object();

		private Thread _syncThread;

		private bool _stop;

		private int _suspended;

		private readonly List<ICommuteSyncOperation> _syncOperations = new List<ICommuteSyncOperation>();

		private int _synchronizationIntervalMilliseconds = 5000;

		private AutoResetEvent _stopEvent;

		public bool IsStarted { get; private set; }

		public bool IsSynchronizing { get; private set; }

		public int SynchronizationIntervalMilliseconds
		{
			get
			{
				return _synchronizationIntervalMilliseconds;
			}
			set
			{
				if (_synchronizationIntervalMilliseconds < 1000)
				{
					throw new ArgumentOutOfRangeException("SynchronizationIntervalMilliseconds", "The polling interval should be at least 1000ms.");
				}
				_synchronizationIntervalMilliseconds = value;
			}
		}

		public event EventHandler<SyncEventArgs> SyncStart;

		public event EventHandler<SyncOperationEventArgs> SyncOperation;

		public event EventHandler<SyncEventArgs> SyncComplete;

		public ICommuteSyncOperation[] GetSyncOperations()
		{
			lock (_syncObject)
			{
				return _syncOperations.ToArray();
			}
		}

		public void AddSyncOperation(ICommuteSyncOperation syncOperation)
		{
			lock (_syncObject)
			{
				_syncOperations.Add(syncOperation);
			}
		}

		public void RemoveSyncOperation(ICommuteSyncOperation syncOperation)
		{
			lock (_syncObject)
			{
				_syncOperations.Remove(syncOperation);
			}
		}

		public void Start()
		{
			lock (_syncObject)
			{
				if (IsStarted)
				{
					throw new InvalidOperationException("The Commute sync thread is already running.");
				}
				_stopEvent = new AutoResetEvent(initialState: false);
				LoggerExtensions.LogDebug(_log, "Starting Project Server synchronization thread.", Array.Empty<object>());
				_syncThread = new Thread(SyncLoop)
				{
					IsBackground = true
				};
				try
				{
					_syncThread.Start();
				}
				catch (Exception ex)
				{
					_syncThread = null;
					LoggerExtensions.LogDebug(_log, "Failed to start Project Server synchronization thread. Reason:" + ex.ToString(), Array.Empty<object>());
					throw new Exception("Failed to start Project Server synchronization thread", ex);
				}
				IsStarted = true;
			}
		}

		public void Stop()
		{
			lock (_syncObject)
			{
				if (!IsStarted)
				{
					throw new InvalidOperationException("The Commute sync thread is not running.");
				}
				if (_stop)
				{
					throw new InvalidOperationException("The Commute sync thread is already being stopped.");
				}
				_stop = true;
			}
			LoggerExtensions.LogDebug(_log, "Stopping Project Server synchronization thread.", Array.Empty<object>());
			try
			{
				_stopEvent.Set();
				_syncThread.Join();
			}
			catch (ThreadInterruptedException)
			{
			}
			finally
			{
				lock (_syncObject)
				{
					IsStarted = false;
					_stop = false;
					_syncThread = null;
					_stopEvent.Dispose();
					_stopEvent = null;
				}
			}
		}

		public void Suspend()
		{
			bool flag = false;
			lock (_syncObject)
			{
				flag = _suspended == 0;
				_suspended++;
				LoggerExtensions.LogDebug(_log, $"Project Server synchronization thread suspend request ({_suspended})", Array.Empty<object>());
			}
			if (flag)
			{
				Stop();
			}
		}

		public void Resume()
		{
			bool flag = false;
			lock (_syncObject)
			{
				_suspended--;
				flag = _suspended == 0;
				LoggerExtensions.LogDebug(_log, $"Project Server synchronization thread resume request ({_suspended})", Array.Empty<object>());
			}
			if (flag)
			{
				Start();
			}
		}

		private void SyncLoop()
		{
			try
			{
				while (!_stopEvent.WaitOne(_synchronizationIntervalMilliseconds))
				{
					ICommuteSyncOperation[] array;
					lock (_syncObject)
					{
						array = _syncOperations.ToArray();
					}
					LoggerExtensions.LogDebug(_log, "Started Project Server synchronization iteration.", Array.Empty<object>());
					bool[] array2 = new bool[array.Length];
					bool includesProjectSync = false;
					for (int i = 0; i < array2.Length; i++)
					{
						try
						{
							LoggerExtensions.LogDebug(_log, "Checking:" + array[i].Description, Array.Empty<object>());
							array2[i] = array[i].ShouldExecute();
							if (array[i].IsFullProjectUpdate && array2[i])
							{
								includesProjectSync = true;
							}
						}
						catch (Exception ex)
						{
							LoggerExtensions.LogError(_log, ex, "Failed to synchronize project", Array.Empty<object>());
							array2[i] = false;
						}
					}
					bool flag = array2.Any((bool b) => b);
					if (_stop)
					{
						break;
					}
					if (flag)
					{
						LoggerExtensions.LogDebug(_log, "Performing synchronization...", Array.Empty<object>());
						OnSyncStart(includesProjectSync);
						for (int j = 0; j < array.Length; j++)
						{
							ICommuteSyncOperation commuteSyncOperation = array[j];
							try
							{
								if (array2[j])
								{
									LoggerExtensions.LogDebug(_log, "Executing: " + commuteSyncOperation.Description, Array.Empty<object>());
									OnSyncOperation(commuteSyncOperation, (SyncOperationStatus)0, null);
									commuteSyncOperation.Execute();
									OnSyncOperation(commuteSyncOperation, (SyncOperationStatus)1, null);
								}
								else
								{
									LoggerExtensions.LogDebug(_log, "Skipped: " + commuteSyncOperation.Description, Array.Empty<object>());
								}
								if (_stop)
								{
									return;
								}
							}
							catch (Exception ex2)
							{
								LoggerExtensions.LogError(_log, ex2, "Unexpected exception during synchronization (" + commuteSyncOperation.Description + ")", Array.Empty<object>());
								OnSyncOperation(commuteSyncOperation, (SyncOperationStatus)2, ex2);
							}
						}
						LoggerExtensions.LogDebug(_log, "Synchronization iteration completed.", Array.Empty<object>());
						OnSyncComplete(includesProjectSync);
					}
					else
					{
						LoggerExtensions.LogDebug(_log, "No synchronization needed. Project Server synchronization iteration completed.", Array.Empty<object>());
					}
				}
			}
			finally
			{
				IsSynchronizing = false;
				IsStarted = false;
				_stop = false;
				LoggerExtensions.LogDebug(_log, "Project Server synchronization thread stopped", Array.Empty<object>());
			}
		}

		private void OnSyncStart(bool includesProjectSync)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			IsSynchronizing = true;
			EventHandler<SyncEventArgs> syncStart = this.SyncStart;
			if (syncStart != null)
			{
				try
				{
					syncStart(this, new SyncEventArgs(includesProjectSync));
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(_log, ex, ex.Message, Array.Empty<object>());
				}
			}
		}

		private void OnSyncComplete(bool includesProjectSync)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			IsSynchronizing = false;
			EventHandler<SyncEventArgs> syncComplete = this.SyncComplete;
			if (syncComplete != null)
			{
				try
				{
					syncComplete(this, new SyncEventArgs(includesProjectSync));
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(_log, ex, ex.Message, Array.Empty<object>());
				}
			}
		}

		private void OnSyncOperation(ICommuteSyncOperation syncOperation, SyncOperationStatus status, Exception ex)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			if (ex != null)
			{
				LoggerExtensions.LogError(_log, ex, ex.Message, Array.Empty<object>());
			}
			EventHandler<SyncOperationEventArgs> syncOperation2 = this.SyncOperation;
			if (syncOperation2 != null)
			{
				try
				{
					syncOperation2(this, new SyncOperationEventArgs(syncOperation.Description, syncOperation.IsFullProjectUpdate, status, ex));
				}
				catch (Exception ex2)
				{
					LoggerExtensions.LogError(_log, ex2, ex2.Message, Array.Empty<object>());
				}
			}
		}
	}
}
