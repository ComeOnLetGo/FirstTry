using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal class OpenServerProjectOperation : IOpenServerProjectOperation, IObjectWithExecutionResult
	{
		private ExecutionResult _lazyResultImpl;

		private readonly ICommuteClientManager _commuteClientManager;

		private readonly IProjectPathUtil _projectPathUtil;

		private readonly ProjectsProvider _serverProjectProvider;

		public Uri ServerUri { get; }

		public Guid ProjectGuid { get; }

		public string LocalProjectFolder { get; }

		public OpenServerProjectStatus Status { get; private set; }

		public IExecutionResult Result => (IExecutionResult)(object)ResultImpl;

		private ExecutionResult ResultImpl
		{
			get
			{
				if (_lazyResultImpl == null)
				{
					_lazyResultImpl = new ExecutionResult(new Sdl.ProjectApi.Implementation.Xml.ExecutionResult(), this);
				}
				return _lazyResultImpl;
			}
		}

		public IProject Project { get; private set; }

		public bool IsFinished
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Invalid comparison between Unknown and I4
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Invalid comparison between Unknown and I4
				if ((int)Status != 4 && (int)Status != 2)
				{
					return (int)Status == 5;
				}
				return true;
			}
		}

		public int PercentComplete { get; private set; }

		public string CurrentOperationDescription { get; private set; }

		public event EventHandler Progress;

		public OpenServerProjectOperation(ICommuteClientManager commuteClientManager, IProjectsProvider server, Uri serverUri, Guid projectGuid, string localProjectFolder, IProjectPathUtil projectPathUtil)
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			if (serverUri == null)
			{
				throw new ArgumentNullException("serverUri");
			}
			if (!CommuteClientManager.IsValidCommuteClientUri(serverUri))
			{
				throw new ArgumentNullException("serverUri", string.Format(CultureInfo.CurrentCulture, "Invalid project server URI: {0}", serverUri));
			}
			if (projectGuid == Guid.Empty)
			{
				throw new ArgumentNullException("projectGuid");
			}
			if (server.GetProject(projectGuid) != null)
			{
				throw new ProjectApiException(ErrorMessages.OpenServerProject_AlreadyOpen);
			}
			if (string.IsNullOrEmpty(localProjectFolder))
			{
				throw new ArgumentNullException("localProjectFolder");
			}
			if (!Util.IsEmptyOrNonExistingDirectory(localProjectFolder))
			{
				throw new ArgumentException(ErrorMessages.OpenServerProject_NonEmptyLocaldataFolder);
			}
			_serverProjectProvider = server as ProjectsProvider;
			_commuteClientManager = commuteClientManager;
			ServerUri = serverUri;
			ProjectGuid = projectGuid;
			LocalProjectFolder = localProjectFolder;
			_projectPathUtil = projectPathUtil;
		}

		public void Execute()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Invalid comparison between Unknown and I4
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Expected O, but got Unknown
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Invalid comparison between Unknown and I4
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Invalid comparison between Unknown and I4
			string text = null;
			try
			{
				if ((int)Status == 1 || (int)Status == 3)
				{
					throw new InvalidOperationException(ErrorMessages.OpenServerProject_CannotExecuteStillRunning);
				}
				if ((int)Status == 5)
				{
					throw new InvalidOperationException(ErrorMessages.OpenServerProject_CannotExecuteCompleted);
				}
				Status = (OpenServerProjectStatus)1;
				OnProgress();
				if (CheckCancelRequested())
				{
					return;
				}
				text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				if (!DownloadPackage(text, out var packageTimeStamp))
				{
					Status = (OpenServerProjectStatus)4;
					return;
				}
				OnProgress();
				if (CheckCancelRequested())
				{
					return;
				}
				OpenServerProjectPackageImport packageImport = new OpenServerProjectPackageImport((IProjectsProvider)(object)_serverProjectProvider, ServerUri, GetServerUserName(), GetServerUserType(), text, packageTimeStamp, LocalProjectFolder, _projectPathUtil);
				packageImport.StatusChanged += (PackageOperationStatusChangedEventHandler)delegate(IPackageOperation packageOperation, PackageStatus status)
				{
					//IL_005a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0060: Invalid comparison between Unknown and I4
					//IL_0068: Unknown result type (might be due to invalid IL or missing references)
					//IL_006e: Invalid comparison between Unknown and I4
					CurrentOperationDescription = packageOperation.CurrentOperationDescription;
					if (string.IsNullOrEmpty(CurrentOperationDescription))
					{
						CurrentOperationDescription = StringResources.OpenServerProjectOperation_OpeningPackage;
					}
					PercentComplete = 75 + packageOperation.PercentComplete / 4;
					OnProgress();
					if ((int)Status == 3 && (int)packageImport.Status == 3)
					{
						packageImport.Cancel();
					}
				};
				packageImport.Result.MessageReported += Result_MessageReported;
				packageImport.Start();
				if ((int)packageImport.Status == 4)
				{
					Status = (OpenServerProjectStatus)2;
					return;
				}
				if ((int)packageImport.Status == 7)
				{
					Status = (OpenServerProjectStatus)4;
					return;
				}
				if (CheckCancelRequested())
				{
					packageImport.Project.Delete();
				}
				Project = packageImport.Project;
				Status = (OpenServerProjectStatus)5;
			}
			catch (Exception exception)
			{
				Status = (OpenServerProjectStatus)2;
				ResultImpl.ReportMessage(StringResources.OpenServerProjectOperation_MessageSource, StringResources.OpenServerProjectOperation_Failed, (MessageLevel)2, exception);
			}
			finally
			{
				DeleteTempPackageFile(text);
			}
		}

		private static void DeleteTempPackageFile(string tempPackageFilePath)
		{
			try
			{
				if (tempPackageFilePath != null && File.Exists(tempPackageFilePath))
				{
					File.Delete(tempPackageFilePath);
				}
			}
			catch (Exception)
			{
			}
		}

		private bool DownloadPackage(string packageFilePath, out DateTime packageTimeStamp)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected I4, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			ICommuteClient val = _commuteClientManager.CreateCommuteClient(ServerUri);
			SynchronizationPackageInfo val2 = val.DownloadProjectPackage(ProjectGuid, packageFilePath, (EventHandler<DownloadProjectPackageEventArgs>)delegate(object sender, DownloadProjectPackageEventArgs args)
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Invalid comparison between Unknown and I4
				CurrentOperationDescription = args.CurrentOperationDescription;
				PercentComplete = args.PercentComplete * 3 / 4;
				EventHandler progress = this.Progress;
				if (progress != null)
				{
					progress(this, EventArgs.Empty);
					if ((int)Status == 3)
					{
						((CancelEventArgs)(object)args).Cancel = true;
					}
				}
			});
			ServerProjectValidity validity = val2.Validity;
			switch (validity - 1)
			{
			case 0:
				throw new ProjectApiException(ErrorMessages.OpenServerProjectOperation_NotPublished);
			case 1:
				throw new ProjectApiException(ErrorMessages.OpenServerProjectOperation_ProjectDeleted);
			case 2:
				throw new ProjectApiException(ErrorMessages.OpenServerProjectOperation_NoPermissions);
			default:
				if (val2 != null && val2.NewPackageTimeStamp.HasValue)
				{
					packageTimeStamp = val2.NewPackageTimeStamp.Value;
					return true;
				}
				packageTimeStamp = DateTime.MinValue;
				return false;
			}
		}

		public void Cancel()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			if ((int)Status == 3)
			{
				throw new InvalidOperationException(ErrorMessages.OpenServerProject_CannotCancelAlreadyCancelling);
			}
			if ((int)Status != 1)
			{
				throw new InvalidOperationException(ErrorMessages.OpenServerProject_CannotCancelNotInProgress);
			}
			Status = (OpenServerProjectStatus)3;
		}

		private bool CheckCancelRequested()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)Status == 3)
			{
				Status = (OpenServerProjectStatus)4;
				return true;
			}
			return false;
		}

		private void Result_MessageReported(object sender, ExecutionMessageEventArgs e)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			ResultImpl.ReportMessage(e.Message.Source, e.Message.Message, e.Message.Level, e.Message.ExceptionData);
		}

		private void OnProgress()
		{
			this.Progress?.Invoke(this, EventArgs.Empty);
		}

		public string GetServerUserName()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected I4, but got Unknown
			string identityKey = IdentityInfoCache.GetIdentityKey(CommuteClientManager.GetUnqualifiedServerUri(ServerUri));
			UserCredentials userCredentials = IdentityInfoCache.Default.GetUserCredentials(identityKey);
			if (userCredentials == null)
			{
				throw new InvalidOperationException("Credentials not available.");
			}
			string result = "";
			UserManagerTokenType userType = userCredentials.UserType;
			switch ((int)userType)
			{
			case 0:
			case 1:
			case 3:
				result = userCredentials.UserName;
				break;
			case 2:
				result = UserHelper.WindowsUserId;
				break;
			}
			return result;
		}

		public UserManagerTokenType GetServerUserType()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			string identityKey = IdentityInfoCache.GetIdentityKey(CommuteClientManager.GetUnqualifiedServerUri(ServerUri));
			UserCredentials userCredentials = IdentityInfoCache.Default.GetUserCredentials(identityKey);
			if (userCredentials == null)
			{
				throw new InvalidOperationException("Credentials not available.");
			}
			return userCredentials.UserType;
		}

		public void RaiseMessageReported(IExecutionMessage message)
		{
		}
	}
}
