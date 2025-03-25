using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Settings;
using Sdl.ProjectApi.Settings.SettingTypes;

namespace Sdl.ProjectApi.Implementation.ProjectSettings
{
	public class FilesMetadataSettingsManager : IFilesMetadataSettingsManager
	{
		private readonly IProject _project;

		private readonly object _syncRoot = new object();

		public ISettingsBundle ProjectSettings
		{
			get
			{
				return ((IObjectWithSettings)_project).Settings;
			}
			set
			{
				((IObjectWithSettings)_project).Settings = value;
			}
		}

		public FilesMetadataSettingsManager(IProject project)
		{
			_project = project;
		}

		public void SetFileMetadata(Guid fileId, int version, string origin, string timeStamp, uint crcValue, string originalBcmDoucmentPath)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected O, but got Unknown
			lock (_syncRoot)
			{
				LanguageCloudFilesSettings settingsGroup = ProjectSettings.GetSettingsGroup<LanguageCloudFilesSettings>();
				if (settingsGroup.FilesMetadata == null)
				{
					settingsGroup.FilesMetadata = new FileMetadataItemList();
				}
				FileMetadataItemList filesMetadata = settingsGroup.FilesMetadata;
				FileMetadataItem val = ((IEnumerable<FileMetadataItem>)filesMetadata).FirstOrDefault((FileMetadataItem fileMetadata) => fileMetadata.Id == fileId);
				if (val == null)
				{
					((List<FileMetadataItem>)(object)filesMetadata).Add(new FileMetadataItem
					{
						Id = fileId,
						Version = version,
						Origin = origin,
						TimeStamp = timeStamp,
						CrcValue = crcValue,
						OriginalBcmDocumentPath = originalBcmDoucmentPath
					});
				}
				else
				{
					val.Id = fileId;
					val.Version = version;
					val.Origin = origin;
					val.TimeStamp = timeStamp;
					val.CrcValue = crcValue;
					val.OriginalBcmDocumentPath = originalBcmDoucmentPath;
				}
				settingsGroup.FilesMetadata = filesMetadata;
				((IProjectConfiguration)_project).Save();
			}
		}

		public FileMetadataItem GetFileMetadata(Guid fileId)
		{
			lock (_syncRoot)
			{
				LanguageCloudFilesSettings settingsGroup = ProjectSettings.GetSettingsGroup<LanguageCloudFilesSettings>();
				if (settingsGroup.FilesMetadata == null)
				{
					return null;
				}
				FileMetadataItemList filesMetadata = settingsGroup.FilesMetadata;
				return ((IEnumerable<FileMetadataItem>)filesMetadata).FirstOrDefault((FileMetadataItem fileMetadata) => fileMetadata.Id == fileId);
			}
		}
	}
}
