using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SDL.ApiClientSDK.GS.Models;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class PublishProjectOperation : IPublishProjectOperation, IObjectWithExecutionResult
	{
		public const double PackageCreationCompletePercentage = 0.25;

		private readonly Project _project;

		private readonly IProjectPathUtil _projectPathUtil;

		private ExecutionResult lazyResultImpl;

		internal PublishProjectOperationSettings Settings => _project.GetSettingsGroup<PublishProjectOperationSettings>();

		public Uri ServerUri
		{
			get
			{
				if (!string.IsNullOrEmpty(Settings.ServerUri.Value))
				{
					return new Uri(Settings.ServerUri.Value);
				}
				return null;
			}
			set
			{
				Settings.ServerUri.Value = ((value != null) ? value.AbsoluteUri : null);
			}
		}

		public Uri UnqualifiedServerUri
		{
			get
			{
				if (!string.IsNullOrEmpty(Settings.ServerUri.Value))
				{
					return CommuteClientManager.GetUnqualifiedServerUri(new Uri(Settings.ServerUri.Value));
				}
				return null;
			}
		}

		public string OrganizationPath
		{
			get
			{
				return Settings.OrganizationPath.Value;
			}
			set
			{
				Settings.OrganizationPath.Value = value;
			}
		}

		public bool IsExecuting { get; private set; }

		public string OriginalServerUserName => Settings.ServerUserName.Value;

		public UserManagerTokenType OriginalServerUserType => Settings.ServerUserType.Value;

		public string ServerUserName
		{
			get
			{
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_006e: Expected I4, but got Unknown
				if (ServerUri == null)
				{
					return null;
				}
				string identityKey = IdentityInfoCache.GetIdentityKey(UnqualifiedServerUri);
				UserCredentials val = null;
				if (IdentityInfoCache.Default.ContainsKey(identityKey))
				{
					val = IdentityInfoCache.Default.GetUserCredentials(identityKey);
				}
				if (val == null)
				{
					return Settings.ServerUserName.Value;
				}
				string result = "";
				UserManagerTokenType userType = val.UserType;
				switch ((int)userType)
				{
				case 0:
					result = val.UserName.ToLower();
					break;
				case 2:
					result = UserHelper.WindowsUserId;
					break;
				case 1:
					result = val.UserName.ToLower();
					break;
				case 3:
					result = val.UserName.ToLower();
					break;
				}
				return result;
			}
		}

		public UserManagerTokenType ServerUserType
		{
			get
			{
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (ServerUri == null)
				{
					return (UserManagerTokenType)0;
				}
				string identityKey = IdentityInfoCache.GetIdentityKey(UnqualifiedServerUri);
				UserCredentials val = null;
				if (IdentityInfoCache.Default.ContainsKey(identityKey))
				{
					val = IdentityInfoCache.Default.GetUserCredentials(identityKey);
				}
				if (val == null)
				{
					return Settings.ServerUserType.Value;
				}
				return val.UserType;
			}
		}

		public PublicationStatus Status
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Invalid comparison between Unknown and I4
				PublicationStatus value = Settings.PublicationStatus.Value;
				if (!IsExecuting && (int)value == 1)
				{
					return (PublicationStatus)5;
				}
				return value;
			}
			private set
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				Settings.PublicationStatus.Value = value;
			}
		}

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
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Invalid comparison between Unknown and I4
				if ((int)Status != 7 && (int)Status != 5 && (int)Status != 4)
				{
					return (int)Status == 8;
				}
				return true;
			}
		}

		public IExecutionResult Result => (IExecutionResult)(object)ResultImpl;

		private ExecutionResult ResultImpl
		{
			get
			{
				if (lazyResultImpl == null)
				{
					lazyResultImpl = new ExecutionResult(new Sdl.ProjectApi.Implementation.Xml.ExecutionResult(), this);
				}
				return lazyResultImpl;
			}
		}

		public int PercentComplete { get; private set; }

		public string CurrentOperationDescription { get; private set; }

		public event EventHandler<PublicationStatusEventArgs> Progress;

		public PublishProjectOperation(IProject project, IProjectPathUtil projectPathUtil)
		{
			_project = project as Project;
			_projectPathUtil = projectPathUtil;
		}

		public void Execute()
		{
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Invalid comparison between Unknown and I4
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Invalid comparison between Unknown and I4
			try
			{
				IsExecuting = true;
				_project.Check();
				Status = (PublicationStatus)1;
				if (ProjectUsesFileBasedTM((IProject)(object)_project))
				{
					ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, StringResources.PublishProjectOperation_FileBasedTMWarning, (MessageLevel)1);
				}
				if (ProjectUsesFileBasedTermbases((IProject)(object)_project))
				{
					ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, StringResources.PublishProjectOperation_FileBasedTermbaseWarning, (MessageLevel)1);
				}
				if (ProjectUsesLocalAutoSuggestDictionaries((IProject)(object)_project))
				{
					ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, StringResources.PublishProjectOperation_LocalAutoSuggestDictionariesWarning, (MessageLevel)1);
				}
				if (ProjectUsesFileBasedLanguageResourceTemplate((IProject)(object)_project))
				{
					ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, StringResources.PublishProjectOperation_FileBasedLanguageResourceTemplateWarning, (MessageLevel)1);
				}
				StoreLocationIdInProjectSettings();
				StoreServerUserInProjectSettings();
				ValidateProjectName();
				ICommuteClient val = _project.ProjectsProvider.Application.CommuteClientManager.CreateCommuteClient(ServerUri);
				PublishProjectPackageCreation publishProjectPackageCreation = CreatePackage();
				publishProjectPackageCreation.Start();
				PackageStatus status = publishProjectPackageCreation.Status;
				if ((int)status != 4)
				{
					if ((int)status == 7)
					{
						SetCancelled();
						return;
					}
					_project.ProjectsProvider.Save();
					if (!CheckCancelRequested())
					{
						if (val.InitiatePublishProjectPackage(_project.Guid, publishProjectPackageCreation.GetAbsolutePackagePath(), (EventHandler<ProgressEventArgs>)delegate(object sender, ProgressEventArgs args)
						{
							//IL_002a: Unknown result type (might be due to invalid IL or missing references)
							//IL_002f: Unknown result type (might be due to invalid IL or missing references)
							//IL_0035: Expected O, but got Unknown
							//IL_003e: Unknown result type (might be due to invalid IL or missing references)
							//IL_0044: Invalid comparison between Unknown and I4
							CurrentOperationDescription = args.CurrentOperationDescription;
							PercentComplete = args.PercentComplete * 3 / 4 + 25;
							EventHandler<PublicationStatusEventArgs> progress = this.Progress;
							if (progress != null)
							{
								PublicationStatusEventArgs e = new PublicationStatusEventArgs(Status);
								progress(this, e);
								if ((int)Status == 6)
								{
									((CancelEventArgs)(object)args).Cancel = true;
								}
							}
						}))
						{
							Status = (PublicationStatus)2;
						}
						else
						{
							SetCancelled();
						}
					}
				}
				else
				{
					SetFailed();
				}
			}
			catch (InvalidOperationException ex)
			{
				SetFailed();
				ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, ex.Message, (MessageLevel)2, ex);
			}
			catch (Exception exception)
			{
				SetFailed();
				ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, StringResources.PublishProjectOperation_Failed, (MessageLevel)2, exception);
			}
			finally
			{
				_project.Save();
				_project.ProjectsProvider.Save();
				IsExecuting = false;
			}
		}

		private void StoreLocationIdInProjectSettings()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			UserManagerClient val = new UserManagerClient(UnqualifiedServerUri.AbsoluteUri);
			try
			{
				Settings.OrganizationIds.Value = val.GetResourceGroupId(Setting<string>.op_Implicit(Settings.OrganizationPath)).ToString();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		private void StoreServerUserInProjectSettings()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Settings.ServerUserName.Value = ServerUserName;
			Settings.ServerUserType.Value = ServerUserType;
			StoreServerUserInformation();
		}

		internal void StoreServerUserInformation()
		{
			UserDetails serverUser = ServerUserManager.GetServerUser(UnqualifiedServerUri, ServerUserName);
			if (serverUser == null)
			{
				throw new InvalidOperationException($"Cannot find user '{ServerUserName}' on the server.");
			}
			IUser user = serverUser.ToProjectApiUser();
			_project.AddUserToCache(user);
		}

		private void SetFailed()
		{
			CurrentOperationDescription = StringResources.PublishProjectOperation_Failed;
			Status = (PublicationStatus)5;
			OnProgress();
		}

		private void SetCancelled()
		{
			CurrentOperationDescription = StringResources.PublishProjectOperation_Cancelled;
			Status = (PublicationStatus)7;
			OnProgress();
		}

		private static bool ProjectUsesFileBasedTM(IProject project)
		{
			bool flag = ((IProjectConfiguration)project).CascadeItem.CascadeEntryItems.Any((ProjectCascadeEntryItem i) => i.MainTranslationProviderItem.IsFileBasedTranslationMemory() || i.ProjectTranslationProviderItems.Any((ITranslationProviderItem j) => j.IsFileBasedTranslationMemory()));
			bool flag2 = ((IProjectConfiguration)project).LanguageDirections.Any((ILanguageDirection ld) => ld.CascadeItem.OverrideParent && ld.CascadeItem.CascadeEntryItems.Any((ProjectCascadeEntryItem i) => i.MainTranslationProviderItem.IsFileBasedTranslationMemory() || i.ProjectTranslationProviderItems.Any((ITranslationProviderItem j) => true)));
			return flag || flag2;
		}

		private static bool ProjectUsesFileBasedLanguageResourceTemplate(IProject project)
		{
			return ((IProjectConfiguration)project).LanguageResources != null;
		}

		private static bool ProjectUsesFileBasedTermbases(IProject project)
		{
			return ((IEnumerable<IProjectTermbase>)((IProjectConfiguration)project).TermbaseConfiguration.Termbases).Any((IProjectTermbase t) => t.IsLocalTermbase());
		}

		private static bool ProjectUsesLocalAutoSuggestDictionaries(IProject project)
		{
			return ((IProjectConfiguration)project).LanguageDirections.Any((ILanguageDirection ld) => ((IEnumerable<IAutoSuggestDictionary>)ld.AutoSuggestDictionaries).Any((IAutoSuggestDictionary asd) => !asd.FilePath.StartsWith("\\\\")));
		}

		public PublishProjectPackageCreation CreatePackage()
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			PublishProjectPackageCreation packageCreation = new PublishProjectPackageCreation((IProject)(object)_project, _projectPathUtil);
			packageCreation.StatusChanged += (PackageOperationStatusChangedEventHandler)delegate(IPackageOperation packageOperation, PackageStatus status)
			{
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Invalid comparison between Unknown and I4
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Invalid comparison between Unknown and I4
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Invalid comparison between Unknown and I4
				CurrentOperationDescription = packageOperation.CurrentOperationDescription;
				PercentComplete = (int)((double)packageOperation.PercentComplete * 0.25);
				OnProgress();
				if ((int)Status == 6 && (int)packageCreation.Status != 6 && (int)packageCreation.Status != 7)
				{
					packageCreation.Cancel();
				}
			};
			packageCreation.Result.MessageReported += ResultMessageReported;
			return packageCreation;
		}

		private void ResultMessageReported(object sender, ExecutionMessageEventArgs e)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			ResultImpl.ReportMessage(e.Message.Source, e.Message.Message, e.Message.Level, e.Message.Exception);
		}

		public void Refresh()
		{
			if (ServerUri == null)
			{
				throw new InvalidOperationException();
			}
			ICommuteClient val = _project.ProjectsProvider.Application.CommuteClientManager.CreateCommuteClient(ServerUri);
			AsyncServerOperation publishProjectPackageStatus = val.GetPublishProjectPackageStatus(_project.Guid);
			ApplyStatusInformation(publishProjectPackageStatus);
		}

		internal void ApplyStatusInformation(AsyncServerOperation asyncOperation)
		{
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			lock (_project.ProjectsProvider.SyncRoot)
			{
				if (asyncOperation != null)
				{
					PercentComplete = asyncOperation.PercentComplete;
					CurrentOperationDescription = GetStatusString(asyncOperation.Status);
					Status = MapAsyncServerOperationStatusToPublicationStatus(asyncOperation.Status);
					if (!string.IsNullOrEmpty(asyncOperation.ErrorMessage) && ResultImpl.Messages.FirstOrDefault((IExecutionMessage m) => m.Message == asyncOperation.ErrorMessage) == null)
					{
						ResultImpl.ReportMessage(StringResources.PublishProjectOperation_MessageSource, string.Format(StringResources.PublishProjectOperation_ExecutionServerError, asyncOperation.ErrorMessage), (MessageLevel)2);
					}
				}
				else if ((int)Status != 0)
				{
					Status = (PublicationStatus)5;
				}
			}
		}

		internal void MarkAsDeletedFromServer()
		{
			Status = (PublicationStatus)8;
		}

		internal void MarkAsArchived()
		{
			Status = (PublicationStatus)9;
		}

		public void MarkAsUnpublished()
		{
			Status = (PublicationStatus)0;
			Settings.ServerUri.Value = null;
			Settings.ServerUserName.Value = null;
			Settings.OrganizationPath.Value = null;
		}

		internal void MarkAsPublished()
		{
			Status = (PublicationStatus)4;
			CurrentOperationDescription = StringResources.PublishProjectOperation_Status_Completed;
			PercentComplete = 100;
			Settings.PermissionsDenied.Value = false;
		}

		private static PublicationStatus MapAsyncServerOperationStatusToPublicationStatus(AsyncServerOperationStatus asyncStatus)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected I4, but got Unknown
			return (PublicationStatus)((int)asyncStatus switch
			{
				3 => 7, 
				2 => 6, 
				5 => 4, 
				4 => 5, 
				1 => 3, 
				0 => 2, 
				_ => throw new Exception("Unexpected value of AsyncServerOperationStatus"), 
			});
		}

		private static string GetStatusString(AsyncServerOperationStatus status)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected I4, but got Unknown
			return (int)status switch
			{
				3 => StringResources.PublishProjectOperation_Status_Cancelled, 
				5 => StringResources.PublishProjectOperation_Status_Completed, 
				4 => StringResources.PublishProjectOperation_Status_Failed, 
				2 => StringResources.PublishProjectOperation_Status_Cancelling, 
				1 => StringResources.PublishProjectOperation_Status_InProgress, 
				0 => StringResources.PublishProjectOperation_Status_Pending, 
				_ => throw new Exception("Unexpected value of AsyncServerOperationStatus"), 
			};
		}

		public void Rollback()
		{
			Status = (PublicationStatus)0;
			Settings.ServerUri.Value = null;
			Settings.ServerUserName.Value = null;
			Settings.OrganizationPath.Value = null;
		}

		public void Cancel()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Invalid comparison between Unknown and I4
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Invalid comparison between Unknown and I4
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Invalid comparison between Unknown and I4
			PublicationStatus status = Status;
			if ((int)status == 6 || (int)status == 7 || (int)status == 5 || (int)status == 4)
			{
				throw new InvalidOperationException();
			}
			Status = (PublicationStatus)6;
			if ((int)status == 2 || (int)status == 3)
			{
				ICommuteClient val = _project.ProjectsProvider.Application.CommuteClientManager.CreateCommuteClient(ServerUri);
				val.InitiateCancelPublishProjectPackage(_project.Guid);
			}
		}

		private bool CheckCancelRequested()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)Status == 6)
			{
				SetCancelled();
				return true;
			}
			return false;
		}

		private void OnProgress()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			EventHandler<PublicationStatusEventArgs> progress = this.Progress;
			if (progress != null)
			{
				PublicationStatusEventArgs e = new PublicationStatusEventArgs(Status);
				progress(this, e);
			}
		}

		public void RaiseMessageReported(IExecutionMessage message)
		{
		}

		public void MarkAsInaccessible()
		{
			Settings.PermissionsDenied.Value = true;
		}

		public void MarkAsAccessible()
		{
			Settings.PermissionsDenied.Value = false;
		}

		private void ValidateProjectName()
		{
			if (!_project.ExecuteOperation("IsValidProjectNameOperation", new object[3] { _project.Name, UnqualifiedServerUri, OrganizationPath }).IsSuccesful)
			{
				throw new InvalidOperationException(StringResources.PublishProjectOperation_ProjectExists);
			}
		}
	}
}
