using System;
using System.Collections.Generic;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.Repositories;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectPackageInitializer : IProjectPackageInitializer
	{
		private readonly List<string> _temporaryExtractionFolderList = new List<string>();

		private readonly object _temporaryExtractionFolderListLockObject = new object();

		private readonly ProjectsProvider _projectsProvider;

		private readonly IUserProvider _userProvider;

		private readonly IProjectPathUtil _projectPathUtil;

		private readonly PackageProjectArchiver _archiver;

		private readonly IEventAggregator _eventAggregator;

		private readonly IProjectOperation _projectOperation;

		internal ProjectPackageInitializer(ProjectsProvider projectsProvider, IProjectPathUtil projectPathUtil, IProjectOperation projectOperation, IEventAggregator eventAggregator)
		{
			_projectOperation = projectOperation;
			_projectPathUtil = projectPathUtil;
			_userProvider = projectsProvider.UserProvider;
			_projectsProvider = projectsProvider;
			_archiver = new PackageProjectArchiver((IZipCompress)(object)new ZipCompress());
			_eventAggregator = eventAggregator;
		}

		public IPackageProject Create(string name, PackageType packageType, Language sourceLanguage, string localDataFolder, Guid originalProjectGuid)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return Create(name, packageType, sourceLanguage, localDataFolder, originalProjectGuid, DateTime.UtcNow, _userProvider.CurrentUser);
		}

		public IPackageProject Create(string name, PackageType packageType, Language sourceLanguage, string localDataFolder, Guid originalProjectGuid, DateTime projectCreatedAt, IUser projectCreatedBy)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			PackageProjectRepository packageProjectRepository = new PackageProjectRepository(_projectsProvider.Application, _projectPathUtil, name, packageType, originalProjectGuid, _userProvider.CurrentUser, projectCreatedBy, projectCreatedAt, sourceLanguage);
			packageProjectRepository.AddUser(_userProvider.CurrentUser);
			return (IPackageProject)(object)new PackageProject(_projectsProvider, localDataFolder, sourceLanguage, (Language[])(object)new Language[0], packageProjectRepository, _archiver, _projectOperation, _eventAggregator);
		}

		public IPackageProject OpenFrom(string packageFilePath)
		{
			return OpenFrom(packageFilePath, licenseOverrideRequired: false);
		}

		public IPackageProject OpenFrom(string packageFilePath, bool licenseOverrideRequired)
		{
			PackageProjectRepository repository = new PackageProjectRepository(_projectsProvider.Application, _projectPathUtil);
			IPackageProject val = (IPackageProject)(object)new PackageProject(_projectsProvider, _archiver.ExtractPackage(packageFilePath), licenseOverrideRequired, repository, _archiver, _projectOperation, _eventAggregator);
			if (((IProject)val).LocalDataFolder.Contains(Path.GetTempPath()) && Directory.Exists(((IProject)val).LocalDataFolder))
			{
				_temporaryExtractionFolderList.Add(((IProject)val).LocalDataFolder);
			}
			return val;
		}

		public void ClearLocalTemporaryFiles()
		{
			if (_temporaryExtractionFolderList.Count == 0)
			{
				return;
			}
			lock (_temporaryExtractionFolderListLockObject)
			{
				foreach (string temporaryExtractionFolder in _temporaryExtractionFolderList)
				{
					try
					{
						if (Directory.Exists(temporaryExtractionFolder))
						{
							Directory.Delete(temporaryExtractionFolder, recursive: true);
						}
					}
					catch (Exception)
					{
					}
				}
				_temporaryExtractionFolderList.Clear();
			}
		}
	}
}
