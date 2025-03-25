using System;
using System.IO;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.StudioServer.ProjectServer.Package;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal class OpenServerProjectPackageImport : PackageImport, IProjectPackageImport, IPackageImport, IPackageOperation
	{
		private readonly Uri _serverUri;

		private readonly string _serverUserName;

		private readonly UserManagerTokenType _serverUserType;

		private readonly DateTime _packageTimeStamp;

		private readonly string _localProjectFolder;

		private readonly IProjectsProvider _projectsProvider;

		public string ProposedProjectFolder { get; set; }

		public OpenServerProjectPackageImport(IProjectsProvider server, Uri serverUri, string serverUserName, UserManagerTokenType serverUserType, string packageFilePath, DateTime packageTimeStamp, string localProjectFolder, IProjectPathUtil projectPathUtil)
			: base(new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlProjectPackageImport(packageFilePath), projectPathUtil)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			_projectsProvider = server;
			_serverUri = serverUri;
			_serverUserName = serverUserName;
			_serverUserType = serverUserType;
			_packageTimeStamp = packageTimeStamp;
			_localProjectFolder = localProjectFolder;
		}

		protected override void StartImpl()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Invalid comparison between Unknown and I4
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			if (!Util.IsEmptyOrNonExistingDirectory(_localProjectFolder))
			{
				throw new ProjectApiException(ErrorMessages.OpenServerProjectPackageImport_LocalProjectFolderNotEmpty);
			}
			using (Stream stream = File.OpenRead(base.XmlPackageImport.Path))
			{
				ProjectPackage val = new ProjectPackage(stream, (PackageAccessMode)1);
				try
				{
					val.ExtractPackage(_localProjectFolder);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			SetPercentComplete(70);
			if ((int)base.Status == 6)
			{
				Directory.Delete(_localProjectFolder, recursive: true);
				SetStatus((PackageStatus)7);
				return;
			}
			string[] files = Directory.GetFiles(_localProjectFolder, "*" + FileTypes.ProjectFileExtension);
			if (files.Length == 0)
			{
				throw new ProjectApiException(ErrorMessages.OpenServerProjectPackageImport_ProjectFileNotFound);
			}
			string text = files[0];
			PackageTransforms.TransformPackageToProject(text, _serverUri, _serverUserName, _serverUserType);
			SetPercentComplete(75);
			Project project = (Project)(object)_projectsProvider.ImportProject(text);
			project.PublishProjectOperationImpl.StoreServerUserInformation();
			ApplyServerTimestamps(project);
			if ((int)base.Status == 6)
			{
				project.Delete();
				SetStatus((PackageStatus)7);
			}
			else
			{
				SetProject((IProject)(object)project);
				SetPercentComplete(100);
			}
		}

		private void ApplyServerTimestamps(Project project)
		{
			project.LastSynchronizationTimestamp = _packageTimeStamp;
			IProjectFile[] allProjectFiles = project.GetAllProjectFiles();
			for (int i = 0; i < allProjectFiles.Length; i++)
			{
				LanguageFile projectFile = (LanguageFile)(object)allProjectFiles[i];
				ApplyServerTimestamps(projectFile);
			}
			project.Save();
		}

		private static void ApplyServerTimestamps(LanguageFile projectFile)
		{
			FileVersion latestXmlFileVersion = projectFile.LatestXmlFileVersion;
			if (latestXmlFileVersion != null)
			{
				DateTime fileTimeStamp = latestXmlFileVersion.FileTimeStamp;
				LanguageFileServerStateSettings settingsGroup = projectFile.Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
				settingsGroup.LatestServerVersionTimestamp.Value = fileTimeStamp.Ticks;
				settingsGroup.LatestServerVersionNumber.Value = latestXmlFileVersion.VersionNumber;
			}
		}
	}
}
