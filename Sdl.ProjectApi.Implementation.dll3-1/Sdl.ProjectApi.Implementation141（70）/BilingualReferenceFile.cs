using System;
using System.Collections.Generic;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;

namespace Sdl.ProjectApi.Implementation
{
	public class BilingualReferenceFile : IBilingualReferenceFile, IReferenceFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		private readonly TranslatableFile _translatableFile;

		private readonly PreviousBilingualFile _xmlPreviousBilingualFile;

		public ITranslatableFile TranslatableFile => (ITranslatableFile)(object)_translatableFile;

		public bool IsReviewed
		{
			get
			{
				return _xmlPreviousBilingualFile.IsReviewed;
			}
			set
			{
				_xmlPreviousBilingualFile.IsReviewed = value;
			}
		}

		public IProject Project { get; private set; }

		public Guid Guid => _translatableFile.Guid;

		public string FolderInProject => _translatableFile.FolderInProject;

		public Language Language
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				return new Language(_xmlPreviousBilingualFile.TargetLanguageCode);
			}
			set
			{
				throw new InvalidOperationException("Cannot change the language of a bilingual reference file.");
			}
		}

		public ILanguageDirection LanguageDirection
		{
			get
			{
				if (!IsSource)
				{
					return Project.GetLanguageDirection(Language);
				}
				return null;
			}
		}

		public bool IsSource => false;

		public ILanguageFile SourceLanguageFile => null;

		public ILanguageFile[] TargetLanguageFiles => null;

		public bool HasTargetLanguageFiles => false;

		public FileRole FileRole
		{
			get
			{
				return (FileRole)2;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				throw new ProjectApiException("You cannot set the FileRole of a bilingual reference file; it is always 'Reference'.");
			}
		}

		public IFileTypeDefinition FilterDefinition
		{
			get
			{
				return _translatableFile.FilterDefinition;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				throw new ProjectApiException("You cannot set the FileTypeInformation of a bilingual reference file.");
			}
		}

		public string FileTypeDefinitionId
		{
			get
			{
				return _translatableFile.FileTypeDefinitionId;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				throw new ProjectApiException("You cannot set the FileTypeInformation of a bilingual reference file.");
			}
		}

		public string FilenameInProject => _translatableFile.PathInProject + Path.GetFileName(PhysicalPath);

		public string PathInProject => _translatableFile.PathInProject;

		public string Filename => Path.GetFileName(PhysicalPath);

		public DateTime LastChanged => File.GetLastWriteTime(PhysicalPath);

		public long Size => new FileInfo(PhysicalPath).Length;

		public string LocalFilePath
		{
			get
			{
				return PhysicalPath;
			}
			set
			{
				throw new InvalidOperationException("Cannot change the local file path of a bilingual reference file.");
			}
		}

		internal string PhysicalPath => _xmlPreviousBilingualFile.PhysicalPath;

		public Language[] SpecificTargetLanguages
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public ITaskFile[] TaskFileHistory
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public LocalFileState LocalFileState
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public DateTime? CheckedOutAt
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public IUser CheckedOutTo
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public bool CheckedOutToMe
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public bool IsCheckedOutOnline
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public List<IFileAssignmentInfo> FileAssignments { get; private set; }

		public ISettingsBundle Settings
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public string CurrentFilename
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IFileRevision LatestRevision
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IFileRevision[] Revisions
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IFileRevision LatestServerRevision
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IFileRevision CurrentServerRevision
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool CheckedOut
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool CheckedOutToAnotherUser
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Guid ProjectFileGuid => _translatableFile.XmlProjectFile.Guid;

		internal BilingualReferenceFile(TranslatableFile translatableFile, PreviousBilingualFile xmlPreviousBilingualFile)
		{
			_translatableFile = translatableFile;
			_xmlPreviousBilingualFile = xmlPreviousBilingualFile;
			Project = translatableFile.Project;
		}

		public void RemoveFromProject(bool deletePhisycalFile = true)
		{
			_translatableFile.RemovePreviousBilingualFile(Language);
		}

		public string DownloadLatestVersion(string downloadRootFolder)
		{
			string text = Path.Combine(downloadRootFolder, Path.GetFileName(PhysicalPath));
			Util.CopyFile(PhysicalPath, text);
			return text;
		}

		public Stream GetLatestVersionDownloadStream()
		{
			return File.OpenRead(PhysicalPath);
		}

		public bool DownloadLatestServerVersion(EventHandler<DataTransferEventArgs> progressEventHandler, bool force)
		{
			throw new NotSupportedException("Download of the latest version of a bilingual reference file from project server is not currently supported.");
		}

		public void UploadNewVersion(string filePath, string comment)
		{
			throw new NotImplementedException();
		}

		public void UploadNewVersion(Stream inputStream, DateTime lastChangeTimestamp, string comment)
		{
			throw new NotImplementedException();
		}

		public void UploadNewVersion(Stream inputStream, string fileName, DateTime lastChangeTimestamp, string comment, bool updateRevisions = true)
		{
			throw new NotImplementedException();
		}

		public void RollbackToVersion(IFileRevision fileRevision)
		{
			throw new NotImplementedException();
		}

		public void ForceKeepLocalChanges()
		{
			throw new NotImplementedException();
		}

		public void UpdateFileSize(long size)
		{
		}
	}
}
