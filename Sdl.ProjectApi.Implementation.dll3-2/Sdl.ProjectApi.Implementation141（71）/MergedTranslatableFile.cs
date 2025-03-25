using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation
{
	public class MergedTranslatableFile : TranslatableFile, IMergedTranslatableFile, ITranslatableFile, ILocalizableFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		public MergeState MergeState => EnumConvert.ConvertMergeState(base.XmlLanguageFile.MergeState);

		public override IAnalysisStatistics AnalysisStatistics => (IAnalysisStatistics)(object)new CompoundAnalysisStatistics(ChildFiles.Select((ITranslatableFile file) => file.AnalysisStatistics));

		public override IConfirmationStatistics ConfirmationStatistics
		{
			get
			{
				if (!base.IsSource)
				{
					return (IConfirmationStatistics)(object)new CompoundConfirmationStatistics(ChildFiles.Select((ITranslatableFile file) => file.ConfirmationStatistics));
				}
				return null;
			}
		}

		public ITranslatableFile[] ChildFiles
		{
			get
			{
				IEnumerable<ITranslatableFile> source = base.XmlLanguageFile.ChildFiles.Select((LanguageFileRef f) => base.ProjectImpl.GetTranslatableFile(f.LanguageFileGuid));
				return source.ToArray();
			}
			set
			{
			}
		}

		public override string LocalFilePath
		{
			get
			{
				string localFilePath = base.LocalFilePath;
				if (localFilePath != null)
				{
					return localFilePath;
				}
				return base.ProjectPathUtil.MakeAbsolutePath(base.Project, Path.Combine(Path.Combine(base.Project.GetProjectLanguageDirectory(base.Language), base.FolderInProject), base.CurrentFilename), true);
			}
			set
			{
				if (base.HasLatestXmlVersion)
				{
					SetFileVersionPath(base.LatestXmlFileVersion, value, copy: false);
				}
			}
		}

		public MergedTranslatableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: base(project, xmlProjectFile, xmlLanguageFile, projectPathUtil)
		{
		}

		public override void UploadNewVersion(string filePath, string comment)
		{
			if (base.XmlLanguageFile.MergeState == Sdl.ProjectApi.Implementation.Xml.MergeState.Split)
			{
				throw new InvalidOperationException("You cannot upload a new version of this merged file, because it has already been split.");
			}
			base.UploadNewVersion(filePath, comment);
			if (base.XmlLanguageFile.MergeState == Sdl.ProjectApi.Implementation.Xml.MergeState.NotMerged)
			{
				base.XmlLanguageFile.MergeState = Sdl.ProjectApi.Implementation.Xml.MergeState.Merged;
			}
		}

		public override void UploadNewVersion(Stream inputStream, DateTime lastChangeTimestamp, string comment)
		{
			if (base.XmlLanguageFile.MergeState == Sdl.ProjectApi.Implementation.Xml.MergeState.Split)
			{
				throw new InvalidOperationException("You cannot upload a new version of this merged file, because it has already been split.");
			}
			base.UploadNewVersion(inputStream, lastChangeTimestamp, comment);
			if (base.XmlLanguageFile.MergeState == Sdl.ProjectApi.Implementation.Xml.MergeState.NotMerged)
			{
				base.XmlLanguageFile.MergeState = Sdl.ProjectApi.Implementation.Xml.MergeState.Merged;
			}
		}

		public void Split(string[] childFilePaths)
		{
			if (base.XmlLanguageFile.MergeState != Sdl.ProjectApi.Implementation.Xml.MergeState.Merged)
			{
				throw new InvalidOperationException("This file cannot be split, because its MergeState is different to MergeState.Merged.");
			}
			if (childFilePaths == null)
			{
				throw new ArgumentNullException("childFilePaths");
			}
			ITranslatableFile[] childFiles = ChildFiles;
			if (childFiles.Length != childFilePaths.Length)
			{
				throw new ArgumentException("The number of file paths specified must be identical to the number of child files.", "childFilePaths");
			}
			for (int i = 0; i < childFilePaths.Length; i++)
			{
				if (string.IsNullOrEmpty(childFilePaths[i]))
				{
					throw new ArgumentNullException("childFilePaths", "The array of child file paths contain null or empty paths.");
				}
				((ILanguageFile)childFiles[i]).UploadNewVersion(childFilePaths[i], string.Empty);
			}
			base.XmlLanguageFile.MergeState = Sdl.ProjectApi.Implementation.Xml.MergeState.Split;
		}

		public void Split()
		{
			if (base.XmlLanguageFile.MergeState != Sdl.ProjectApi.Implementation.Xml.MergeState.Merged)
			{
				throw new InvalidOperationException("This file cannot be split, because its MergeState is different to MergeState.Merged.");
			}
			base.XmlLanguageFile.MergeState = Sdl.ProjectApi.Implementation.Xml.MergeState.Split;
		}

		public void UnSplit()
		{
			if (base.XmlLanguageFile.MergeState != Sdl.ProjectApi.Implementation.Xml.MergeState.Split)
			{
				throw new InvalidOperationException("This file cannot be unsplit, because its MergeState is different to MergeState.Split.");
			}
			base.XmlLanguageFile.MergeState = Sdl.ProjectApi.Implementation.Xml.MergeState.Merged;
		}

		protected override void RevertToSDLXLIFFServerProject()
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Invalid comparison between Unknown and I4
			ICommuteClient val = base.ProjectImpl.CreateCommuteClient();
			ITranslatableFile val2 = ChildFiles.First();
			LanguageFileVersion val3 = val.GetLanguageFileVersions(((IProjectFile)val2).Guid, false).Last();
			LanguageFileVersion val4 = val.GetLanguageFileVersions(base.Guid, false).Last();
			if (val3.LastModified > val4.LastModified)
			{
				ITranslatableFile[] childFiles = ChildFiles;
				foreach (ITranslatableFile val5 in childFiles)
				{
					if ((int)((IProjectFile)val5).LocalFileState == 1)
					{
						((IProjectFile)val5).DownloadLatestServerVersion((EventHandler<DataTransferEventArgs>)null, true);
					}
				}
				val.GetFileVersion(base.Project.Guid, LocalFilePath, base.Guid, val4.VersionNumber, (EventHandler<FileTransferEventArgs>)null);
				UnSplit();
				UploadNewVersion(LocalFilePath, StringResources.RevertToSDLXLIFF);
			}
			else
			{
				UnSplit();
			}
		}

		protected override void RevertToSDLXLIFFLocalProject()
		{
			UnSplit();
		}
	}
}
