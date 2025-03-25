using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Server.ProjectSyncOperations;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;
using Sdl.StudioServer.ProjectServer.Package;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ProjectSyncOperation : AbstractCommuteSyncOperation
	{
		private readonly Project _project;

		private readonly ILogger _log;

		private readonly IProjectSettingsUpdater _settingsUpdater;

		public IProject Project => (IProject)(object)_project;

		public override string Description => string.Format(StringResources.ProjectSyncOperation_Description, _project.Name);

		public override bool IsFullProjectUpdate => true;

		public ProjectSyncOperation(Project project, IProjectSettingsUpdater settingsUpdater, ILogger<ProjectSyncOperation> logger)
		{
			if (project == null)
			{
				throw new ArgumentNullException("project");
			}
			if (!project.IsPublished)
			{
				throw new ArgumentException("Can only synchronize projects that have been published to a server.");
			}
			_project = project;
			_settingsUpdater = settingsUpdater;
			_log = (ILogger)(object)logger;
		}

		public override bool ShouldExecute()
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Invalid comparison between Unknown and I4
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Expected I4, but got Unknown
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Invalid comparison between Unknown and I4
			if (!AbstractCommuteSyncOperation.IsServerAvailable((IProject)(object)_project))
			{
				LoggerExtensions.LogDebug(_log, "Skipping project '" + _project.Name + "'. The server is not available or the user is logged on with the wrong user name.", Array.Empty<object>());
				return false;
			}
			if ((int)_project.PublishProjectOperation.Status == 8)
			{
				return false;
			}
			LoggerExtensions.LogDebug(_log, "Checking for new synchronization package for project '" + _project.Name + "'...", Array.Empty<object>());
			ICommuteClient val = _project.CreateCommuteClient();
			DateTime? lastSynchronizationTimestamp = _project.LastSynchronizationTimestamp;
			DateTime dateTime = (lastSynchronizationTimestamp.HasValue ? lastSynchronizationTimestamp.Value : (DateTime.UtcNow - TimeSpan.FromDays(1000.0)));
			SynchronizationPackageInfo val2 = val.HasNewSynchronizationPackage(_project.Guid, dateTime);
			ServerProjectValidity validity = val2.Validity;
			switch ((int)validity)
			{
			case 0:
				if (_project.ExecuteOperation("ServerUserHasPermissionOperation", null).IsSuccesful)
				{
					_project.PublishProjectOperationImpl.MarkAsAccessible();
				}
				if ((int)_project.Status == 4)
				{
					_project.PublishProjectOperationImpl.MarkAsArchived();
				}
				LoggerExtensions.LogDebug(_log, val2.NewPackageTimeStamp.HasValue ? $"New synchronization package is available for project {_project.Name} (last sync package={dateTime.Ticks})" : $"Project {_project.Name} is up-to-date (last sync package={dateTime.Ticks})", Array.Empty<object>());
				return val2.NewPackageTimeStamp.HasValue;
			case 1:
				return false;
			case 2:
				return true;
			case 3:
				_project.PublishProjectOperationImpl.MarkAsInaccessible();
				return false;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		[Conditional("DEBUG")]
		private static void DebugOutPackage(MemoryStream packageStream)
		{
			string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			using (FileStream fileStream = File.OpenWrite(path))
			{
				byte[] array = new byte[packageStream.Length];
				packageStream.Seek(0L, SeekOrigin.Begin);
				packageStream.Read(array, 0, array.Length);
				fileStream.Write(array, 0, array.Length);
			}
			File.Delete(path);
		}

		public override void Execute()
		{
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected I4, but got Unknown
			string text = null;
			try
			{
				ICommuteClient val = _project.CreateCommuteClient();
				DateTime? lastSynchronizationTimestamp = _project.LastSynchronizationTimestamp;
				LoggerExtensions.LogDebug(_log, "Downloading synchronization package for project " + _project.Name, Array.Empty<object>());
				text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				MemoryStream memoryStream = new MemoryStream();
				SynchronizationPackageInfo val2 = val.DownloadSynchronizationPackage(_project.Guid, (Stream)memoryStream, lastSynchronizationTimestamp.HasValue ? lastSynchronizationTimestamp.Value : (DateTime.Now - TimeSpan.FromDays(1000.0)), (EventHandler<DownloadProjectPackageEventArgs>)delegate
				{
				});
				memoryStream.Seek(0L, SeekOrigin.Begin);
				ServerProjectValidity validity = val2.Validity;
				switch (validity - 1)
				{
				case 0:
					return;
				case 1:
					_project.PublishProjectOperationImpl.MarkAsDeletedFromServer();
					return;
				case 2:
					_project.PublishProjectOperationImpl.MarkAsInaccessible();
					return;
				}
				_project.PublishProjectOperationImpl.MarkAsAccessible();
				if (!val2.NewPackageTimeStamp.HasValue)
				{
					return;
				}
				DateTime? lastSynchronizationTimestamp2 = _project.LastSynchronizationTimestamp;
				if (lastSynchronizationTimestamp2 == lastSynchronizationTimestamp)
				{
					if (lastSynchronizationTimestamp != val2.NewPackageTimeStamp.Value)
					{
						LoggerExtensions.LogDebug(_log, string.Format("Downloaded new sync package for project {0}; applying sync package to project... (last sync package={1}, new sync package={2})", _project.Name, lastSynchronizationTimestamp.HasValue ? lastSynchronizationTimestamp.Value.Ticks.ToString() : "never", val2.NewPackageTimeStamp.Value.Ticks), Array.Empty<object>());
						ApplySynchronizationData(memoryStream);
						_project.LastSynchronizationTimestamp = val2.NewPackageTimeStamp.Value;
						LoggerExtensions.LogDebug(_log, "Applying sync package for project " + _project.Name + " completed.", Array.Empty<object>());
					}
					else
					{
						LoggerExtensions.LogDebug(_log, $"Project {_project.Name} is up-to-date (last sync package={lastSynchronizationTimestamp.Value.Ticks})", Array.Empty<object>());
					}
				}
				else
				{
					LoggerExtensions.LogDebug(_log, string.Format("Synchronization for project project {0} aborted because of another conflicting sync operation was ongoing. (last sync package={1}, conflicting sync package={2})", _project.Name, lastSynchronizationTimestamp.HasValue ? lastSynchronizationTimestamp.Value.Ticks.ToString() : "never", lastSynchronizationTimestamp2.HasValue ? lastSynchronizationTimestamp2.Value.Ticks.ToString() : "never"), Array.Empty<object>());
				}
			}
			finally
			{
				if (text != null)
				{
					File.Delete(text);
				}
			}
		}

		private void ApplySynchronizationData(Stream packageStream)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				ProjectPackage package = new ProjectPackage(packageStream, (PackageAccessMode)1);
				try
				{
					IBackgroundProjectUpdater backgroundProjectUpdater = ((IProjectConfiguration)Project).ProjectsProvider.Application.BackgroundProjectUpdater;
					backgroundProjectUpdater.PerformProjectUpdate(Project, (Action)delegate
					{
						PerformProjectUpdate(package.GetManifest(), _settingsUpdater);
					});
				}
				finally
				{
					if (package != null)
					{
						((IDisposable)package).Dispose();
					}
				}
			}
			finally
			{
				if (Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
			}
		}

		private void PerformProjectUpdate(Stream packageManifestStream, IProjectSettingsUpdater settingsUpdater)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			using MemoryStream projectManifestStream = PackageTransforms.TransformPackageToProject(packageManifestStream, Project.PublishProjectOperation.ServerUri, Project.PublishProjectOperation.ServerUserName, Project.PublishProjectOperation.ServerUserType);
			ProjectRepositorySerializer projectRepositorySerializer = new ProjectRepositorySerializer();
			Sdl.ProjectApi.Implementation.Xml.Project xmlProject = projectRepositorySerializer.Deserialize(projectManifestStream);
			List<IGroupshareSyncOperation> list = new List<IGroupshareSyncOperation>
			{
				new SettingsSync(settingsUpdater),
				new TermbaseSettingsSync(),
				new GroupshareProjectFileSync()
			};
			foreach (IGroupshareSyncOperation item in list)
			{
				item.SyncData((IProject)(object)_project, xmlProject, _project.ProjectRepository);
			}
			_project.ResetAnalysisBands();
		}
	}
}
