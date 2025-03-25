using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.FileTypeSupport.Framework;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class LanguageFile : AbstractProjectItem, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		internal List<IFileRevision> _lazyFileRevisions;

		private readonly IFile _fileWrapper;

		private readonly ProjectFileFactory _projectFileFactory;

		public Guid Guid => XmlLanguageFile.Guid;

		public string FilenameInProject => XmlProjectFile.Path + XmlProjectFile.Name;

		public string Filename => XmlProjectFile.Name;

		public string CurrentFilename
		{
			get
			{
				if (LatestXmlFileVersion != null)
				{
					return LatestXmlFileVersion.FileName;
				}
				if (!IsSource && SourceLanguageFile != null)
				{
					return SourceLanguageFile.CurrentFilename;
				}
				return Filename;
			}
		}

		public Guid ProjectFileGuid => XmlProjectFile.Guid;

		public string PathInProject => XmlProjectFile.Path;

		public DateTime LastChanged
		{
			get
			{
				if (LatestXmlFileVersion == null)
				{
					return DateTime.MinValue;
				}
				return GetLastChangeTime();
			}
		}

		public long Size
		{
			get
			{
				if (LatestXmlFileVersion == null)
				{
					return 0L;
				}
				return LatestXmlFileVersion.Size;
			}
		}

		public virtual string LocalFilePath
		{
			get
			{
				return LatestFileVersionPath;
			}
			set
			{
				if (HasLatestXmlVersion)
				{
					SetFileVersionPath(LatestXmlFileVersion, value, copy: false);
				}
			}
		}

		public FileRole FileRole
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return EnumConvert.ConvertFileRole(XmlProjectFile.Role);
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				XmlProjectFile.Role = EnumConvert.ConvertFileRole(value);
				if ((int)value != 1)
				{
					XmlProjectFile.FilterDefinitionId = null;
				}
			}
		}

		public IFileTypeDefinition FilterDefinition
		{
			get
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				if (!string.IsNullOrEmpty(FileTypeDefinitionId))
				{
					return ((IProjectConfiguration)base.Project).FileTypeConfiguration.FilterManager.FindFileTypeDefinition(new FileTypeDefinitionId(FileTypeDefinitionId));
				}
				return null;
			}
			set
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				ProjectFile xmlProjectFile = XmlProjectFile;
				FileTypeDefinitionId fileTypeDefinitionId = value.FileTypeInformation.FileTypeDefinitionId;
				xmlProjectFile.FilterDefinitionId = ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id;
			}
		}

		public string FileTypeDefinitionId
		{
			get
			{
				if (!string.IsNullOrEmpty(XmlProjectFile.FilterDefinitionId))
				{
					return XmlProjectFile.FilterDefinitionId;
				}
				return null;
			}
			set
			{
				XmlProjectFile.FilterDefinitionId = value;
			}
		}

		public ITaskFile[] TaskFileHistory => base.Project.ProjectTaskFileManager.GetTaskFileHistory(Guid).ToArray();

		public Language Language
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				return new Language(XmlLanguageFile.LanguageCode);
			}
			set
			{
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				if (LanguageBase.IsNullOrInvalid((LanguageBase)(object)value))
				{
					throw new ArgumentNullException("value");
				}
				if (!((object)Language).Equals((object)value))
				{
					if (XmlProjectFile.GetLanguageFileByLanguage(((LanguageBase)value).IsoAbbreviation) != null)
					{
						throw new ProjectApiException("Cannot change the language of this file to the specifie dlanguage, because there is already a language file with that language.");
					}
					XmlLanguageFile.LanguageCode = ((LanguageBase)value).IsoAbbreviation;
				}
			}
		}

		public ISettingsBundle Settings
		{
			get
			{
				ISettingsBundle val = (IsSource ? ((IObjectWithSettings)base.Project).Settings : ((IObjectWithSettings)base.Project.GetLanguageDirection(Language)).Settings);
				if (XmlLanguageFile.SettingsBundleGuid == Guid.Empty)
				{
					XmlLanguageFile.SettingsBundleGuid = Guid.NewGuid();
					base.ProjectImpl.SettingsBundlesList.AddSettingsBundle(XmlLanguageFile.SettingsBundleGuid, SettingsUtil.CreateSettingsBundle(val));
				}
				return base.ProjectImpl.SettingsBundlesList.GetSettingsBundle(XmlLanguageFile.SettingsBundleGuid, val);
			}
			set
			{
				if (XmlLanguageFile.SettingsBundleGuid == Guid.Empty)
				{
					XmlLanguageFile.SettingsBundleGuid = Guid.NewGuid();
				}
				base.ProjectImpl.SettingsBundlesList.ImportSettingsBundle(XmlLanguageFile.SettingsBundleGuid, value);
				base.ProjectImpl.SettingsBundlesList.SaveAndClearCache();
			}
		}

		public bool IsLatestVersionAutoUploaded
		{
			get
			{
				LanguageFileServerStateSettings serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				FileVersion fileVersion = XmlLanguageFile.FileVersions.Single((FileVersion f) => f.VersionNumber == Setting<int>.op_Implicit(serverStateSettings.LatestServerVersionNumber));
				return fileVersion.IsAutoUpload;
			}
		}

		public string FolderInProject => XmlProjectFile.Path;

		public ILanguageDirection LanguageDirection
		{
			get
			{
				if (!IsSource)
				{
					return base.Project.GetLanguageDirection(Language);
				}
				return null;
			}
		}

		public ILanguageFile SourceLanguageFile
		{
			get
			{
				if (((ILanguageFile)this).IsSource)
				{
					return null;
				}
				return base.ProjectImpl.GetSourceLanguageFile((ILanguageFile)(object)this);
			}
		}

		public bool HasTargetLanguageFiles
		{
			get
			{
				if (!IsSource)
				{
					return false;
				}
				string isoAbbreviation = ((LanguageBase)base.Project.SourceLanguage).IsoAbbreviation;
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in XmlProjectFile.LanguageFiles)
				{
					if (!LanguageBase.Equals(languageFile.LanguageCode, isoAbbreviation))
					{
						return true;
					}
				}
				return false;
			}
		}

		public ILanguageFile[] TargetLanguageFiles
		{
			get
			{
				if (!((ILanguageFile)this).IsSource)
				{
					return null;
				}
				ILanguageFile[] array = (ILanguageFile[])(object)new ILanguageFile[XmlProjectFile.LanguageFiles.Count - 1];
				int num = 0;
				string isoAbbreviation = ((LanguageBase)base.Project.SourceLanguage).IsoAbbreviation;
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in XmlProjectFile.LanguageFiles)
				{
					if (!LanguageBase.Equals(languageFile.LanguageCode, isoAbbreviation))
					{
						array[num++] = _projectFileFactory.CreateLanguageFile((IProject)(object)base.ProjectImpl, XmlProjectFile, languageFile);
					}
				}
				return array;
			}
		}

		public IFileRevision LatestRevision
		{
			get
			{
				if (RevisionsList.Count <= 0)
				{
					return null;
				}
				return RevisionsList[RevisionsList.Count - 1];
			}
		}

		public IFileRevision LatestServerRevision
		{
			get
			{
				LanguageFileServerStateSettings serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				if (!serverStateSettings.LatestServerVersionNumber.Inherited)
				{
					return RevisionsList.FirstOrDefault((IFileRevision v) => v.RevisionNumber == serverStateSettings.LatestServerVersionNumber.Value);
				}
				return null;
			}
		}

		public IFileRevision CurrentServerRevision
		{
			get
			{
				LanguageFileServerStateSettings serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				string currentFileName = Path.GetFileName(LocalFilePath);
				if (!serverStateSettings.GetCurrentServerVersionNumber(currentFileName).Inherited)
				{
					return RevisionsList.FirstOrDefault((IFileRevision v) => v.RevisionNumber == serverStateSettings.GetCurrentServerVersionNumber(currentFileName).Value);
				}
				return null;
			}
		}

		public IFileRevision[] Revisions => RevisionsList.ToArray();

		private List<IFileRevision> RevisionsList
		{
			get
			{
				if (_lazyFileRevisions != null && _lazyFileRevisions.Count != XmlLanguageFile.FileVersions.Count)
				{
					_lazyFileRevisions = null;
				}
				if (_lazyFileRevisions == null)
				{
					_lazyFileRevisions = new List<IFileRevision>();
					foreach (FileVersion fileVersion in XmlLanguageFile.FileVersions)
					{
						_lazyFileRevisions.Add((IFileRevision)(object)new FileRevision(this, fileVersion));
					}
				}
				return _lazyFileRevisions;
			}
		}

		public IManualTaskFile LastTaskFileAssignedByMe => base.Project.ProjectTaskFileManager.GetLastTaskFileAssignedByMe(Guid, base.Project.CurrentUser);

		public ITaskFile CurrentTaskFile => base.Project.ProjectTaskFileManager.GetCurrentTaskFile(Guid);

		public IManualTaskFile LastTaskFileAssignedToMe => base.Project.ProjectTaskFileManager.GetLastTaskFileAssignedToMe(Guid, base.Project.CurrentUser);

		public IManualTaskFile LastLocalTaskFileAssignedToMe => base.Project.ProjectTaskFileManager.GetLastLocalTaskFileAssignedToMe(Guid, base.Project.CurrentUser);

		public Language[] SpecificTargetLanguages
		{
			get
			{
				if (!IsSource)
				{
					return null;
				}
				if (XmlProjectFile.SpecificTargetLanguages.Count > 0)
				{
					return XmlProjectFile.SpecificTargetLanguages.ConvertAll((Converter<string, Language>)((string languageCode) => new Language(languageCode))).ToArray();
				}
				return null;
			}
			set
			{
				if (!IsSource)
				{
					throw new InvalidOperationException("The property is only valid for a source file.");
				}
				XmlProjectFile.SpecificTargetLanguages.Clear();
				if (value != null)
				{
					foreach (Language val in value)
					{
						XmlProjectFile.SpecificTargetLanguages.Add(((LanguageBase)val).IsoAbbreviation);
					}
				}
			}
		}

		internal string LanguageFileDirectory => Path.Combine(base.Project.GetProjectLanguageDirectory(Language), XmlProjectFile.Path);

		internal ProjectFile XmlProjectFile { get; }

		internal Sdl.ProjectApi.Implementation.Xml.LanguageFile XmlLanguageFile { get; }

		internal bool HasLatestXmlVersion => XmlLanguageFile.FileVersions.Count > 0;

		internal FileVersion LatestXmlFileVersion
		{
			get
			{
				if (XmlLanguageFile.FileVersions.Count <= 0)
				{
					return null;
				}
				return XmlLanguageFile.FileVersions[XmlLanguageFile.FileVersions.Count - 1];
			}
		}

		internal string LatestFileVersionPath
		{
			get
			{
				if (LatestXmlFileVersion != null)
				{
					return GetFileVersionPath(LatestXmlFileVersion);
				}
				return null;
			}
		}

		public bool IsSource => string.Equals(((LanguageBase)base.Project.SourceLanguage).IsoAbbreviation, XmlLanguageFile.LanguageCode, StringComparison.InvariantCultureIgnoreCase);

		public LocalFileState LocalFileState
		{
			get
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Invalid comparison between Unknown and I4
				PublishProjectOperationSettings settingsGroup = Settings.GetSettingsGroup<PublishProjectOperationSettings>();
				if ((int)Setting<PublicationStatus>.op_Implicit(settingsGroup.PublicationStatus) == 4)
				{
					if (File.Exists(LocalFilePath))
					{
						LanguageFileServerStateSettings serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
						string fileName = Path.GetFileName(LocalFilePath);
						long num = Setting<long>.op_Implicit(serverStateSettings.GetCurrentServerVersionTimestamp(fileName));
						long num2 = Setting<long>.op_Implicit(serverStateSettings.LatestServerVersionTimestamp);
						long localTimestamp = GetLocalTimestamp(LocalFilePath);
						if (num2 != 0L)
						{
							if (num != 0L)
							{
								if (num == num2)
								{
									if (num == localTimestamp)
									{
										if (serverStateSettings.LatestServerVersionNumber.Value == serverStateSettings.GetCurrentServerVersionNumber(fileName).Value)
										{
											return (LocalFileState)2;
										}
										return (LocalFileState)3;
									}
									return (LocalFileState)4;
								}
								if (num != num2)
								{
									string fileName2 = XmlLanguageFile.FileVersions.FirstOrDefault((FileVersion fv) => fv.VersionNumber == serverStateSettings.LatestServerVersionNumber.Value).FileName;
									if (num != localTimestamp)
									{
										if (!(fileName2 != fileName))
										{
											return (LocalFileState)5;
										}
										return (LocalFileState)4;
									}
									return (LocalFileState)3;
								}
								return (LocalFileState)0;
							}
							return (LocalFileState)4;
						}
						return (LocalFileState)6;
					}
					return (LocalFileState)1;
				}
				return (LocalFileState)0;
			}
		}

		public DateTime? CheckedOutAt
		{
			get
			{
				LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				if (!settingsGroup.CheckedOutAt.Inherited)
				{
					return settingsGroup.CheckedOutAt.Value.ToLocalTime();
				}
				return null;
			}
		}

		public IUser CheckedOutTo
		{
			get
			{
				string checkedOutToUserId = CheckedOutToUserId;
				if (!string.IsNullOrEmpty(checkedOutToUserId))
				{
					return base.ProjectImpl.GetUserById(checkedOutToUserId);
				}
				return null;
			}
		}

		public bool IsCheckedOutOnline
		{
			get
			{
				LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				return Setting<bool>.op_Implicit(settingsGroup.IsCheckedOutOnline);
			}
		}

		private string CheckedOutToUserId
		{
			get
			{
				LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				return Setting<string>.op_Implicit(settingsGroup.CheckedOutTo);
			}
		}

		public bool CheckedOutToMe
		{
			get
			{
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Invalid comparison between Unknown and I4
				if (!base.Project.IsPublished)
				{
					return false;
				}
				Uri unqualifiedServerUri = base.Project.PublishProjectOperation.UnqualifiedServerUri;
				if (IdentityInfoCache.Default.ContainsKey(unqualifiedServerUri.AbsoluteUri))
				{
					ConnectionInfo connectionInfo = IdentityInfoCache.Default.GetConnectionInfo(unqualifiedServerUri.AbsoluteUri);
					if (connectionInfo != null && connectionInfo.Credentials != null)
					{
						return string.Equals(CheckedOutToUserId, ((int)connectionInfo.Credentials.UserType == 2) ? UserHelper.WindowsUserId : connectionInfo.Credentials.UserName, StringComparison.OrdinalIgnoreCase);
					}
				}
				return string.Equals(CheckedOutToUserId, base.Project.PublishProjectOperation.OriginalServerUserName, StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool CheckedOut
		{
			get
			{
				if (string.IsNullOrEmpty(CheckedOutToUserId))
				{
					return IsCheckedOutOnline;
				}
				return true;
			}
		}

		public bool CheckedOutToAnotherUser
		{
			get
			{
				if (CheckedOut)
				{
					return !CheckedOutToMe;
				}
				return false;
			}
		}

		public List<IFileAssignmentInfo> FileAssignments
		{
			get
			{
				List<IFileAssignmentInfo> list = new List<IFileAssignmentInfo>();
				LanguageFileServerPhasesSettings settingsGroup = ((IObjectWithSettings)base.Project).Settings.GetSettingsGroup<LanguageFileServerPhasesSettings>();
				ISettingsBundle settingsBundle = base.ProjectImpl.SettingsBundlesList.GetSettingsBundle(XmlLanguageFile.SettingsBundleGuid, ((IObjectWithSettings)base.Project).Settings);
				if (settingsGroup.Phases.Value != null)
				{
					foreach (string item in settingsGroup.Phases.Value)
					{
						LanguageFileServerAssignmentsSettings settingsGroup2 = settingsBundle.GetSettingsGroup<LanguageFileServerAssignmentsSettings>("LanguageFileServerAssignmentsSettings_" + item);
						DateTime value = settingsGroup2.AssignedAt.Value;
						string value2 = settingsGroup2.AssignedBy.Value;
						List<string> value3 = settingsGroup2.Assignees.Value;
						DateTime? dateTime = settingsGroup2.DueDate.Value;
						if (dateTime == DateTime.MinValue)
						{
							dateTime = null;
						}
						if (value != DateTime.MinValue && !string.IsNullOrEmpty(value2))
						{
							list.Add((IFileAssignmentInfo)(object)new FileAssignmentInfo
							{
								PhaseName = item,
								AssignedAt = value,
								AssignedBy = ((!string.IsNullOrEmpty(value2)) ? GetUser(Setting<string>.op_Implicit(settingsGroup2.AssignedBy)) : null),
								IsCurrentAssignment = Setting<bool>.op_Implicit(settingsGroup2.IsCurrentAssignment),
								Assignees = ((value3 != null) ? new List<IUser>(value3.Distinct().Select(GetUser)) : new List<IUser>()),
								DueDate = (dateTime.HasValue ? new DateTime?(dateTime.Value.ToLocalTime()) : dateTime)
							});
						}
					}
				}
				return list;
			}
		}

		protected IProjectPathUtil ProjectPathUtil { get; }

		public LanguageFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: this(project, xmlProjectFile, xmlLanguageFile, (IFile)new FileWrapper(), projectPathUtil)
		{
		}//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown


		public LanguageFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IFile fileWrapper, IProjectPathUtil projectPathUtil)
			: base(project)
		{
			XmlProjectFile = xmlProjectFile;
			XmlLanguageFile = xmlLanguageFile;
			_fileWrapper = fileWrapper;
			ProjectPathUtil = projectPathUtil;
			_projectFileFactory = new ProjectFileFactory(ProjectPathUtil);
		}

		public string DownloadLatestVersion(string downloadRootFolder)
		{
			if (!HasLatestXmlVersion)
			{
				return null;
			}
			return DownloadFileVersion(LatestXmlFileVersion, downloadRootFolder);
		}

		public Stream GetLatestVersionDownloadStream()
		{
			if (!HasLatestXmlVersion)
			{
				return null;
			}
			return _fileWrapper.OpenRead(GetFileVersionPath(LatestXmlFileVersion));
		}

		public bool DownloadLatestServerVersion(EventHandler<DataTransferEventArgs> progressEventHandler, bool force)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			if (!force && (int)LocalFileState == 2)
			{
				return true;
			}
			RemoveLocalVersions();
			ICommuteClient val = base.ProjectImpl.CreateCommuteClient();
			LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			bool downloadCancelled = false;
			FileInfo fileInfo = new FileInfo(LocalFilePath);
			if (!fileInfo.Directory.Exists)
			{
				fileInfo.Directory.Create();
			}
			val.GetFileVersion(base.Project.Guid, LocalFilePath, Guid, settingsGroup.LatestServerVersionNumber.Value, (EventHandler<FileTransferEventArgs>)delegate(object sender, FileTransferEventArgs args)
			{
				if (progressEventHandler != null)
				{
					DataTransferEventArgs val2 = args.ToDataTransferEventArgs();
					progressEventHandler(this, val2);
					if (((CancelEventArgs)(object)val2).Cancel)
					{
						downloadCancelled = true;
						((CancelEventArgs)(object)args).Cancel = true;
					}
				}
			});
			if (!downloadCancelled)
			{
				string fileName = Path.GetFileName(LocalFilePath);
				settingsGroup.GetCurrentServerVersionNumber(fileName).Value = settingsGroup.LatestServerVersionNumber.Value;
				settingsGroup.GetCurrentServerVersionTimestamp(fileName).Value = settingsGroup.LatestServerVersionTimestamp.Value;
				FileInfo fileInfo2 = new FileInfo(LocalFilePath)
				{
					LastWriteTimeUtc = new DateTime(settingsGroup.LatestServerVersionTimestamp.Value, DateTimeKind.Utc)
				};
				return true;
			}
			return false;
		}

		public void UpdateFileSize(long size)
		{
			LatestXmlFileVersion.Size = size;
			((IProjectConfiguration)base.Project).Save();
		}

		public void ForceKeepLocalChanges()
		{
			LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			string fileName = Path.GetFileName(LatestServerRevision.LocalFilePath);
			settingsGroup.GetCurrentServerVersionNumber(fileName).Value = settingsGroup.LatestServerVersionNumber.Value;
			settingsGroup.GetCurrentServerVersionTimestamp(fileName).Value = settingsGroup.LatestServerVersionTimestamp.Value;
		}

		public LanguageFileInfo CreateLanguageFileInfoForCheckin(string comment, bool isAutoUpload)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			LanguageFileInfo val = new LanguageFileInfo
			{
				Action = (LanguageFileAction)(((int)LocalFileState != 6) ? 1 : 0),
				LanguageFileId = Guid,
				LanguageCode = ((LanguageBase)Language).IsoAbbreviation
			};
			IMergedTranslatableFile val2 = (IMergedTranslatableFile)(object)((this is IMergedTranslatableFile) ? this : null);
			if (val2 != null)
			{
				val.MergeState = val2.MergeState.ToCommuteMergeState();
				val.ChildLanguageFileIds = val2.ChildFiles.Select((ITranslatableFile cf) => ((IProjectFile)cf).Guid).ToArray();
			}
			ITranslatableFile val3 = (ITranslatableFile)(object)((this is ITranslatableFile) ? this : null);
			if (val3 != null)
			{
				val.AnalysisStatistics = val3.AnalysisStatistics.ToCommuteAnalysisStatistics();
			}
			if ((int)val.Action == 0)
			{
				int nextVersionNumber = ((IsSource || SourceLanguageFile == null) ? 1 : (SourceLanguageFile.LatestRevision.RevisionNumber + 1));
				List<FileVersionInfo> list = CreateFileVersionInfos(0, nextVersionNumber, comment);
				val.NewFileVersions = list.ToArray();
			}
			else
			{
				LanguageFileServerStateSettings serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				int latestServerFileVersionIndex = XmlLanguageFile.FileVersions.FindIndex((FileVersion fv) => fv.VersionNumber == Setting<int>.op_Implicit(serverStateSettings.LatestServerVersionNumber));
				List<FileVersionInfo> list2 = CreateNewFileVersionInfosForExistingFile(comment, latestServerFileVersionIndex, Setting<int>.op_Implicit(serverStateSettings.LatestServerVersionNumber) + 1, isAutoUpload);
				val.NewFileVersions = list2.ToArray();
			}
			return val;
		}

		private List<FileVersionInfo> CreateNewFileVersionInfosForExistingFile(string comment, int latestServerFileVersionIndex, int nextVersionNumber, bool isAutoUpload)
		{
			List<FileVersionInfo> list = new List<FileVersionInfo>();
			FileVersion latestServerFileVersion = XmlLanguageFile.FileVersions[latestServerFileVersionIndex];
			bool flag = false;
			int i;
			for (i = latestServerFileVersionIndex; i + 1 < XmlLanguageFile.FileVersions.Count && latestServerFileVersion.FileName.Equals(XmlLanguageFile.FileVersions[i + 1].FileName, StringComparison.OrdinalIgnoreCase); i++)
			{
				flag = true;
			}
			FileVersion fileVersion = null;
			if (flag)
			{
				fileVersion = XmlLanguageFile.FileVersions[i];
			}
			else if ((ToSqlDateTimeUtc(latestServerFileVersion.FileTimeStamp) != GetSqlLastWriteTimeUtc(GetFileVersionPath(latestServerFileVersion)) || latestServerFileVersion.IsAutoUpload) && (latestServerFileVersionIndex == XmlLanguageFile.FileVersions.Count - 1 || XmlLanguageFile.FileVersions.FindIndex(latestServerFileVersionIndex + 1, (FileVersion fv) => fv.FileName == latestServerFileVersion.FileName) == -1))
			{
				fileVersion = latestServerFileVersion;
			}
			if (fileVersion != null)
			{
				list.Add(CreateFileVersionInfo(fileVersion.IsAutoUpload ? fileVersion.Guid : Guid.NewGuid(), GetFileVersionPath(fileVersion), fileVersion.IsAutoUpload ? fileVersion.VersionNumber : nextVersionNumber++, isAutoUpload ? fileVersion.Comment : comment));
			}
			list.AddRange(CreateFileVersionInfos(i + 1, nextVersionNumber, comment));
			return list;
		}

		private List<FileVersionInfo> CreateFileVersionInfos(int startFileVersionIndex, int nextVersionNumber, string comment)
		{
			List<FileVersionInfo> list = new List<FileVersionInfo>();
			while (startFileVersionIndex < XmlLanguageFile.FileVersions.Count)
			{
				string fileName = XmlLanguageFile.FileVersions[startFileVersionIndex].FileName;
				while (startFileVersionIndex + 1 < XmlLanguageFile.FileVersions.Count && XmlLanguageFile.FileVersions[startFileVersionIndex + 1].FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
				{
					startFileVersionIndex++;
				}
				list.Add(CreateFileVersionInfo(Guid.NewGuid(), GetFileVersionPath(XmlLanguageFile.FileVersions[startFileVersionIndex]), nextVersionNumber++, comment));
				startFileVersionIndex++;
			}
			return list;
		}

		private FileVersionInfo CreateFileVersionInfo(Guid fileId, string localFilePath, int versionNumber, string comment)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			FileVersionInfo val = new FileVersionInfo
			{
				FileId = fileId,
				TimeStamp = GetLocalTimestamp(localFilePath),
				VersionNumber = versionNumber,
				FileName = Path.GetFileName(localFilePath),
				Size = new FileInfo(localFilePath).Length,
				LocalFilePath = localFilePath,
				Comment = comment
			};
			ITranslatableFile val2 = (ITranslatableFile)(object)((this is ITranslatableFile) ? this : null);
			if (val2 != null && FileRevision.IsBilingualFileVersion(base.Project, localFilePath))
			{
				IConfirmationStatistics confirmationStatistics = val2.ConfirmationStatistics;
				if (confirmationStatistics != null)
				{
					val.ConfirmationStatistics = confirmationStatistics.ToCommuteConfirmationStatistics();
				}
			}
			return val;
		}

		public void UpdateAfterCheckin(LanguageFileInfo languageFileInfo, LanguageFileServerStateSettings serverStateSettings = null, bool isAutoUpload = false)
		{
			if (serverStateSettings == null)
			{
				serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			}
			FileVersionInfo lastFileVersion = languageFileInfo.GetLastFileVersion();
			if (lastFileVersion != null)
			{
				if (!serverStateSettings.LatestServerVersionNumber.Inherited)
				{
					RemoveLocalVersions(serverStateSettings);
				}
				else
				{
					XmlLanguageFile.FileVersions.Clear();
				}
				FileVersionInfo[] newFileVersions = languageFileInfo.NewFileVersions;
				foreach (FileVersionInfo fileVersionInfo in newFileVersions)
				{
					FileVersion fileVersion = XmlLanguageFile.FileVersions.SingleOrDefault((FileVersion x) => x.VersionNumber == fileVersionInfo.VersionNumber);
					if (fileVersion != null)
					{
						fileVersion.FileTimeStamp = new DateTime(fileVersionInfo.TimeStamp, DateTimeKind.Utc);
						fileVersion.Size = fileVersionInfo.Size;
						fileVersion.Comment = fileVersionInfo.Comment;
						fileVersion.IsAutoUpload = isAutoUpload;
						continue;
					}
					string text = Path.Combine(LanguageFileDirectory, fileVersionInfo.FileName);
					FileVersion item = new FileVersion
					{
						Comment = fileVersionInfo.Comment,
						CreatedAt = DateTime.UtcNow,
						CreatedBy = base.Project.PublishProjectOperation.OriginalServerUserName,
						FileName = fileVersionInfo.FileName,
						FileTimeStamp = new DateTime(fileVersionInfo.TimeStamp, DateTimeKind.Utc),
						Guid = fileVersionInfo.FileId,
						PhysicalPath = ProjectPathUtil.MakeRelativePath(base.Project, text, true),
						Size = fileVersionInfo.Size,
						VersionNumber = fileVersionInfo.VersionNumber,
						IsAutoUpload = isAutoUpload
					};
					XmlLanguageFile.FileVersions.Add(item);
				}
				_lazyFileRevisions = null;
				serverStateSettings.LatestServerVersionTimestamp.Value = lastFileVersion.TimeStamp;
				serverStateSettings.LatestServerVersionNumber.Value = lastFileVersion.VersionNumber;
			}
			Dictionary<string, FileVersionInfo> lastFileVersionPerFileName = languageFileInfo.GetLastFileVersionPerFileName();
			foreach (KeyValuePair<string, FileVersionInfo> item2 in lastFileVersionPerFileName)
			{
				serverStateSettings.GetCurrentServerVersionTimestamp(item2.Key).Value = item2.Value.TimeStamp;
				serverStateSettings.GetCurrentServerVersionNumber(item2.Key).Value = item2.Value.VersionNumber;
			}
		}

		public void UpdateAfterRollBack()
		{
			XmlLanguageFile.FileVersions.Remove(LatestXmlFileVersion);
			LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			settingsGroup.LatestServerVersionTimestamp.Value = LatestXmlFileVersion.FileTimeStamp.Ticks;
			settingsGroup.LatestServerVersionNumber.Value = LatestXmlFileVersion.VersionNumber;
			settingsGroup.GetCurrentServerVersionTimestamp(LatestXmlFileVersion.FileName).Value = LatestXmlFileVersion.FileTimeStamp.Ticks;
			settingsGroup.GetCurrentServerVersionNumber(LatestXmlFileVersion.FileName).Value = LatestXmlFileVersion.VersionNumber;
		}

		private void RemoveLocalVersions(LanguageFileServerStateSettings serverStateSettings = null)
		{
			if (serverStateSettings == null)
			{
				serverStateSettings = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			}
			if (serverStateSettings.LatestServerVersionNumber.Inherited)
			{
				throw new InvalidOperationException("This method is invalid for local files.");
			}
			while (XmlLanguageFile.FileVersions.Count > 0 && XmlLanguageFile.FileVersions[XmlLanguageFile.FileVersions.Count - 1].VersionNumber > serverStateSettings.LatestServerVersionNumber.Value)
			{
				XmlLanguageFile.FileVersions.RemoveAt(XmlLanguageFile.FileVersions.Count - 1);
			}
		}

		internal void ApplyServerTimestamps(Action<string> messageLogger)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < XmlLanguageFile.FileVersions.Count; i++)
			{
				FileVersion fileVersion = XmlLanguageFile.FileVersions[i];
				dictionary[fileVersion.FileName] = i;
			}
			for (int j = 0; j < XmlLanguageFile.FileVersions.Count; j++)
			{
				FileVersion fileVersion2 = XmlLanguageFile.FileVersions[j];
				string fileVersionPath = GetFileVersionPath(fileVersion2);
				if (File.Exists(fileVersionPath))
				{
					AdjustFileVersionTimestamp(fileVersion2, fileVersionPath);
					LanguageFileServerStateSettings settingsGroup = Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
					if (j == dictionary[fileVersion2.FileName])
					{
						settingsGroup.GetCurrentServerVersionTimestamp(fileVersion2.FileName).Value = fileVersion2.FileTimeStamp.Ticks;
						settingsGroup.GetCurrentServerVersionNumber(fileVersion2.FileName).Value = fileVersion2.VersionNumber;
					}
					if (j == XmlLanguageFile.FileVersions.Count - 1)
					{
						settingsGroup.LatestServerVersionTimestamp.Value = fileVersion2.FileTimeStamp.Ticks;
						settingsGroup.LatestServerVersionNumber.Value = fileVersion2.VersionNumber;
					}
				}
				else
				{
					messageLogger(string.Format(StringResources.PublishProjectPackageCreation_FileXNotFound, fileVersionPath));
				}
			}
		}

		private static void AdjustFileVersionTimestamp(FileVersion xmlFileVersion, string filePath)
		{
			DateTime sqlLastWriteTimeUtc = GetSqlLastWriteTimeUtc(filePath);
			xmlFileVersion.FileTimeStamp = sqlLastWriteTimeUtc;
		}

		private static DateTime GetSqlLastWriteTimeUtc(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("File not found", filePath);
			}
			DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(filePath);
			return ToSqlDateTimeUtc(lastWriteTimeUtc);
		}

		private static DateTime ToSqlDateTimeUtc(DateTime dateTime)
		{
			return new DateTime(new SqlDateTime(dateTime).Value.Ticks, DateTimeKind.Utc);
		}

		private static long GetLocalTimestamp(string localFilePath)
		{
			return GetSqlLastWriteTimeUtc(localFilePath).Ticks;
		}

		public void RemoveFromProject(bool deletePhysicalFiles = true)
		{
			if (IsSource)
			{
				ILanguageFile[] targetLanguageFiles = TargetLanguageFiles;
				ILanguageFile[] array = targetLanguageFiles;
				for (int i = 0; i < array.Length; i++)
				{
					LanguageFile languageFile = (LanguageFile)(object)array[i];
					languageFile.RemoveFromProject(deletePhysicalFiles);
				}
			}
			if (deletePhysicalFiles)
			{
				foreach (FileVersion fileVersion in XmlLanguageFile.FileVersions)
				{
					string fileVersionPath = GetFileVersionPath(fileVersion);
					if (File.Exists(fileVersionPath))
					{
						File.Delete(fileVersionPath);
					}
				}
			}
			base.Project.RemoveLanguageFile((ILanguageFile)(object)this);
		}

		public virtual void UploadNewVersion(Stream inputStream, DateTime lastChangeTimestamp, string comment)
		{
			UploadNewVersion(inputStream, null, lastChangeTimestamp, comment);
		}

		public void RollbackToVersion(IFileRevision fileRevision)
		{
			if (((IProjectFile)fileRevision.CurrentFile).Guid != Guid)
			{
				throw new ArgumentException("The specified revision does not belong to this file.");
			}
			int num = RevisionsList.FindIndex((IFileRevision fr) => fr.RevisionNumber == fileRevision.RevisionNumber);
			if (num == -1)
			{
				throw new ArgumentException($"Revision number {fileRevision.RevisionNumber} could not be found.");
			}
			if (num == XmlLanguageFile.FileVersions.Count - 1)
			{
				throw new ArgumentException("The specified revision is the last revision.");
			}
			XmlLanguageFile.FileVersions.RemoveRange(num + 1, XmlLanguageFile.FileVersions.Count - num - 1);
			RevisionsList.RemoveRange(num + 1, RevisionsList.Count - num - 1);
		}

		public virtual void UploadNewVersion(string filePath, string comment)
		{
			string fileName = Path.GetFileName(filePath);
			string targetFilePath = Path.Combine(LanguageFileDirectory, fileName);
			Util.CopyFile(filePath, targetFilePath);
			FileInfo fileInfo = new FileInfo(filePath);
			FileVersion xmlFileRevision = AddXmlFileVersion(fileName, comment, fileInfo.LastWriteTimeUtc, fileInfo.Length);
			RevisionsList.Add((IFileRevision)(object)new FileRevision(this, xmlFileRevision));
		}

		public virtual void UploadNewVersion(Stream inputStream, string fileName, DateTime lastChangeTimestamp, string comment, bool updateRevisions = true)
		{
			FileVersion latestXmlFileVersion = LatestXmlFileVersion;
			fileName = fileName ?? latestXmlFileVersion.FileName;
			string targetFilePath = Path.Combine(LanguageFileDirectory, fileName);
			long size = Util.WriteStream(inputStream, targetFilePath, lastChangeTimestamp.ToUniversalTime());
			if (updateRevisions)
			{
				FileVersion xmlFileRevision = AddXmlFileVersion(fileName, comment, lastChangeTimestamp.ToUniversalTime(), size);
				RevisionsList.Add((IFileRevision)(object)new FileRevision(this, xmlFileRevision));
			}
		}

		protected virtual FileVersion AddXmlFileVersion(string fileName, string comment, DateTime fileTimeStampUtc, long size)
		{
			FileVersion fileVersion = new FileVersion();
			fileVersion.AssignNewGuid();
			fileVersion.VersionNumber = ((!HasLatestXmlVersion) ? 1 : (LatestXmlFileVersion.VersionNumber + 1));
			fileVersion.Size = size;
			fileVersion.FileName = fileName;
			string text = Path.Combine(LanguageFileDirectory, fileName);
			fileVersion.PhysicalPath = ProjectPathUtil.MakeRelativePath(base.Project, text, true);
			fileVersion.CreatedAt = DateTime.UtcNow;
			fileVersion.CreatedBy = base.Project.CurrentUser.UserId;
			((IProjectConfiguration)base.Project).AddUserToCache(base.Project.CurrentUser);
			fileVersion.Comment = comment;
			fileVersion.FileTimeStamp = fileTimeStampUtc;
			XmlLanguageFile.FileVersions.Add(fileVersion);
			return fileVersion;
		}

		internal string GetFileVersionPath(FileVersion xmlFileVersion)
		{
			return ProjectPathUtil.MakeAbsolutePath(base.Project, xmlFileVersion.PhysicalPath, true);
		}

		internal void OnInPlaceProjectFilePathChanged(string newProjectFilePath, IProjectPathUtil pathUtil)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in XmlProjectFile.LanguageFiles)
			{
				foreach (FileVersion fileVersion in languageFile.FileVersions)
				{
					string text = pathUtil.MakeAbsolutePath(fileVersion.PhysicalPath, Path.GetDirectoryName(newProjectFilePath));
					string fileVersionPath = GetFileVersionPath(fileVersion);
					if (File.Exists(fileVersionPath) && !object.Equals(fileVersionPath, text))
					{
						Util.CopyFile(GetFileVersionPath(fileVersion), text);
					}
				}
			}
		}

		internal void SetFileVersionPath(FileVersion xmlFileVersion, string filePath, bool copy)
		{
			if (File.Exists(GetFileVersionPath(xmlFileVersion)))
			{
				if (copy)
				{
					Util.CopyFile(GetFileVersionPath(xmlFileVersion), filePath);
				}
				else
				{
					Util.MoveFile(GetFileVersionPath(xmlFileVersion), filePath);
				}
			}
			string path = (xmlFileVersion.PhysicalPath = ProjectPathUtil.MakeRelativePath(base.Project, filePath, true));
			xmlFileVersion.FileName = Path.GetFileName(path);
		}

		internal string DownloadFileVersion(FileVersion xmlFileVersion, string downloadRootFolder)
		{
			string text = Path.Combine(Path.Combine(downloadRootFolder, XmlProjectFile.Path), xmlFileVersion.FileName);
			Util.CopyFile(GetFileVersionPath(xmlFileVersion), text);
			return text;
		}

		public override string ToString()
		{
			return $"{FilenameInProject}[{Guid}]: localfilepath={LocalFilePath}";
		}

		private IUser GetUser(string userId)
		{
			return base.Project.GetProjectUsers().FirstOrDefault((IUser user) => user.UserId.Equals(userId, StringComparison.CurrentCultureIgnoreCase)) ?? base.ProjectImpl.GetUserById(userId);
		}

		private DateTime GetLastChangeTime()
		{
			if (_fileWrapper.Exists(LatestFileVersionPath))
			{
				return _fileWrapper.GetLastWriteTime(LatestFileVersionPath);
			}
			return LatestXmlFileVersion.CreatedAt.ToLocalTime();
		}
	}
}
