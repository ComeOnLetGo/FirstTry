using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class FileRevision : AbstractProjectItem, IFileRevision
	{
		private readonly LanguageFile _languageFile;

		private readonly FileVersion _xmlFileRevision;

		public ILanguageFile CurrentFile => (ILanguageFile)(object)_languageFile;

		public int RevisionNumber => _xmlFileRevision.VersionNumber;

		public long Size => _xmlFileRevision.Size;

		public Guid Guid => _xmlFileRevision.Guid;

		public bool IsBilingual => IsBilingualFileVersion(base.Project, _xmlFileRevision.FileName);

		public DateTime Date
		{
			get
			{
				return _xmlFileRevision.CreatedAt.ToLocalTime();
			}
			set
			{
				_xmlFileRevision.CreatedAt = value.ToUniversalTime();
			}
		}

		public DateTime LastModified
		{
			get
			{
				return _xmlFileRevision.FileTimeStamp.ToLocalTime();
			}
			set
			{
				_xmlFileRevision.FileTimeStamp = value.ToUniversalTime();
			}
		}

		public string Filename => _xmlFileRevision.FileName;

		public IUser CreatedBy
		{
			get
			{
				return base.ProjectImpl.GetUserById(_xmlFileRevision.CreatedBy);
			}
			set
			{
				_xmlFileRevision.CreatedBy = value.UserId;
			}
		}

		public string Comment
		{
			get
			{
				return _xmlFileRevision.Comment;
			}
			set
			{
				_xmlFileRevision.Comment = value;
			}
		}

		public string LocalFilePath
		{
			get
			{
				return _languageFile.GetFileVersionPath(_xmlFileRevision);
			}
			set
			{
				_languageFile.SetFileVersionPath(_xmlFileRevision, value, copy: false);
			}
		}

		public FileRevision(LanguageFile languageFile, FileVersion xmlFileRevision)
			: base(languageFile.Project)
		{
			_languageFile = languageFile;
			_xmlFileRevision = xmlFileRevision;
		}

		internal static bool IsBilingualFileVersion(IProject project, string fileName)
		{
			return fileName.EndsWith(((IProjectConfiguration)project).FileTypeConfiguration.FilterManager.DefaultBilingualFileTypeDefinition.FileTypeInformation.DefaultFileExtension, StringComparison.OrdinalIgnoreCase);
		}

		public string Download(string downloadRootFolder)
		{
			return _languageFile.DownloadFileVersion(_xmlFileRevision, downloadRootFolder);
		}
	}
}
