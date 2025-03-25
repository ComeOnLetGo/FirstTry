using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectRepository : AbstractProjectConfigurationRepository, IProjectRepository, IProjectConfigurationRepository
	{
		protected internal readonly IProjectPathUtil PathUtil;

		private ProjectAuxiliaryFileCache _auxiliaryFileCache;

		private ProjectMergedFilesCache _mergedFilesCache;

		protected ProjectLanguageFileCache _languageFileCache;

		private readonly IProjectRepositorySerializer _projectRepositorySerializer;

		private readonly TaskFileFactory _taskFileBuilder = new TaskFileFactory();

		private readonly ProjectFileFactory _projectFileFactory;

		protected override ProjectConfiguration XmlConfiguration => _projectRepositorySerializer.XmlProject;

		protected virtual Sdl.ProjectApi.Implementation.Xml.Project XmlProject => _projectRepositorySerializer.XmlProject;

		public Guid ProjectGuid => XmlProject.Guid;

		public GeneralProjectInfo ProjectInfo => XmlProject.GeneralProjectInfo;

		public string SourceLanguage
		{
			get
			{
				return XmlProject.SourceLanguageCode;
			}
			set
			{
				string sourceLanguageCode = XmlProject.SourceLanguageCode;
				XmlProject.SourceLanguageCode = value;
				UpdateSourceLanguageForFiles(sourceLanguageCode, XmlProject.SourceLanguageCode);
			}
		}

		public Guid ProjectTemplateGuid
		{
			get
			{
				return XmlProject.ProjectTemplateGuid;
			}
			set
			{
				XmlProject.ProjectTemplateGuid = value;
			}
		}

		public Guid ReferenceProjectGuid
		{
			get
			{
				return XmlProject.ReferenceProjectGuid;
			}
			set
			{
				XmlProject.ReferenceProjectGuid = value;
			}
		}

		public IEnumerable<Guid> LanguageDirectionSettingsGuids => XmlProject.LanguageDirections.Select((Sdl.ProjectApi.Implementation.Xml.LanguageDirection ld) => ld.SettingsBundleGuid);

		private PackageOperations XmlPackageOperations
		{
			get
			{
				if (XmlProject.PackageOperations == null)
				{
					XmlProject.PackageOperations = new PackageOperations();
				}
				return XmlProject.PackageOperations;
			}
		}

		public ProjectRepository(IApplication application, IProjectPathUtil pathUtil)
			: base(application)
		{
			PathUtil = pathUtil;
			_projectFileFactory = new ProjectFileFactory(PathUtil);
			_projectRepositorySerializer = new ProjectRepositorySerializer();
		}

		public ProjectRepository(IApplication application, IProjectPathUtil pathUtil, string projectName, Guid projectGuid, IUser projectCreatedBy, DateTime projectCreatedAt, bool inPlace, Language sourceLanguage)
			: this(application, pathUtil, projectName, projectGuid, projectCreatedBy, projectCreatedAt, inPlace, sourceLanguage, new ProjectRepositorySerializer())
		{
		}

		public ProjectRepository(IApplication application, IProjectPathUtil pathUtil, string projectName, Guid projectGuid, IUser projectCreatedBy, DateTime projectCreatedAt, bool inPlace, Language sourceLanguage, IProjectRepositorySerializer projectRepositorySerializer)
			: base(application)
		{
			PathUtil = pathUtil;
			_projectFileFactory = new ProjectFileFactory(PathUtil);
			_projectRepositorySerializer = projectRepositorySerializer;
			Initialize(projectName, projectGuid, projectCreatedBy, projectCreatedAt, inPlace, sourceLanguage);
		}

		public ProjectRepository(IApplication application, IPackageProject packageProject, IProjectPathUtil pathUtil)
			: base(application)
		{
			PathUtil = pathUtil;
			_projectFileFactory = new ProjectFileFactory(PathUtil);
			_projectRepositorySerializer = new ProjectRepositorySerializer(packageProject);
			InitializeCaches();
			MarkAsInitialized();
		}

		internal ProjectRepository(IApplication application, IProjectPathUtil pathUtil, IProjectRepositorySerializer projectRepositorySerializer)
			: base(application)
		{
			PathUtil = pathUtil;
			_projectFileFactory = new ProjectFileFactory(PathUtil);
			_projectRepositorySerializer = projectRepositorySerializer;
			InitializeCaches();
			MarkAsInitialized();
			LoadCaches();
		}

		public void UpdateSourceLanguageForFiles(string oldLanguageCode, string replacementLanguage)
		{
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if (LanguageBase.Equals(languageFile.LanguageCode, oldLanguageCode))
					{
						languageFile.LanguageCode = replacementLanguage;
						break;
					}
				}
			}
		}

		protected void InitializeCaches()
		{
			_auxiliaryFileCache = new ProjectAuxiliaryFileCache();
			_mergedFilesCache = new ProjectMergedFilesCache();
			_languageFileCache = new ProjectLanguageFileCache();
		}

		protected void Initialize(string projectName, Guid projectGuid, IUser projectCreatedBy, DateTime projectCreatedAt, bool inPlace, Language sourceLanguage)
		{
			XmlProject.Version = "4.0.0.0";
			XmlProject.Guid = projectGuid;
			XmlProject.Owner = projectCreatedBy.UserId;
			AddUser(projectCreatedBy);
			XmlProject.SettingsBundleGuid = Guid.NewGuid();
			XmlProject.GeneralProjectInfo = new GeneralProjectInfo
			{
				Name = projectName,
				IsImported = false,
				CreatedAt = projectCreatedAt.ToUniversalTime(),
				CreatedBy = projectCreatedBy.UserId,
				Status = ProjectStatus.Pending,
				IsInPlace = inPlace
			};
			XmlProject.SourceLanguageCode = ((LanguageBase)sourceLanguage).IsoAbbreviation;
			InitializeCaches();
			MarkAsInitialized();
		}

		protected void DeserializeProject(XmlDocument xmlDocument)
		{
			_projectRepositorySerializer.DeserializeProject(xmlDocument);
		}

		protected virtual Sdl.ProjectApi.Implementation.Xml.Project Deserialize(string projectFilePath)
		{
			return _projectRepositorySerializer.Deserialize(projectFilePath);
		}

		public virtual void Load(string projectFilePath)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				VersionUtil.CheckProjectVersion(projectFilePath, Application.ServerEvents);
				_projectRepositorySerializer.XmlProject = Deserialize(projectFilePath);
			}
			catch (FileNotFoundException innerException)
			{
				throw new FileNotFoundException(string.Format(StringResources.ProjectFile_FileXNotFound, projectFilePath), innerException);
			}
			catch (InvalidVersionException)
			{
				throw;
			}
			catch (PackageLicenseCheckFailedException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Project_ErrorDeserializing, string.Empty, projectFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
			LoadCaches();
			MarkAsInitialized();
		}

		protected void LoadCaches()
		{
			_auxiliaryFileCache = new ProjectAuxiliaryFileCache(XmlProject.ProjectFiles);
			_mergedFilesCache = new ProjectMergedFilesCache(XmlProject.ProjectFiles);
			_languageFileCache = new ProjectLanguageFileCache(XmlProject.ProjectFiles);
		}

		protected virtual XmlDocument SerializeToXmlDocument()
		{
			return _projectRepositorySerializer.SerializeToXmlDocument();
		}

		public virtual void Save(string projectFilePath)
		{
			base.SettingsBundles.Save();
			_projectRepositorySerializer.Serialize(projectFilePath);
		}

		public override void DiscardData()
		{
			_projectRepositorySerializer.XmlProject = null;
			_auxiliaryFileCache = null;
			_mergedFilesCache = null;
			_languageFileCache = null;
			base.DiscardData();
		}

		public void MarkProjectAsStarted()
		{
			XmlProject.GeneralProjectInfo.Status = ProjectStatus.Started;
			XmlProject.GeneralProjectInfo.StartedAt = DateTime.UtcNow;
			XmlProject.GeneralProjectInfo.StartedAtSpecified = true;
		}

		public IProjectFile GetProjectFile(IProject project, string fileName, string folderName, Language language)
		{
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				bool flag = string.Compare(fileName, projectFile.Name, ignoreCase: true) == 0;
				bool flag2 = string.Compare(folderName, projectFile.Path, ignoreCase: true) == 0;
				if (flag && flag2)
				{
					Sdl.ProjectApi.Implementation.Xml.LanguageFile matchingXmlLanguageFile = GetMatchingXmlLanguageFile(language, projectFile);
					if (matchingXmlLanguageFile != null)
					{
						return (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, projectFile, matchingXmlLanguageFile, XmlProject.ProjectFiles);
					}
				}
			}
			return null;
		}

		public IProjectFile GetProjectFile(IProject project, Guid projectFileGuid)
		{
			_languageFileCache.GetXmlLanguageFile(projectFileGuid, out var xmlProjectFile, out var xmlLanguageFile);
			if (xmlProjectFile != null && xmlLanguageFile != null)
			{
				return (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, xmlProjectFile, xmlLanguageFile, XmlProject.ProjectFiles);
			}
			return null;
		}

		public void AddNativeFileVersionForSourceFile(IProject project, ITranslatableFile file, string nativeFilePath)
		{
			_languageFileCache.GetXmlLanguageFile(((IProjectFile)file).Guid, out var xmlProjectFile, out var xmlLanguageFile);
			string projectLanguageDirectory = PathUtil.GetProjectLanguageDirectory(project.IsInPlace, project.ProjectFilePath, project.LocalDataFolder, ((IProjectFile)file).Language);
			string text2 = (xmlProjectFile.Name = xmlProjectFile.Name.Substring(0, xmlProjectFile.Name.LastIndexOf(".sdlxliff", StringComparison.OrdinalIgnoreCase)));
			string text3 = PathUtil.NormalizeFolder(((ILanguageFile)file).FolderInProject);
			string path = PathUtil.NormalizeFolder(text3);
			string text4 = Path.Combine(projectLanguageDirectory, path);
			FileVersion fileVersion = GetFileVersion(project, text2, nativeFilePath, text4, validateExtensions: true, 1);
			foreach (FileVersion fileVersion2 in xmlLanguageFile.FileVersions)
			{
				fileVersion2.VersionNumber++;
			}
			xmlLanguageFile.FileVersions.Insert(0, fileVersion);
			Util.CopyFile(nativeFilePath, Path.Combine(text4, text2));
		}

		public ITranslatableFile GetTranslatableFile(IProject project, Guid languageFileGuid)
		{
			if (!_languageFileCache.GetXmlLanguageFile(languageFileGuid, out var xmlProjectFile, out var xmlLanguageFile))
			{
				return null;
			}
			return _projectFileFactory.CreateTranslatableFile(project, xmlProjectFile, xmlLanguageFile);
		}

		public ITranslatableFile GetTranslatableFile(IProject project, string fileName, string folderName, Language language)
		{
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				bool flag = string.Compare(fileName, projectFile.Name, ignoreCase: true) == 0;
				bool flag2 = string.Compare(folderName, projectFile.Path, ignoreCase: true) == 0;
				if (projectFile.Role == FileRole.Translatable && flag && flag2)
				{
					Sdl.ProjectApi.Implementation.Xml.LanguageFile matchingXmlLanguageFile = GetMatchingXmlLanguageFile(language, projectFile);
					if (matchingXmlLanguageFile != null)
					{
						return _projectFileFactory.CreateTranslatableFile(project, projectFile, matchingXmlLanguageFile);
					}
					return null;
				}
			}
			return null;
		}

		private Sdl.ProjectApi.Implementation.Xml.LanguageFile GetMatchingXmlLanguageFile(Language language, ProjectFile xmlProjectFile)
		{
			Sdl.ProjectApi.Implementation.Xml.LanguageFile result = null;
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in xmlProjectFile.LanguageFiles)
			{
				if (LanguageBase.Equals(languageFile.LanguageCode, ((LanguageBase)language).IsoAbbreviation))
				{
					result = languageFile;
					break;
				}
			}
			return result;
		}

		public List<ITranslatableFile> GetTranslatableTargetOrSingleDocumentFiles(IProject project)
		{
			List<ITranslatableFile> list = new List<ITranslatableFile>();
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				if (projectFile.Role != FileRole.Translatable && projectFile.Role != 0)
				{
					continue;
				}
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if ((!string.Equals(languageFile.LanguageCode, XmlProject.SourceLanguageCode, StringComparison.OrdinalIgnoreCase) || projectFile.LanguageFiles.Count == 1) && ShouldInclude(languageFile))
					{
						ITranslatableFile item = _projectFileFactory.CreateTranslatableFile(project, projectFile, languageFile);
						list.Add(item);
					}
				}
			}
			return list;
		}

		public List<IProjectFile> GetProjectFiles(IProject project, bool allFiles = false)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if (allFiles || _mergedFilesCache == null || ShouldInclude(languageFile))
					{
						IProjectFile item = (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, projectFile, languageFile, XmlProject.ProjectFiles);
						list.Add(item);
					}
				}
			}
			return list;
		}

		public List<IProjectFile> GetProjectFiles(IProject project, Language language)
		{
			List<IProjectFile> list = new List<IProjectFile>();
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					bool flag = _mergedFilesCache == null || ShouldInclude(languageFile);
					if (LanguageBase.Equals(languageFile.LanguageCode, ((LanguageBase)language).IsoAbbreviation) && flag)
					{
						IProjectFile item = (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, projectFile, languageFile, XmlProject.ProjectFiles);
						list.Add(item);
					}
				}
			}
			return list;
		}

		public List<IProjectFile> GetPagedProjectFiles(IProject project, Language language, int pageSize, int skip)
		{
			ProjectFile[] array = XmlProject.ProjectFiles.Where((ProjectFile pf) => pf.LanguageFiles.Where((Sdl.ProjectApi.Implementation.Xml.LanguageFile lf) => LanguageBase.Equals(lf.LanguageCode, ((LanguageBase)language).IsoAbbreviation)).Any()).ToArray();
			int num = ((pageSize + skip > array.Count()) ? array.Count() : (pageSize + skip));
			List<IProjectFile> list = new List<IProjectFile>();
			for (int i = skip; i < num; i++)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in array[i].LanguageFiles)
				{
					bool flag = _mergedFilesCache == null || ShouldInclude(languageFile);
					if (LanguageBase.Equals(languageFile.LanguageCode, ((LanguageBase)language).IsoAbbreviation) && flag)
					{
						IProjectFile item = (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, array[i], languageFile, array.ToList());
						list.Add(item);
					}
				}
			}
			return list;
		}

		public IMergedTranslatableFile GetMergedTranslatableFile(IProject project, Guid projectFileGuid)
		{
			List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> childFiles = _mergedFilesCache.GetChildFiles(projectFileGuid);
			IMergedTranslatableFile result = null;
			if (childFiles.Any())
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile item in childFiles)
				{
					if (item.MergeState == MergeState.NotMerged || item.MergeState == MergeState.Merged)
					{
						IProjectFile projectFile = GetProjectFile(project, item.Guid);
						result = (IMergedTranslatableFile)(object)((projectFile is IMergedTranslatableFile) ? projectFile : null);
					}
				}
			}
			return result;
		}

		public List<IMergedTranslatableFile> GetMergedTranslatableFileHistory(IProject project, Guid projectFileGuid)
		{
			List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> childFiles = _mergedFilesCache.GetChildFiles(projectFileGuid);
			List<IMergedTranslatableFile> list = new List<IMergedTranslatableFile>();
			if (childFiles.Any())
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile item2 in childFiles)
				{
					IProjectFile projectFile = GetProjectFile(project, item2.Guid);
					IMergedTranslatableFile item = (IMergedTranslatableFile)(object)((projectFile is IMergedTranslatableFile) ? projectFile : null);
					list.Add(item);
				}
			}
			return list;
		}

		public IProjectFile AddProjectFile(IProject project, string fileToAdd, string folderInProject, Language sourceLanguage, FileRole fileRole = 0)
		{
			FileRole result;
			bool flag = Enum.TryParse<FileRole>(((object)(FileRole)(ref fileRole)).ToString(), out result);
			ProjectFile projectFile = AddProjectFile(project, fileToAdd, folderInProject, flag ? result : FileRole.Unknown, sourceLanguage, null, Guid.Empty);
			return (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, projectFile, projectFile.LanguageFiles[0], XmlProject.ProjectFiles);
		}

		internal void AddProjectFile(ProjectFile xmlProjectFile)
		{
			XmlProject.ProjectFiles.Add(xmlProjectFile);
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile in xmlProjectFile.LanguageFiles)
			{
				_languageFileCache.Add(xmlProjectFile, xmlLanguageFile);
				xmlLanguageFile.ChildFiles?.ForEach(delegate(LanguageFileRef f)
				{
					_mergedFilesCache.AddToMergedFileIndex(f.LanguageFileGuid, xmlLanguageFile);
				});
			}
		}

		public ITranslatableFile AddTranslatableFile(IProject project, ITranslatableFile translatableFile, ITranslatableFile referenceFile, IPackageOperationMessageReporter messageReporter)
		{
			TranslatableFile translatableFile2 = (TranslatableFile)(object)referenceFile;
			if (translatableFile2 == null)
			{
				ProjectFile projectFile = AddProjectFile(project, translatableFile, FileRole.Translatable, Guid.Empty);
				IMergedTranslatableFile val = (IMergedTranslatableFile)(object)((translatableFile is IMergedTranslatableFile) ? translatableFile : null);
				if (val != null)
				{
					AddMergedTranslatableFiles(project, val, projectFile.LanguageFiles[0], messageReporter);
				}
				return _projectFileFactory.CreateTranslatableFile(project, projectFile, projectFile.LanguageFiles[0]);
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = AddLanguageFile(project, ((IProjectFile)translatableFile).LocalFilePath, ((IProjectFile)translatableFile).Guid, ((ILanguageFile)translatableFile).FolderInProject, ((IProjectFile)translatableFile).Language, translatableFile2.XmlProjectFile);
			IMergedTranslatableFile val2 = (IMergedTranslatableFile)(object)((translatableFile is IMergedTranslatableFile) ? translatableFile : null);
			if (val2 != null)
			{
				AddMergedTranslatableFiles(project, val2, xmlLanguageFile, messageReporter);
			}
			return _projectFileFactory.CreateTranslatableFile(project, translatableFile2.XmlProjectFile, xmlLanguageFile);
		}

		public ITranslatableFile AddTranslatableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, string filterDefinitionId, ITranslatableFile referenceFile)
		{
			TranslatableFile translatableFile = (TranslatableFile)(object)referenceFile;
			if (translatableFile == null)
			{
				ProjectFile projectFile = AddProjectFile(project, fileToAdd, Guid.NewGuid(), id, folderInProject, FileRole.Translatable, language, filterDefinitionId, Guid.Empty);
				return _projectFileFactory.CreateTranslatableFile(project, projectFile, projectFile.LanguageFiles[0]);
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = AddLanguageFile(project, fileToAdd, id, folderInProject, language, translatableFile.XmlProjectFile);
			return _projectFileFactory.CreateTranslatableFile(project, translatableFile.XmlProjectFile, xmlLanguageFile);
		}

		public ILocalizableFile AddLocalizableFile(IProject project, ILocalizableFile localizableFile, ILocalizableFile referenceFile)
		{
			LocalizableFile localizableFile2 = (LocalizableFile)(object)referenceFile;
			if (localizableFile2 == null)
			{
				ProjectFile projectFile = AddProjectFile(project, ((IProjectFile)localizableFile).LocalFilePath, Guid.NewGuid(), ((IProjectFile)localizableFile).Guid, ((ILanguageFile)localizableFile).FolderInProject, FileRole.Localizable, ((IProjectFile)localizableFile).Language, null, Guid.Empty);
				return _projectFileFactory.CreateLocalizableFile(project, projectFile, projectFile.LanguageFiles[0]);
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = AddLanguageFile(project, ((IProjectFile)localizableFile).LocalFilePath, ((IProjectFile)localizableFile).Guid, ((ILanguageFile)localizableFile).FolderInProject, ((IProjectFile)localizableFile).Language, localizableFile2.XmlProjectFile);
			return _projectFileFactory.CreateLocalizableFile(project, localizableFile2.XmlProjectFile, xmlLanguageFile);
		}

		public IReferenceFile AddReferenceFile(IProject project, IReferenceFile referenceFile, IReferenceFile parentReferenceFile)
		{
			LanguageFile languageFile = (LanguageFile)(object)parentReferenceFile;
			if (languageFile == null)
			{
				ProjectFile projectFile = AddProjectFile(project, ((IProjectFile)referenceFile).LocalFilePath, Guid.NewGuid(), ((IProjectFile)referenceFile).Guid, ((ILanguageFile)referenceFile).FolderInProject, FileRole.Reference, ((IProjectFile)referenceFile).Language, null, Guid.Empty);
				return (IReferenceFile)(object)new ReferenceFile(project, projectFile, projectFile.LanguageFiles[0], PathUtil);
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = AddLanguageFile(project, ((IProjectFile)referenceFile).LocalFilePath, ((IProjectFile)referenceFile).Guid, ((ILanguageFile)referenceFile).FolderInProject, ((IProjectFile)referenceFile).Language, languageFile.XmlProjectFile);
			return (IReferenceFile)(object)new ReferenceFile(project, languageFile.XmlProjectFile, xmlLanguageFile, PathUtil);
		}

		public IMergedTranslatableFile CreateMergedTranslatableFile(IProject project, string mergedFileName, string folderInProject, Language language, string fileTypeDefinitionId, ITranslatableFile[] childFiles)
		{
			ProjectFile projectFile = AddVirtualXmlProjectFile(project, mergedFileName, folderInProject, language, fileTypeDefinitionId);
			projectFile.FilterDefinitionId = fileTypeDefinitionId;
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = projectFile.LanguageFiles[0];
			languageFile.MergeState = MergeState.NotMerged;
			languageFile.MergeStateSpecified = true;
			foreach (ITranslatableFile val in childFiles)
			{
				LanguageFileRef item = new LanguageFileRef
				{
					LanguageFileGuid = ((IProjectFile)val).Guid
				};
				languageFile.ChildFiles.Add(item);
				_mergedFilesCache.AddToMergedFileIndex(((IProjectFile)val).Guid, languageFile);
			}
			return (IMergedTranslatableFile)(object)new MergedTranslatableFile(project, projectFile, languageFile, PathUtil);
		}

		public bool SplitFileIntoTargetLanguage(IProject project, ILanguageFile languageFile, Language targetLanguage, out IProjectFile targetLanguageFile)
		{
			bool result = true;
			LanguageFile languageFile2 = languageFile as LanguageFile;
			MergedTranslatableFile mergedTranslatableFile = languageFile as MergedTranslatableFile;
			List<ITranslatableFile> splitChildFiles = new List<ITranslatableFile>();
			if (mergedTranslatableFile != null)
			{
				splitChildFiles = SplitChildFilesIntoTargetLanguage(project, mergedTranslatableFile, targetLanguage);
			}
			targetLanguageFile = (IProjectFile)(object)GetLanguageFile(project, languageFile2, targetLanguage);
			if (targetLanguageFile != null)
			{
				if (mergedTranslatableFile == null)
				{
					return false;
				}
				result = false;
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = CreateXmlLanguageFile(project, languageFile2, targetLanguage, Guid.NewGuid(), copyFile: true);
			if (mergedTranslatableFile != null)
			{
				InitSplitXmlLanguageFile(mergedTranslatableFile, xmlLanguageFile);
			}
			AddXmlLanguageFile(languageFile2.XmlProjectFile, xmlLanguageFile);
			targetLanguageFile = (IProjectFile)(object)_projectFileFactory.CreateProjectFile(project, languageFile2.XmlProjectFile, xmlLanguageFile, XmlProject.ProjectFiles);
			AddFileSettingsToProject(project, languageFile, targetLanguage, targetLanguageFile, xmlLanguageFile);
			if (mergedTranslatableFile != null)
			{
				AddToMergeFilesCache(targetLanguageFile, splitChildFiles);
			}
			return result;
		}

		private ILanguageFile GetLanguageFile(IProject project, LanguageFile languageFile, Language language)
		{
			if (languageFile.Language == language)
			{
				return (ILanguageFile)(object)languageFile;
			}
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile2 in languageFile.XmlProjectFile.LanguageFiles)
			{
				if (LanguageBase.Equals(languageFile2.LanguageCode, ((LanguageBase)language).IsoAbbreviation))
				{
					return _projectFileFactory.CreateLanguageFile(project, languageFile.XmlProjectFile, languageFile2);
				}
			}
			return null;
		}

		public ILocalizableFile AddLocalizableFile(IProject project, string fileToAdd, string folderInProject, Language language, Guid id, ILocalizableFile referenceFile)
		{
			LocalizableFile localizableFile = (LocalizableFile)(object)referenceFile;
			if (localizableFile == null)
			{
				ProjectFile projectFile = AddProjectFile(project, fileToAdd, Guid.NewGuid(), id, folderInProject, FileRole.Localizable, language, null, Guid.Empty);
				return (ILocalizableFile)(object)_projectFileFactory.CreateTranslatableFile(project, projectFile, projectFile.LanguageFiles[0]);
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile = AddLanguageFile(project, fileToAdd, id, folderInProject, language, localizableFile.XmlProjectFile);
			return _projectFileFactory.CreateLocalizableFile(project, localizableFile.XmlProjectFile, xmlLanguageFile);
		}

		private Sdl.ProjectApi.Implementation.Xml.LanguageFile CreateXmlLanguageFile(IProject project, LanguageFile languageFile, Language targetLanguage, Guid targetLanguageFileGuid, bool copyFile)
		{
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile2 = new Sdl.ProjectApi.Implementation.Xml.LanguageFile
			{
				Guid = targetLanguageFileGuid,
				LanguageCode = ((LanguageBase)targetLanguage).IsoAbbreviation
			};
			FileVersion latestXmlFileVersion = languageFile.LatestXmlFileVersion;
			if (latestXmlFileVersion != null)
			{
				FileVersion fileVersion = new FileVersion();
				fileVersion.AssignNewGuid();
				fileVersion.CreatedAt = DateTime.UtcNow;
				fileVersion.CreatedBy = project.CurrentUser.UserId;
				AddUser(project.CurrentUser);
				fileVersion.Size = latestXmlFileVersion.Size;
				fileVersion.FileName = latestXmlFileVersion.FileName;
				fileVersion.FileTimeStamp = latestXmlFileVersion.FileTimeStamp;
				fileVersion.VersionNumber = latestXmlFileVersion.VersionNumber + 1;
				string path = Path.Combine(project.GetProjectLanguageDirectory(targetLanguage), languageFile.XmlProjectFile.Path);
				string text = Path.Combine(path, fileVersion.FileName);
				string physicalPath = PathUtil.MakeRelativePath(project, text, true);
				fileVersion.PhysicalPath = physicalPath;
				languageFile2.FileVersions.Add(fileVersion);
				if (copyFile)
				{
					Util.CopyFile(languageFile.GetFileVersionPath(latestXmlFileVersion), text);
				}
			}
			return languageFile2;
		}

		private void AddFileSettingsToProject(IProject project, ILanguageFile languageFile, Language targetLanguage, IProjectFile targetLanguageFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			ITranslatableFile val = (ITranslatableFile)(object)((targetLanguageFile is ITranslatableFile) ? targetLanguageFile : null);
			if (val != null)
			{
				xmlLanguageFile.SettingsBundleGuid = Guid.NewGuid();
				base.SettingsBundles.AddSettingsBundle(xmlLanguageFile.SettingsBundleGuid, SettingsUtil.CreateSettingsBundle(((IObjectWithSettings)((IProjectConfiguration)project).GetLanguageDirection(((IProjectFile)languageFile).Language, targetLanguage)).Settings));
			}
		}

		private List<ITranslatableFile> SplitChildFilesIntoTargetLanguage(IProject project, MergedTranslatableFile mergedFile, Language targetLanguage)
		{
			List<ITranslatableFile> list = new List<ITranslatableFile>();
			ITranslatableFile[] childFiles = mergedFile.ChildFiles;
			ITranslatableFile[] array = childFiles;
			foreach (ITranslatableFile val in array)
			{
				TranslatableFile languageFile = (TranslatableFile)(object)val;
				if (SplitFileIntoTargetLanguage(project, (ILanguageFile)(object)languageFile, targetLanguage, out var targetLanguageFile))
				{
					ITranslatableFile item = (ITranslatableFile)(object)((targetLanguageFile is ITranslatableFile) ? targetLanguageFile : null);
					list.Add(item);
				}
			}
			return list;
		}

		private void InitSplitXmlLanguageFile(MergedTranslatableFile mergedFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			xmlLanguageFile.MergeState = EnumConvert.ConvertMergeState(mergedFile.MergeState);
			xmlLanguageFile.MergeStateSpecified = true;
			Language targetLanguage = new Language(xmlLanguageFile.LanguageCode);
			ITranslatableFile[] childFiles = mergedFile.ChildFiles;
			ITranslatableFile[] array = childFiles;
			foreach (ITranslatableFile val in array)
			{
				ITranslatableFile val2 = Array.Find(val.TargetLanguageFiles, (ITranslatableFile tf) => ((object)((IProjectFile)tf).Language).Equals((object)targetLanguage));
				LanguageFileRef item = new LanguageFileRef
				{
					LanguageFileGuid = ((IProjectFile)val2).Guid
				};
				xmlLanguageFile.ChildFiles.Add(item);
			}
		}

		private void AddToMergeFilesCache(IProjectFile targetLanguageFile, List<ITranslatableFile> splitChildFiles)
		{
			foreach (ITranslatableFile splitChildFile in splitChildFiles)
			{
				_languageFileCache.GetXmlLanguageFile(targetLanguageFile.Guid, out var _, out var xmlLanguageFile);
				_mergedFilesCache.AddToMergedFileIndex(((IProjectFile)splitChildFile).Guid, xmlLanguageFile);
			}
		}

		public void ResetAnalysisStatics(IProject project)
		{
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if (languageFile.AnalysisStatistics != null)
					{
						Sdl.ProjectApi.Implementation.Statistics.AnalysisStatistics.ResetAnalysisCounts(project, languageFile.AnalysisStatistics);
					}
				}
			}
		}

		public void ResetWordCountStatistics()
		{
			XmlProject.WordCountStatistics = null;
		}

		public ILanguageFile GetSourceLanguageFile(IProject project, ILanguageFile targetLanguageFile)
		{
			if (!_languageFileCache.GetXmlLanguageFile(((IProjectFile)targetLanguageFile).Guid, out var xmlProjectFile, out var _))
			{
				return null;
			}
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFileByLanguage = xmlProjectFile.GetLanguageFileByLanguage(SourceLanguage);
			if (languageFileByLanguage == null)
			{
				return null;
			}
			return _projectFileFactory.CreateProjectFile(project, xmlProjectFile, languageFileByLanguage, XmlProject.ProjectFiles);
		}

		internal ProjectFile GetXmlProjectFile(Guid projectFileGuid)
		{
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				if (projectFile.Guid == projectFileGuid)
				{
					return projectFile;
				}
			}
			return null;
		}

		private bool ShouldInclude(Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			if (!xmlLanguageFile.MergeStateSpecified)
			{
				List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> childFiles = _mergedFilesCache.GetChildFiles(xmlLanguageFile.Guid);
				if (childFiles.Count > 0)
				{
					foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile item in childFiles)
					{
						if (item.MergeState == MergeState.NotMerged || item.MergeState == MergeState.Merged)
						{
							return false;
						}
					}
				}
				return true;
			}
			return xmlLanguageFile.MergeState != MergeState.Split;
		}

		private ProjectFile AddProjectFile(IProject project, string fileToAdd, string folderInProject, FileRole fileRole, Language language, string filterDefinitionId, Guid parentXmlProjectFileGuid)
		{
			folderInProject = PathUtil.NormalizeFolder(folderInProject);
			string fileName = Path.GetFileName(fileToAdd);
			FileInfo fileInfo = new FileInfo(fileToAdd);
			DateTime lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
			long length = fileInfo.Length;
			string projectLanguageDirectory = project.GetProjectLanguageDirectory(language);
			string path = Path.Combine(projectLanguageDirectory, folderInProject);
			string targetFilePath = Path.Combine(path, fileName);
			Util.CopyFile(fileToAdd, targetFilePath);
			return AddXmlProjectFile(project, fileName, folderInProject, lastWriteTimeUtc, length, fileRole, language, filterDefinitionId, parentXmlProjectFileGuid, projectLanguageDirectory);
		}

		private ProjectFile AddProjectFile(IProject project, ITranslatableFile file, FileRole fileRole, Guid parentXmlProjectFileGuid)
		{
			string text = PathUtil.NormalizeFolder(((ILanguageFile)file).FolderInProject);
			string projectLanguageDirectory = project.GetProjectLanguageDirectory(((IProjectFile)file).Language);
			ProjectFile result = AddXmlProjectFileWithNativeVersion(project, file, text, fileRole, parentXmlProjectFileGuid, projectLanguageDirectory);
			string path = Path.Combine(projectLanguageDirectory, text);
			IFileRevision[] revisions = ((ILanguageFile)file).Revisions;
			foreach (IFileRevision val in revisions)
			{
				string targetFilePath = Path.Combine(path, val.Filename);
				Util.CopyFile(PathUtil.MakeAbsolutePath(project, val.LocalFilePath, true), targetFilePath);
			}
			return result;
		}

		private ProjectFile AddXmlProjectFileWithNativeVersion(IProject project, ITranslatableFile file, string folderInProject, FileRole fileRole, Guid parentXmlProjectFileGuid, string projectLanguageDirectory)
		{
			string text = PathUtil.NormalizeFolder(folderInProject);
			string absoluteFolderInProject = Path.Combine(projectLanguageDirectory, text);
			bool validateExtensions = this is IPackageProject;
			ProjectFile projectFile = new ProjectFile
			{
				Guid = Guid.NewGuid(),
				Name = ((ILanguageFile)file).Revisions[0].Filename,
				Path = text,
				Role = fileRole
			};
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = new Sdl.ProjectApi.Implementation.Xml.LanguageFile
			{
				Guid = ((IProjectFile)file).Guid,
				LanguageCode = ((LanguageBase)((IProjectFile)file).Language).IsoAbbreviation
			};
			projectFile.FilterDefinitionId = ((IProjectFile)file).FileTypeDefinitionId;
			if (((ILanguageFile)file).Revisions.Length == 1)
			{
				languageFile.FileVersions.Add(GetFileVersion(project, ((ILanguageFile)file).Revisions[0], absoluteFolderInProject, validateExtensions, 1));
			}
			else
			{
				languageFile.FileVersions.Add(GetFileVersion(project, ((ILanguageFile)file).Revisions[0], absoluteFolderInProject, validateExtensions, 1));
				languageFile.FileVersions.Add(GetFileVersion(project, ((ILanguageFile)file).Revisions[((ILanguageFile)file).Revisions.Length - 1], absoluteFolderInProject, validateExtensions, 2));
			}
			if (fileRole == FileRole.Auxiliary)
			{
				projectFile.ParentProjectFileGuid = parentXmlProjectFileGuid;
				_auxiliaryFileCache.AddFile(parentXmlProjectFileGuid, projectFile);
			}
			AddXmlLanguageFile(projectFile, languageFile);
			XmlProject.ProjectFiles.Add(projectFile);
			return projectFile;
		}

		private FileVersion GetFileVersion(IProject project, IFileRevision fileRevision, string absoluteFolderInProject, bool validateExtensions, int version)
		{
			string filename = fileRevision.Filename;
			FileInfo fileInfo = new FileInfo(fileRevision.LocalFilePath);
			string text = Path.Combine(absoluteFolderInProject, filename);
			PathUtil.ValidateAbsolutePath(text, validateExtensions);
			string physicalPath = PathUtil.MakeRelativePath(project, text, true);
			FileVersion fileVersion = new FileVersion();
			fileVersion.AssignNewGuid();
			fileVersion.CreatedAt = DateTime.UtcNow;
			fileVersion.CreatedBy = project.CurrentUser.UserId;
			AddUser(project.CurrentUser);
			fileVersion.FileName = filename;
			fileVersion.FileTimeStamp = fileInfo.LastWriteTimeUtc;
			fileVersion.VersionNumber = version;
			fileVersion.Size = fileInfo.Length;
			fileVersion.PhysicalPath = physicalPath;
			return fileVersion;
		}

		private FileVersion GetFileVersion(IProject project, string nativeFileName, string filePath, string absoluteFolderInProject, bool validateExtensions, int version)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			string text = Path.Combine(absoluteFolderInProject, nativeFileName);
			PathUtil.ValidateAbsolutePath(text, validateExtensions);
			string physicalPath = PathUtil.MakeRelativePath(project, text, true);
			FileVersion fileVersion = new FileVersion();
			fileVersion.AssignNewGuid();
			fileVersion.CreatedAt = DateTime.UtcNow;
			fileVersion.CreatedBy = project.CurrentUser.UserId;
			AddUser(project.CurrentUser);
			fileVersion.FileName = nativeFileName;
			fileVersion.FileTimeStamp = fileInfo.LastWriteTimeUtc;
			fileVersion.VersionNumber = version;
			fileVersion.Size = fileInfo.Length;
			fileVersion.PhysicalPath = physicalPath;
			return fileVersion;
		}

		private ProjectFile AddProjectFile(IProject project, string fileToAdd, Guid projectFileGuid, Guid fileExistingGuid, string folderInProject, FileRole fileRole, Language language, string filterDefinitionId, Guid parentXmlProjectFileGuid)
		{
			folderInProject = PathUtil.NormalizeFolder(folderInProject);
			string fileName = Path.GetFileName(fileToAdd);
			FileInfo fileInfo = new FileInfo(fileToAdd);
			DateTime lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
			long length = fileInfo.Length;
			string projectLanguageDirectory = project.GetProjectLanguageDirectory(language);
			ProjectFile result = AddXmlProjectFile(project, projectFileGuid, fileExistingGuid, fileName, folderInProject, lastWriteTimeUtc, length, fileRole, language, filterDefinitionId, parentXmlProjectFileGuid, projectLanguageDirectory);
			string path = Path.Combine(projectLanguageDirectory, folderInProject);
			string targetFilePath = Path.Combine(path, fileName);
			Util.CopyFile(fileToAdd, targetFilePath);
			return result;
		}

		private ProjectFile AddXmlProjectFile(IProject project, string fileName, string folderInProject, DateTime lastModifiedAtUtc, long size, FileRole fileRole, Language language, string filterDefinitionId, Guid parentXmlProjectFileGuid, string projectLanguageDirectory)
		{
			return AddXmlProjectFile(project, Guid.NewGuid(), Guid.NewGuid(), fileName, folderInProject, lastModifiedAtUtc, size, fileRole, language, filterDefinitionId, parentXmlProjectFileGuid, projectLanguageDirectory);
		}

		private ProjectFile AddXmlProjectFile(IProject project, Guid projectFileGuid, Guid languageFileGuid, string fileName, string folderInProject, DateTime lastModifiedAtUtc, long size, FileRole fileRole, Language language, string filterDefinitionId, Guid parentXmlProjectFileGuid, string projectLanguageDirectory)
		{
			string text = PathUtil.NormalizeFolder(folderInProject);
			string path = Path.Combine(projectLanguageDirectory, text);
			string text2 = Path.Combine(path, fileName);
			bool flag = this is IPackageProject;
			PathUtil.ValidateAbsolutePath(text2, flag);
			string physicalPath = PathUtil.MakeRelativePath(project, text2, true);
			ProjectFile projectFile = new ProjectFile
			{
				Guid = projectFileGuid,
				Name = fileName,
				Path = text,
				Role = fileRole
			};
			if (fileRole == FileRole.Auxiliary)
			{
				projectFile.ParentProjectFileGuid = parentXmlProjectFileGuid;
				_auxiliaryFileCache.AddFile(parentXmlProjectFileGuid, projectFile);
			}
			projectFile.FilterDefinitionId = filterDefinitionId;
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = new Sdl.ProjectApi.Implementation.Xml.LanguageFile
			{
				Guid = languageFileGuid,
				LanguageCode = ((LanguageBase)language).IsoAbbreviation
			};
			FileVersion fileVersion = new FileVersion();
			fileVersion.AssignNewGuid();
			fileVersion.CreatedAt = DateTime.UtcNow;
			fileVersion.CreatedBy = project.CurrentUser.UserId;
			AddUser(project.CurrentUser);
			fileVersion.FileName = fileName;
			fileVersion.FileTimeStamp = lastModifiedAtUtc;
			fileVersion.VersionNumber = 1;
			fileVersion.Size = size;
			fileVersion.PhysicalPath = physicalPath;
			languageFile.FileVersions.Add(fileVersion);
			AddXmlLanguageFile(projectFile, languageFile);
			XmlProject.ProjectFiles.Add(projectFile);
			return projectFile;
		}

		private ProjectFile AddVirtualXmlProjectFile(IProject project, string fileName, string folderInProject, Language language, string filterDefinitionId)
		{
			string text = PathUtil.NormalizeFolder(folderInProject);
			string path = Path.Combine(project.GetProjectLanguageDirectory(language), text);
			string text2 = Path.Combine(path, fileName);
			bool flag = this is IPackageProject;
			PathUtil.ValidateAbsolutePath(text2, flag);
			ProjectFile projectFile = new ProjectFile();
			projectFile.Guid = Guid.NewGuid();
			projectFile.Name = fileName;
			projectFile.Path = text;
			projectFile.Role = FileRole.Translatable;
			projectFile.FilterDefinitionId = filterDefinitionId;
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = new Sdl.ProjectApi.Implementation.Xml.LanguageFile();
			languageFile.Guid = Guid.NewGuid();
			languageFile.LanguageCode = ((LanguageBase)language).IsoAbbreviation;
			AddXmlLanguageFile(projectFile, languageFile);
			XmlProject.ProjectFiles.Add(projectFile);
			return projectFile;
		}

		private void AddMergedTranslatableFiles(IProject project, IMergedTranslatableFile mergedTranslatableFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IPackageOperationMessageReporter messageReporter)
		{
			xmlLanguageFile.MergeState = MergeState.Merged;
			xmlLanguageFile.MergeStateSpecified = true;
			ITranslatableFile[] childFiles = mergedTranslatableFile.ChildFiles;
			foreach (ITranslatableFile val in childFiles)
			{
				LanguageFileRef languageFileRef = new LanguageFileRef();
				languageFileRef.LanguageFileGuid = ((IProjectFile)val).Guid;
				xmlLanguageFile.ChildFiles.Add(languageFileRef);
				_mergedFilesCache.AddToMergedFileIndex(((IProjectFile)val).Guid, xmlLanguageFile);
				project.AddTranslatableFile(val, messageReporter);
			}
		}

		private Sdl.ProjectApi.Implementation.Xml.LanguageFile AddLanguageFile(IProject project, string fileToAdd, Guid languageFileGuid, string folderInProject, Language language, ProjectFile parentXmlProjectFile)
		{
			folderInProject = PathUtil.NormalizeFolder(folderInProject);
			string fileName = Path.GetFileName(fileToAdd);
			string path = PathUtil.NormalizeFolder(folderInProject);
			string path2 = Path.Combine(project.GetProjectLanguageDirectory(language), path);
			string text = Path.Combine(path2, fileName);
			bool flag = this is IPackageProject;
			PathUtil.ValidateAbsolutePath(text, flag);
			string relativeFilePath = PathUtil.MakeRelativePath(project, text, true);
			FileInfo fileInfo = new FileInfo(fileToAdd);
			DateTime lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
			long length = fileInfo.Length;
			Sdl.ProjectApi.Implementation.Xml.LanguageFile result = AddXmlLanguageFile(project, languageFileGuid, fileName, lastWriteTimeUtc, length, language, relativeFilePath, parentXmlProjectFile);
			Util.CopyFile(fileToAdd, text);
			return result;
		}

		private Sdl.ProjectApi.Implementation.Xml.LanguageFile AddXmlLanguageFile(IProject project, Guid languageFileGuid, string fileName, DateTime lastModifiedAtUtc, long size, Language language, string relativeFilePath, ProjectFile xmlProjectFile)
		{
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = new Sdl.ProjectApi.Implementation.Xml.LanguageFile();
			languageFile.Guid = languageFileGuid;
			languageFile.LanguageCode = ((LanguageBase)language).IsoAbbreviation;
			FileVersion fileVersion = new FileVersion();
			fileVersion.AssignNewGuid();
			fileVersion.CreatedAt = DateTime.UtcNow;
			fileVersion.CreatedBy = project.CurrentUser.UserId;
			AddUser(project.CurrentUser);
			fileVersion.FileName = fileName;
			fileVersion.FileTimeStamp = lastModifiedAtUtc;
			fileVersion.VersionNumber = 1;
			fileVersion.Size = size;
			fileVersion.PhysicalPath = relativeFilePath;
			languageFile.FileVersions.Add(fileVersion);
			AddXmlLanguageFile(xmlProjectFile, languageFile);
			return languageFile;
		}

		public virtual IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, DateTime dueDate, IUser createdBy, DateTime createdAt)
		{
			Sdl.ProjectApi.Implementation.Xml.ManualTask manualTask = new XmlManualTaskBuilder(taskId, taskName, taskDescription, createdAt).WithDueDate(dueDate).WithTemplate(template, XmlConfiguration).WithCreatedBy(createdBy)
				.Build();
			if (createdBy != null)
			{
				AddUser(createdBy);
			}
			XmlProject.Tasks.Items.Add(manualTask);
			return (IManualCollaborativeTask)(object)new ManualTask(project, manualTask, _taskFileBuilder, PathUtil);
		}

		public IManualCollaborativeTask CreateNewManualTask(IProject project, IManualTaskTemplate template, Guid taskId, string taskName, string taskDescription, string externalId, DateTime startedAt, DateTime dueDate, IUser createdBy, DateTime createdAt, DateTime completedAt, int percentComplete)
		{
			Sdl.ProjectApi.Implementation.Xml.ManualTask manualTask = new XmlManualTaskBuilder(taskId, taskName, taskDescription, createdAt).WithExternalId(externalId).WithPercentComplete(percentComplete).WithDueDate(dueDate)
				.WithStartedAt(startedAt)
				.WithCompletedAt(completedAt)
				.WithTemplate(template, XmlConfiguration)
				.WithCreatedBy(createdBy)
				.Build();
			if (createdBy != null)
			{
				AddUser(createdBy);
			}
			XmlProject.Tasks.Items.Add(manualTask);
			return (IManualCollaborativeTask)(object)new ManualTask(project, manualTask, _taskFileBuilder, PathUtil);
		}

		public List<IScheduledTask> GetTasks(IProject project)
		{
			List<IScheduledTask> list = new List<IScheduledTask>();
			foreach (Task item in XmlProject.Tasks.Items)
			{
				ScheduledTask scheduledTask = null;
				if (item is Sdl.ProjectApi.Implementation.Xml.ManualTask xmlManualTask)
				{
					scheduledTask = new ManualTask(project, xmlManualTask, _taskFileBuilder, PathUtil);
				}
				else
				{
					if (!(item is Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlTask))
					{
						throw new Exception("Unexpected task type: " + item.GetType().ToString());
					}
					scheduledTask = new AutomaticTask(project, xmlTask, _taskFileBuilder, PathUtil);
				}
				list.Add((IScheduledTask)(object)scheduledTask);
			}
			return list;
		}

		public void RemoveTask(IScheduledTask task)
		{
			Task task2 = XmlProject.Tasks.Items.FirstOrDefault(delegate(Task t)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				Guid guid = t.Guid;
				TaskId id = ((ITaskBase)task).Id;
				return guid == ((TaskId)(ref id)).ToGuidArray()[0];
			});
			if (task2 != null)
			{
				XmlProject.Tasks.Items.Remove(task2);
			}
		}

		public AutomaticTask AddAutomaticTasks(IProject project, Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask)
		{
			XmlProject.Tasks.Items.Add(xmlAutomaticTask);
			return new AutomaticTask(project, xmlAutomaticTask, _taskFileBuilder, PathUtil);
		}

		public AutomaticTask AddAutomaticTasks(IProject project, Guid newGuid, Guid previousTaskGuid, ITaskTemplate[] subTaskTemplates, IUser currentUser, string complexTaskTemplateId)
		{
			Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask = new Sdl.ProjectApi.Implementation.Xml.AutomaticTask();
			automaticTask.Guid = newGuid;
			foreach (ITaskTemplate val in subTaskTemplates)
			{
				automaticTask.TaskTemplateIds.Add(val.Id);
			}
			automaticTask.ComplexTaskTemplateId = complexTaskTemplateId;
			automaticTask.PredecessorTaskGuid = previousTaskGuid;
			automaticTask.CreatedAt = DateTime.UtcNow;
			automaticTask.CreatedBy = currentUser.UserId;
			AddUser(currentUser);
			automaticTask.PercentComplete = 0;
			automaticTask.Status = TaskStatus.Created;
			return AddAutomaticTasks(project, automaticTask);
		}

		public AutomaticTask AddAutomaticTasks(IProject project, IUser currentUser, string taskTemplateId)
		{
			Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask = new Sdl.ProjectApi.Implementation.Xml.AutomaticTask();
			automaticTask.AssignNewGuid();
			automaticTask.TaskTemplateIds.Add(taskTemplateId);
			automaticTask.CreatedAt = DateTime.UtcNow;
			automaticTask.CreatedBy = currentUser.UserId;
			AddUser(currentUser);
			automaticTask.PercentComplete = 0;
			automaticTask.Status = TaskStatus.Created;
			return AddAutomaticTasks(project, automaticTask);
		}

		public AutomaticTask AddAutomaticTasks(IProject project, IAutomaticTask task)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.AutomaticTask automaticTask = new Sdl.ProjectApi.Implementation.Xml.AutomaticTask();
			TaskId id = ((ITaskBase)task).Id;
			automaticTask.Guid = ((TaskId)(ref id)).ToGuidArray()[0];
			automaticTask.ExternalId = ((IScheduledTask)task).ExternalId;
			automaticTask.CreatedAt = ((ITaskBase)task).CreatedAt.ToUniversalTime();
			automaticTask.CreatedBy = ((ITaskBase)task).CreatedBy.UserId;
			AddUser(((ITaskBase)task).CreatedBy);
			if (((IScheduledTask)task).StartedAt > DateTime.MinValue)
			{
				automaticTask.StartedAt = ((IScheduledTask)task).StartedAt.ToUniversalTime();
				automaticTask.StartedAtSpecified = true;
			}
			if (((IScheduledTask)task).CompletedAt > DateTime.MinValue)
			{
				automaticTask.CompletedAt = ((IScheduledTask)task).CompletedAt.ToUniversalTime();
				automaticTask.CompletedAtSpecified = true;
			}
			automaticTask.PercentComplete = ((IScheduledTask)task).PercentComplete;
			automaticTask.Status = TaskStatus.Created;
			ITaskTemplate[] taskTemplates = ((ITaskBase)task).TaskTemplates;
			foreach (ITaskTemplate val in taskTemplates)
			{
				automaticTask.TaskTemplateIds.Add(val.Id);
			}
			return AddAutomaticTasks(project, automaticTask);
		}

		public List<IUser> GetUsers()
		{
			return XmlProject.Users.Select((Sdl.ProjectApi.Implementation.Xml.User xmlUser) => (IUser)(object)new User(xmlUser)).ToList();
		}

		public void RemoveLanguageFile(ILanguageFile languageFile)
		{
			if (languageFile is LanguageFile languageFile2)
			{
				RemoveXmlLanguageFile(languageFile2.XmlLanguageFile);
			}
		}

		private void RemoveXmlLanguageFile(Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			if (_languageFileCache.GetXmlLanguageFile(xmlLanguageFile.Guid, out var xmlProjectFile, out xmlLanguageFile))
			{
				_languageFileCache.Remove(xmlLanguageFile.Guid);
				RemoveLanguageFileAssociatedTasks(xmlLanguageFile.Guid);
				RemoveMergedXmlLanguageFile(xmlLanguageFile);
				xmlProjectFile.LanguageFiles.Remove(xmlLanguageFile);
				if (xmlProjectFile.LanguageFiles.Count == 0)
				{
					RemoveProjectFile(xmlProjectFile);
				}
			}
		}

		private void RemoveMergedXmlLanguageFile(Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			if (xmlLanguageFile.MergeStateSpecified)
			{
				foreach (LanguageFileRef childFile in xmlLanguageFile.ChildFiles)
				{
					_mergedFilesCache.RemoveFromMergedFileIndex(xmlLanguageFile.Guid, childFile.LanguageFileGuid);
				}
			}
			List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> childFiles = _mergedFilesCache.GetChildFiles(xmlLanguageFile.Guid);
			if (!childFiles.Any())
			{
				return;
			}
			while (childFiles.Count > 0)
			{
				Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = childFiles[0];
				if (languageFile.MergeState == MergeState.Merged)
				{
					throw new InvalidOperationException("You cannot remove a child file of merged file with MergeState=Merged.");
				}
				int num = languageFile.ChildFiles.FindIndex((LanguageFileRef fileRef) => fileRef.LanguageFileGuid == xmlLanguageFile.Guid);
				if (num != -1)
				{
					if (languageFile.MergeState == MergeState.NotMerged && languageFile.ChildFiles.Count <= 2)
					{
						RemoveXmlLanguageFile(languageFile);
					}
					else
					{
						childFiles.RemoveAt(0);
					}
					languageFile.ChildFiles.RemoveAt(num);
					continue;
				}
				break;
			}
		}

		private void RemoveProjectFile(ProjectFile xmlProjectFile)
		{
			_auxiliaryFileCache.RemoveFile(xmlProjectFile);
			XmlProject.ProjectFiles.Remove(xmlProjectFile);
		}

		private void RemoveLanguageFileAssociatedTasks(Guid languageFileGuid)
		{
			foreach (Task item in XmlProject.Tasks.Items)
			{
				int num = item.Files.FindIndex((Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile) => xmlTaskFile.LanguageFileGuid == languageFileGuid);
				if (num != -1)
				{
					item.Files.RemoveAt(num);
				}
			}
		}

		public void AddProjectPackageCreationOperation(IProjectPackageCreation c)
		{
			if (c is ProjectPackageCreation projectPackageCreation)
			{
				XmlPackageOperations.ProjectPackageCreationOperations.Add(projectPackageCreation.XmlProjectPackageCreation);
			}
		}

		public void AddProjectPackageImportOperation(IProjectPackageImport i)
		{
			if (i is ProjectPackageImport projectPackageImport)
			{
				XmlPackageOperations.ProjectPackageImportOperations.Add(projectPackageImport.XmlProjectPackageImport);
			}
		}

		public void AddReturnPackageCreationOperation(IReturnPackageCreation c)
		{
			if (c is ReturnPackageCreation returnPackageCreation)
			{
				XmlPackageOperations.ReturnPackageCreationOperations.Add(returnPackageCreation.XmlReturnPackageCreation);
			}
		}

		public void AddReturnPackageImportOperation(IReturnPackageImport i)
		{
			if (i is ReturnPackageImport returnPackageImport)
			{
				XmlPackageOperations.ReturnPackageImportOperations.Add(returnPackageImport.XmlReturnPackageImport);
			}
		}

		public List<IProjectPackageCreation> LoadProjectPackageCreations(IProject project, IProjectPackageInitializer packageInitializer)
		{
			List<IProjectPackageCreation> list = new List<IProjectPackageCreation>();
			foreach (Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreationOperation in XmlPackageOperations.ProjectPackageCreationOperations)
			{
				ProjectPackageCreation item = new ProjectPackageCreation(project, projectPackageCreationOperation, packageInitializer, PathUtil);
				list.Add((IProjectPackageCreation)(object)item);
			}
			return list;
		}

		public List<IProjectPackageImport> LoadProjectPackageImports(IProject project)
		{
			List<IProjectPackageImport> list = new List<IProjectPackageImport>();
			foreach (Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport projectPackageImportOperation in XmlPackageOperations.ProjectPackageImportOperations)
			{
				ProjectPackageImport item = new ProjectPackageImport(project, projectPackageImportOperation, PathUtil);
				list.Add((IProjectPackageImport)(object)item);
			}
			return list;
		}

		public List<IReturnPackageCreation> LoadReturnPackageCreations(IProject project, IProjectPackageInitializer packageInitializer)
		{
			List<IReturnPackageCreation> list = new List<IReturnPackageCreation>();
			foreach (Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation returnPackageCreationOperation in XmlPackageOperations.ReturnPackageCreationOperations)
			{
				ReturnPackageCreation item = new ReturnPackageCreation(project, returnPackageCreationOperation, packageInitializer, PathUtil);
				list.Add((IReturnPackageCreation)(object)item);
			}
			return list;
		}

		public List<IReturnPackageImport> LoadReturnPackageImports(IProject project, IProjectPackageInitializer packageInitializer)
		{
			List<IReturnPackageImport> list = new List<IReturnPackageImport>();
			foreach (Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport returnPackageImportOperation in XmlPackageOperations.ReturnPackageImportOperations)
			{
				ReturnPackageImport item = new ReturnPackageImport(project, returnPackageImportOperation, packageInitializer, PathUtil);
				list.Add((IReturnPackageImport)(object)item);
			}
			return list;
		}

		public Dictionary<string, string> GetPhysicalFiles(IProject project)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (ProjectFile projectFile in XmlProject.ProjectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					foreach (FileVersion fileVersion in languageFile.FileVersions)
					{
						string fileName = PathUtil.MakeAbsolutePath(project, fileVersion.PhysicalPath, true);
						FileInfo fileInfo = new FileInfo(fileName);
						if (!dictionary.ContainsKey(fileInfo.FullName) && fileInfo.Exists)
						{
							dictionary[fileInfo.FullName] = fileVersion.PhysicalPath;
						}
					}
				}
			}
			return dictionary;
		}

		internal LanguageFile AddProjectFile(IProject project, Guid projectFileId, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			ProjectFile xmlProjectFile = GetXmlProjectFile(projectFileId);
			AddXmlLanguageFile(xmlProjectFile, xmlLanguageFile);
			xmlLanguageFile.ChildFiles?.ForEach(delegate(LanguageFileRef f)
			{
				_mergedFilesCache.AddToMergedFileIndex(f.LanguageFileGuid, xmlLanguageFile);
			});
			return GetProjectFile(project, xmlLanguageFile.Guid) as LanguageFile;
		}

		protected void AddXmlLanguageFile(ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			xmlProjectFile.LanguageFiles.Add(xmlLanguageFile);
			_languageFileCache.Add(xmlProjectFile, xmlLanguageFile);
		}
	}
}
