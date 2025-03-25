using System;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class CommuteServerManager : ICommuteServerManager
	{
		private CommuteServerSyncDaemon _syncDaemon;

		private bool _online;

		private bool _autoDetectOnline;

		private readonly ProjectsListSyncOperation _listSyncOperation;

		private readonly ILoggerFactory _loggerFactory;

		public bool IsSynchronizationThreadRunning => _syncDaemon.IsStarted;

		public bool IsSynchronizing => _syncDaemon.IsSynchronizing;

		public int SynchronizationIntervalMilliseconds
		{
			get
			{
				return _syncDaemon.SynchronizationIntervalMilliseconds;
			}
			set
			{
				_syncDaemon.SynchronizationIntervalMilliseconds = value;
			}
		}

		public bool Online
		{
			get
			{
				return _online;
			}
			set
			{
				if (_online == value)
				{
					return;
				}
				_online = value;
				if (value)
				{
					if (!_syncDaemon.IsStarted)
					{
						_syncDaemon.Start();
					}
				}
				else if (_syncDaemon.IsStarted)
				{
					_syncDaemon.Stop();
				}
			}
		}

		public bool AutoDetectOnline
		{
			get
			{
				return _autoDetectOnline;
			}
			set
			{
				if (_autoDetectOnline != value)
				{
					_autoDetectOnline = value;
					if (value)
					{
						StartNetworkChangeMonitoring();
					}
					else
					{
						StopNetWorkChangeMonitoring();
					}
				}
			}
		}

		public bool HasNetworkConnection { get; private set; }

		public event EventHandler<SyncEventArgs> SyncStart;

		public event EventHandler<SyncOperationEventArgs> SyncOperation;

		public event EventHandler<SyncEventArgs> SyncComplete;

		public event EventHandler OnlineChanged;

		public CommuteServerManager(IProjectsProvider projectsProvider, ILoggerFactory loggerFactory)
		{
			CheckNetworkAvailable();
			_online = false;
			AutoDetectOnline = true;
			_listSyncOperation = new ProjectsListSyncOperation(projectsProvider, loggerFactory);
			CreateSyncDaemon();
			_loggerFactory = loggerFactory;
		}

		private void CreateSyncDaemon()
		{
			_syncDaemon = new CommuteServerSyncDaemon();
			_syncDaemon.SyncOperation += delegate(object sender, SyncOperationEventArgs e)
			{
				this.SyncOperation?.Invoke(this, e);
			};
			_syncDaemon.SyncStart += delegate(object sender, SyncEventArgs e)
			{
				this.SyncStart?.Invoke(this, e);
			};
			_syncDaemon.SyncComplete += delegate(object sender, SyncEventArgs e)
			{
				this.SyncComplete?.Invoke(this, e);
			};
		}

		public void StartSynchronizationThread()
		{
			CheckNetworkAvailable();
			if (HasNetworkConnection && !_syncDaemon.IsStarted)
			{
				_online = true;
				_syncDaemon.Start();
			}
		}

		public void StopSynchronizationThread()
		{
			if (_syncDaemon.IsStarted)
			{
				_online = false;
				_syncDaemon.Stop();
			}
		}

		public void SuspendSynchronizationThread()
		{
			_syncDaemon.Suspend();
		}

		public void ResumeSynchronizationThread()
		{
			_syncDaemon.Resume();
		}

		private void StartNetworkChangeMonitoring()
		{
			NetworkChange.NetworkAvailabilityChanged += NetworkChangeOnNetworkAvailabilityChanged;
			NetworkChange.NetworkAddressChanged += NetworkChangeNetworkAddressChanged;
			CheckNetworkAvailable();
		}

		private void StopNetWorkChangeMonitoring()
		{
			NetworkChange.NetworkAvailabilityChanged -= NetworkChangeOnNetworkAvailabilityChanged;
			NetworkChange.NetworkAddressChanged -= NetworkChangeNetworkAddressChanged;
		}

		private void NetworkChangeNetworkAddressChanged(object sender, EventArgs e)
		{
			OnNetworkChanged();
		}

		private void NetworkChangeOnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs networkAvailabilityEventArgs)
		{
			OnNetworkChanged();
		}

		private void OnNetworkChanged()
		{
			CheckNetworkAvailable();
			if (AutoDetectOnline)
			{
				Online = HasNetworkConnection;
			}
		}

		private void CheckNetworkAvailable()
		{
			HasNetworkConnection = CheckNetworkConnection();
		}

		private bool CheckNetworkConnection()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				return false;
			}
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel && networkInterface.Speed >= 10000000 && networkInterface.Description.IndexOf("virtual ethernet", StringComparison.OrdinalIgnoreCase) < 0 && networkInterface.Name.IndexOf("virtual ethernet", StringComparison.OrdinalIgnoreCase) < 0 && !networkInterface.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public void StartProjectSynchronization(IProject project)
		{
			if (!project.IsPublished)
			{
				throw new ArgumentException("Only server-based projects can be synchronized.", "project");
			}
			if (IsSynchronizingProject(project))
			{
				throw new InvalidOperationException($"Project {project.Guid} is already being synchronized.");
			}
			ILogger<ProjectSyncOperation> logger = LoggerFactoryExtensions.CreateLogger<ProjectSyncOperation>(_loggerFactory);
			_syncDaemon.AddSyncOperation(new ProjectSyncOperation((Project)(object)project, (IProjectSettingsUpdater)(object)new ProjectSettingsUpdater(), logger));
		}

		public void StopProjectSynchronization(IProject project)
		{
			ICommuteSyncOperation[] syncOperations = _syncDaemon.GetSyncOperations();
			ICommuteSyncOperation commuteSyncOperation = syncOperations.FirstOrDefault((ICommuteSyncOperation op) => op is ProjectSyncOperation projectSyncOperation && projectSyncOperation.Project.Guid == project.Guid);
			if (commuteSyncOperation != null)
			{
				_syncDaemon.RemoveSyncOperation(commuteSyncOperation);
			}
		}

		public bool IsSynchronizingProject(IProject project)
		{
			ICommuteSyncOperation[] syncOperations = _syncDaemon.GetSyncOperations();
			return syncOperations.FirstOrDefault((ICommuteSyncOperation op) => op is ProjectSyncOperation projectSyncOperation && projectSyncOperation.Project.Guid == project.Guid) != null;
		}

		public void StartProjectListSynchronization()
		{
			_syncDaemon.AddSyncOperation(_listSyncOperation);
		}

		public void StopProjectListSynchronization()
		{
			_syncDaemon.RemoveSyncOperation(_listSyncOperation);
		}
	}
}
