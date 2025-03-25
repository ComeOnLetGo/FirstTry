using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sdl.ProjectApi.Implementation.Server.ProjectSyncOperations;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal class ProjectsListSyncOperation : AbstractCommuteSyncOperation
	{
		private readonly ILogger _log;

		private readonly IProjectsProvider _projectsProvider;

		public override string Description => StringResources.ProjectsListSyncOperation_Description;

		public override bool IsFullProjectUpdate => false;

		public ProjectsListSyncOperation(IProjectsProvider projectsProvider, ILoggerFactory loggerFactory)
		{
			_projectsProvider = projectsProvider;
			_log = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ProjectsListSyncOperation>(loggerFactory);
		}

		public override bool ShouldExecute()
		{
			return GetProjectsToSync().Count > 0;
		}

		public override void Execute()
		{
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Expected I4, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Invalid comparison between Unknown and I4
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			List<IProject> projectsToSync = GetProjectsToSync();
			IEnumerable<IGrouping<Uri, IProject>> enumerable = from p in projectsToSync
				group p by p.PublishProjectOperation.ServerUri;
			foreach (IGrouping<Uri, IProject> item in enumerable)
			{
				Uri key = item.Key;
				ICommuteClient val = _projectsProvider.Application.CommuteClientManager.CreateCommuteClient(key);
				LoggerExtensions.LogDebug(_log, "Getting status of server projects.", Array.Empty<object>());
				try
				{
					ServerProjectInfo[] serverProjectInformation = val.GetServerProjectInformation(item.Select((IProject p) => p.Guid).ToArray());
					int num = 0;
					foreach (Project item2 in item)
					{
						ServerProjectInfo val2 = serverProjectInformation[num];
						LoggerExtensions.LogDebug(_log, "Applying publication status information for project '" + item2.Name + "'.", Array.Empty<object>());
						lock (item2.ProjectsProvider.SyncRoot)
						{
							DateTime utcNow = DateTime.UtcNow;
							PublishProjectOperationSettings settingsGroup = item2.GetSettingsGroup<PublishProjectOperationSettings>();
							settingsGroup.LastSyncedAt.Value = utcNow;
							if (item2.IsLoaded)
							{
								PublishProjectOperationSettings settingsGroup2 = item2.Settings.GetSettingsGroup<PublishProjectOperationSettings>();
								settingsGroup2.LastSyncedAt.Value = utcNow;
							}
							ServerProjectValidity validity = val2.Validity;
							switch ((int)validity)
							{
							case 0:
							{
								if ((int)val2.Project.Status == 8)
								{
									item2.PublishProjectOperationImpl.MarkAsArchived();
								}
								else
								{
									item2.PublishProjectOperationImpl.MarkAsPublished();
								}
								SettingsSync settingsSync = new SettingsSync((IProjectSettingsUpdater)(object)new ProjectSettingsUpdater());
								string name = val2.Project.Name;
								string description = val2.Project.Description;
								DateTime? dueDate = (val2.Project.DueDate.HasValue ? new DateTime?(val2.Project.DueDate.Value.ToLocalTime()) : null);
								string organizationPath = val2.Project.OrganizationPath;
								ProjectStatus status = val2.Project.Status.ToProjectStatus();
								DateTime? completedAt = val2.Project.CompletedAt;
								Customer customer = val2.Project.Customer;
								string customerName = ((customer != null) ? customer.Name : null);
								Customer customer2 = val2.Project.Customer;
								settingsSync.ApplySynchronizationInfo(name, description, dueDate, organizationPath, status, completedAt, customerName, (customer2 != null) ? customer2.Email : null, (IProject)(object)item2);
								break;
							}
							case 1:
								item2.PublishProjectOperationImpl.ApplyStatusInformation(val2.PublishProjectInfo);
								break;
							case 2:
								item2.PublishProjectOperationImpl.MarkAsDeletedFromServer();
								break;
							case 3:
								item2.PublishProjectOperationImpl.MarkAsInaccessible();
								break;
							default:
								throw new ArgumentOutOfRangeException();
							}
						}
						num++;
					}
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(_log, ex, ex.Message, Array.Empty<object>());
				}
			}
		}

		private List<IProject> GetProjectsToSync()
		{
			lock (_projectsProvider.SyncRoot)
			{
				List<IProject> list = new List<IProject>();
				IProject[] myProjects = _projectsProvider.MyProjects;
				foreach (IProject val in myProjects)
				{
					if (ShouldSyncProject(val))
					{
						if (AbstractCommuteSyncOperation.IsServerAvailable(val))
						{
							LoggerExtensions.LogDebug(_log, "Including project '" + val.Name + "'.", Array.Empty<object>());
							list.Add(val);
						}
						else
						{
							LoggerExtensions.LogDebug(_log, "Skipping project '" + val.Name + "'. Server is not available or the user is logged on with the wrong user name.", Array.Empty<object>());
						}
					}
					else
					{
						LoggerExtensions.LogDebug(_log, "Skipping project '" + val.Name + "'. Local project.", Array.Empty<object>());
					}
				}
				return list;
			}
		}

		private static bool ShouldSyncProject(IProject project)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Invalid comparison between Unknown and I4
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Invalid comparison between Unknown and I4
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Invalid comparison between Unknown and I4
			bool flag = project.PublishProjectOperation.ServerUri != null && !project.PublishProjectOperation.IsExecuting && !project.PublishProjectOperation.IsFinished;
			bool flag2 = (int)project.ProjectType == 6 && (int)project.Status == 4;
			bool flag3 = (int)project.ProjectType == 6 && (int)project.Status == 3;
			return flag || flag2 || flag3;
		}
	}
}
