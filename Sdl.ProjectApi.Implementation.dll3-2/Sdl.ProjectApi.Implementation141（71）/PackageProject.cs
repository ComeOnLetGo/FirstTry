using System;
using System.ComponentModel;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.Interfaces;

namespace Sdl.ProjectApi.Implementation
{
	public class PackageProject : Project, IPackageProject, IProject, IProjectConfiguration, ISettingsBundleProvider, IObjectWithSettings, INotifyPropertyChanged
	{
		private PackageType? _lazyPackageType;

		private readonly IPackageProjectRepository _packageProjectRepository;

		private readonly IPackageProjectArchiver _packageProjectArchiver;

		internal override IProjectRepository ProjectRepository => (IPackageProjectRepository)base.ProjectRepository;

		public Guid PackageGuid
		{
			get
			{
				return _packageProjectRepository.PackageGuid;
			}
			set
			{
				_packageProjectRepository.PackageGuid = value;
			}
		}

		public PackageType PackageType
		{
			get
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				if (!_lazyPackageType.HasValue)
				{
					_lazyPackageType = _packageProjectRepository.PackageType;
				}
				return _lazyPackageType.Value;
			}
		}

		public string Comment
		{
			get
			{
				return _packageProjectRepository.Comment;
			}
			set
			{
				_packageProjectRepository.Comment = value;
			}
		}

		public DateTime PackageCreatedAt => _packageProjectRepository.PackageCreatedAt.ToLocalTime();

		public IUser PackageCreatedBy => GetUserById(_packageProjectRepository.PackageCreatedBy);

		public string PackageLicenseInfo
		{
			get
			{
				if (_packageProjectRepository.PackageLicenseInfo != null && _packageProjectRepository.PackageLicenseInfo.Grant != null)
				{
					return _packageProjectRepository.PackageLicenseInfo.Grant.Feature;
				}
				return string.Empty;
			}
		}

		public IManualTask[] PackageManualTasks => _packageProjectRepository.GetPackageTasks(this).ToArray();

		internal PackageProject(ProjectsProvider projectsProvider, string localDataFolder, Language sourceLanguage, Language[] targetLanguage, IPackageProjectRepository repository, IPackageProjectArchiver packageProjectArchiver, IProjectOperation operation, IEventAggregator eventAggregator)
			: base((IProjectsProvider)(object)projectsProvider, localDataFolder, sourceLanguage, targetLanguage, repository, operation, eventAggregator)
		{
			_packageProjectRepository = repository;
			_packageProjectArchiver = packageProjectArchiver;
		}

		internal PackageProject(ProjectsProvider projectsProvider, string projectFilePath, bool licenseOverrideRequired, IPackageProjectRepository repository, IPackageProjectArchiver packageProjectArchiver, IProjectOperation operation, IEventAggregator eventAggregator)
			: base((IProjectsProvider)(object)projectsProvider, projectFilePath, licenseOverrideRequired, repository, operation, eventAggregator)
		{
			_packageProjectRepository = repository;
			_packageProjectArchiver = packageProjectArchiver;
			ValidateLicensedFeatures(projectFilePath);
		}

		public void CreatePackageFile(string packageFilePath)
		{
			OnBeforeSerialization();
			_packageProjectRepository.Save(packageFilePath);
		}

		public override IManualTask AddManualTask(IManualTask task, ITaskFile[] taskFiles, IUser assignTo, IPackageOperationMessageReporter messageReporter)
		{
			IManualTask result = base.AddManualTask(task, taskFiles, assignTo, messageReporter);
			_packageProjectRepository.AddPackageTask(task);
			return result;
		}

		protected override IManualCollaborativeTask CreateNewManualTask(IManualTaskTemplate template, IProjectFile[] files, Guid taskId, string taskName, string taskDescription, DateTime dueDate, IUser createdBy, DateTime createdAt, IUser assignedBy, IUser assignedTo, string comment)
		{
			IManualCollaborativeTask val = base.CreateNewManualTask(template, files, taskId, taskName, taskDescription, dueDate, createdBy, createdAt, assignedBy, assignedTo, comment);
			_packageProjectRepository.AddPackageTask((IManualTask)(object)val);
			return val;
		}

		public void CreatePackage(string packageFilePath)
		{
			CreatePackage(packageFilePath, saveProject: true);
		}

		public void CreatePackage(string packageFilePath, bool saveProject)
		{
			if (saveProject)
			{
				Save();
			}
			string directoryName = Path.GetDirectoryName(packageFilePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string localDataFolder = base.LocalDataFolder;
			_packageProjectArchiver.ZipDirectory(packageFilePath, localDataFolder);
			if (Directory.Exists(localDataFolder))
			{
				Directory.Delete(localDataFolder, recursive: true);
			}
		}

		public string ExtractPackage(string packageFilePath)
		{
			return _packageProjectArchiver.ExtractPackage(packageFilePath);
		}

		private void ValidateLicensedFeatures(string projectFilePath)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (LicenseOverrideRequired)
			{
				if (_packageProjectRepository.PackageLicenseInfo?.Grant == null || string.IsNullOrEmpty(_packageProjectRepository.PackageLicenseInfo.Grant.Feature) || !_packageProjectRepository.PackageLicenseInfo.Grant.Feature.Contains("AllowOpenStudioPackage"))
				{
					throw new PackageLicenseCheckFailedException(StringResources.PackageProject_InvalidLicenseOverride);
				}
				if (!PackageProjectHelpers.HasValidDigitalSignature(projectFilePath))
				{
					throw new PackageLicenseCheckFailedException(StringResources.PackageProject_InvalidDigitalSignature);
				}
			}
		}
	}
}
