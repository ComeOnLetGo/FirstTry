using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.LanguageCloud.Builders
{
	public class FileBuilderBase
	{
		protected FileVersion CreateXmlLanguageFileVersionForLC(Guid latestFileVersionId, string fileName, string filePath, int version)
		{
			return new FileVersion
			{
				Guid = latestFileVersionId,
				FileName = fileName,
				VersionNumber = version,
				Size = 0L,
				PhysicalPath = filePath,
				CreatedAt = DateTime.UtcNow
			};
		}

		protected Sdl.ProjectApi.Implementation.Xml.LanguageFile CreateXmlLanguageFileForLC(Language targetLanguage, Guid targetLanguageFileGuid, FileVersion latestXmlFileVersion = null)
		{
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = new Sdl.ProjectApi.Implementation.Xml.LanguageFile
			{
				Guid = targetLanguageFileGuid,
				LanguageCode = ((LanguageBase)targetLanguage).IsoAbbreviation,
				SettingsBundleGuid = Guid.NewGuid()
			};
			if (latestXmlFileVersion != null)
			{
				languageFile.FileVersions = new List<FileVersion> { latestXmlFileVersion };
			}
			return languageFile;
		}
	}
}
