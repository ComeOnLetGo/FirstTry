using System;
using System.Linq;
using System.Runtime.InteropServices;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation
{
	[Guid("E7F30FEA-DEE1-4BB4-B892-5FD56A87C2AD")]
	public class TranslatableFile : LocalizableFile, ITranslatableFile, ILocalizableFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		public TaskFileType TaskFileType
		{
			get
			{
				if (IsBilingual)
				{
					if (base.IsSource)
					{
						return (TaskFileType)2;
					}
					return (TaskFileType)3;
				}
				if (base.IsSource)
				{
					return (TaskFileType)1;
				}
				return (TaskFileType)4;
			}
		}

		private bool IsBilingual => base.CurrentFilename.EndsWith(((IProjectConfiguration)base.Project).FileTypeConfiguration.FilterManager.DefaultBilingualFileTypeDefinition.FileTypeInformation.DefaultFileExtension, StringComparison.OrdinalIgnoreCase);

		public new ITranslatableFile SourceLanguageFile => (ITranslatableFile)((ILanguageFile)this).SourceLanguageFile;

		public new ITranslatableFile[] TargetLanguageFiles
		{
			get
			{
				ILanguageFile[] targetLanguageFiles = ((ILanguageFile)this).TargetLanguageFiles;
				ITranslatableFile[] array = (ITranslatableFile[])(object)new ITranslatableFile[targetLanguageFiles.Length];
				targetLanguageFiles.CopyTo(array, 0);
				return array;
			}
		}

		public virtual IAnalysisStatistics AnalysisStatistics
		{
			get
			{
				if (base.XmlLanguageFile.AnalysisStatistics == null)
				{
					base.XmlLanguageFile.AnalysisStatistics = new Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics();
				}
				return (IAnalysisStatistics)(object)new Sdl.ProjectApi.Implementation.Statistics.AnalysisStatistics(base.Project, base.LanguageDirection, this, base.XmlLanguageFile.AnalysisStatistics, canUpdate: true);
			}
		}

		public virtual TQAData TQAData
		{
			get
			{
				if (base.XmlLanguageFile.TQAData == null)
				{
					base.XmlLanguageFile.TQAData = new TQAData();
				}
				return base.XmlLanguageFile.TQAData;
			}
		}

		public virtual IConfirmationStatistics ConfirmationStatistics
		{
			get
			{
				if (base.IsSource)
				{
					return null;
				}
				if (base.XmlLanguageFile.ConfirmationStatistics == null)
				{
					base.XmlLanguageFile.ConfirmationStatistics = new Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics();
				}
				return (IConfirmationStatistics)(object)new ConfirmationStatisticsRepository(base.LanguageDirection, this, base.XmlLanguageFile.ConfirmationStatistics, canUpdate: true);
			}
		}

		public IBilingualReferenceFile BilingualReferenceFile
		{
			get
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				if (base.IsSource)
				{
					throw new ProjectApiException(ErrorMessages.TranslatableFile_CannotGetBilingualReferenceFileForSource);
				}
				PreviousBilingualFile xmlPreviousBilingualFile = GetXmlPreviousBilingualFile(base.Language);
				if (xmlPreviousBilingualFile == null)
				{
					return null;
				}
				return (IBilingualReferenceFile)(object)new BilingualReferenceFile(this, xmlPreviousBilingualFile);
			}
			set
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				if (base.IsSource)
				{
					throw new ProjectApiException(ErrorMessages.TranslatableFile_CannotSetBilingualReferenceFileForSource);
				}
				if (value == null)
				{
					RemovePreviousBilingualFile(base.Language);
				}
			}
		}

		private FileVersion XmlOriginalFileVersion
		{
			get
			{
				Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = (base.IsSource ? base.XmlLanguageFile : base.XmlProjectFile.GetLanguageFileByLanguage(((LanguageBase)base.Project.SourceLanguage).IsoAbbreviation));
				if (languageFile == null || languageFile.FileVersions.Count <= 0)
				{
					return null;
				}
				return languageFile.FileVersions[0];
			}
		}

		public IMergedTranslatableFile MergedFile => base.ProjectImpl.GetMergedTranslatableFile(base.Guid);

		public IMergedTranslatableFile[] MergedFileHistory => base.ProjectImpl.GetMergedTranslatableFileHistory(base.Guid).ToArray();

		public TranslatableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: this(project, xmlProjectFile, xmlLanguageFile, (IFile)new FileWrapper(), projectPathUtil)
		{
		}//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown


		public TranslatableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IFile fileWrapper, IProjectPathUtil projectPathUtil)
			: base(project, xmlProjectFile, xmlLanguageFile, fileWrapper, projectPathUtil)
		{
		}

		public IBilingualReferenceFile GetBilingualReferenceFile(Language targetLanguage)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (!base.IsSource && !((object)base.Language).Equals((object)targetLanguage))
			{
				throw new ProjectApiException(ErrorMessages.TranslatableFile_CannotGetBilingualReferenceFilesForTarget);
			}
			PreviousBilingualFile xmlPreviousBilingualFile = GetXmlPreviousBilingualFile(targetLanguage);
			if (xmlPreviousBilingualFile != null)
			{
				return (IBilingualReferenceFile)(object)new BilingualReferenceFile(this, xmlPreviousBilingualFile);
			}
			return null;
		}

		public void RemoveBilingualReferenceFile(Language targetLanguage)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (!base.IsSource && !((object)base.Language).Equals((object)targetLanguage))
			{
				throw new ProjectApiException(ErrorMessages.TranslatableFile_CannotRemoveBilingualReferenceFilesForTarget);
			}
			RemovePreviousBilingualFile(targetLanguage);
		}

		public IFileRevision GetMostRecentBilingualRevision()
		{
			IFileRevision[] revisions = base.Revisions;
			for (int num = revisions.Length - 1; num >= 0; num--)
			{
				if (revisions[num].IsBilingual)
				{
					return revisions[num];
				}
			}
			return null;
		}

		public string GetOriginalFilePath()
		{
			return XmlOriginalFileVersion.PhysicalPath;
		}

		private PreviousBilingualFile GetXmlPreviousBilingualFile(Language language)
		{
			return base.XmlProjectFile.PreviousBilingualFiles.FirstOrDefault((PreviousBilingualFile xmlPreviousBilingualFile) => LanguageBase.Equals(xmlPreviousBilingualFile.TargetLanguageCode, ((LanguageBase)language).IsoAbbreviation));
		}

		internal BilingualReferenceFile AddPreviousBilingualFile(string fileToAdd, bool isReviewed, Language targetLanguage)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (GetXmlPreviousBilingualFile(targetLanguage) != null)
			{
				throw new ProjectApiException(ErrorMessages.TranslatableFile_OnlyOnePreviousBilingualFilePerTargetLanguage);
			}
			PreviousBilingualFile previousBilingualFile = new PreviousBilingualFile
			{
				PhysicalPath = fileToAdd,
				IsReviewed = isReviewed,
				TargetLanguageCode = ((LanguageBase)targetLanguage).IsoAbbreviation
			};
			base.XmlProjectFile.PreviousBilingualFiles.Add(previousBilingualFile);
			return new BilingualReferenceFile(this, previousBilingualFile);
		}

		internal void RemovePreviousBilingualFile(Language language)
		{
			int num = 0;
			foreach (PreviousBilingualFile previousBilingualFile in base.XmlProjectFile.PreviousBilingualFiles)
			{
				if (LanguageBase.Equals(previousBilingualFile.TargetLanguageCode, ((LanguageBase)language).IsoAbbreviation))
				{
					base.XmlProjectFile.PreviousBilingualFiles.RemoveAt(num);
					break;
				}
				num++;
			}
		}

		public virtual void RevertToSDLXLIFF()
		{
			if (base.Project.IsPublished)
			{
				RevertToSDLXLIFFServerProject();
			}
			else
			{
				RevertToSDLXLIFFLocalProject();
			}
		}

		protected virtual void RevertToSDLXLIFFServerProject()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between Unknown and I4
			ICommuteClient val = base.ProjectImpl.CreateCommuteClient();
			LanguageFileVersion[] languageFileVersions = val.GetLanguageFileVersions(base.Guid, false);
			if (!languageFileVersions.Last().FileName.EndsWith(".sdlxliff", StringComparison.OrdinalIgnoreCase))
			{
				if ((int)base.LocalFileState != 2)
				{
					DownloadLatestServerVersion(null, force: true);
				}
				IFileRevision mostRecentBilingualRevision = GetMostRecentBilingualRevision();
				val.GetFileVersion(base.Project.Guid, mostRecentBilingualRevision.LocalFilePath, base.Guid, mostRecentBilingualRevision.RevisionNumber, (EventHandler<FileTransferEventArgs>)null);
				UploadNewVersion(mostRecentBilingualRevision.LocalFilePath, StringResources.RevertToSDLXLIFF);
			}
			else
			{
				IFileRevision mostRecentBilingualRevision2 = GetMostRecentBilingualRevision();
				RollbackToVersion(mostRecentBilingualRevision2);
			}
		}

		protected virtual void RevertToSDLXLIFFLocalProject()
		{
			IFileRevision mostRecentBilingualRevision = GetMostRecentBilingualRevision();
			RollbackToVersion(mostRecentBilingualRevision);
		}
	}
}
