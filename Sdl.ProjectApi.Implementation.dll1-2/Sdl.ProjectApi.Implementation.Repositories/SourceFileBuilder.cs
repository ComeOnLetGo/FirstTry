using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.LanguageCloud.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class SourceFileBuilder : FileBuilderBase
	{
		public ProjectFile CreateTranslatableSourceProjectFile(LightFile sourceFile)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			int versionIndex = 0;
			ProjectFile projectFile = AddOrUpdateSourceFileForLC(sourceFile);
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = CreateXmlLanguageFileForLC(new Language(sourceFile.LanguageCode), Guid.Parse(sourceFile.Id));
			FileVersion val = sourceFile.Versions.Where((FileVersion v) => (int)v.Type == 0).FirstOrDefault();
			if (val != null)
			{
				Guid latestFileVersionId = Guid.Parse(val.FileId);
				FileVersion item = CreateXmlLanguageFileVersionForLC(latestFileVersionId, sourceFile.Name, sourceFile.LanguageCode + "\\" + sourceFile.Name, ++versionIndex);
				languageFile.FileVersions.Add(item);
			}
			if ((int)sourceFile.Role == 0)
			{
				foreach (FileVersion version in sourceFile.Versions)
				{
					if (!languageFile.FileVersions.Exists((FileVersion v) => v.Guid.Equals(Guid.Parse(version.FileId))))
					{
						AddSourceFileVersion(sourceFile, languageFile, version, ref versionIndex);
					}
				}
			}
			projectFile.LanguageFiles.Add(languageFile);
			return projectFile;
		}

		public void UpdateSourceTranslatableProjectFile(LightFile sourceFile, ProjectFile xmlProjectFile)
		{
			xmlProjectFile.FilterDefinitionId = sourceFile.FileTypeDefinition.Id;
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = xmlProjectFile.LanguageFiles.Where((Sdl.ProjectApi.Implementation.Xml.LanguageFile lf) => lf.LanguageCode.Equals(sourceFile.LanguageCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (languageFile != null)
			{
				if (sourceFile.Versions.Count > languageFile.FileVersions.Count)
				{
					UpdateSourceFileVersions(sourceFile, languageFile);
				}
				if (sourceFile.Versions.Count > 1)
				{
					languageFile.FileVersions.LastOrDefault().Guid = Guid.Parse(sourceFile.LatestFileVersion);
				}
			}
		}

		public void UpdateReferenceProjectFile(LightFile sourceFile, ProjectFile xmlProjectFile, ICollection<string> targetLanguages)
		{
			foreach (string language in targetLanguages.Union(new List<string> { sourceFile.LanguageCode }))
			{
				Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = xmlProjectFile.LanguageFiles.Where((Sdl.ProjectApi.Implementation.Xml.LanguageFile lf) => lf.LanguageCode.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				if (languageFile != null)
				{
					languageFile.FileVersions.LastOrDefault().Guid = Guid.Parse(sourceFile.LatestFileVersion);
				}
			}
		}

		public ProjectFile CreateReferenceProjectFile(LightFile file, ICollection<string> targetLanguages)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			Guid guid = Guid.Parse(file.Id);
			Guid guid2 = Guid.Parse(file.LatestFileVersion);
			ProjectFile obj = new ProjectFile
			{
				Guid = guid,
				Name = file.Name,
				Role = file.MapFileRole(),
				Path = string.Empty
			};
			FileTypeDefinition fileTypeDefinition = file.FileTypeDefinition;
			obj.FilterDefinitionId = ((fileTypeDefinition != null) ? fileTypeDefinition.Id : null);
			ProjectFile projectFile = obj;
			foreach (string item2 in targetLanguages.Union(new List<string> { file.LanguageCode }))
			{
				Language language = new Language(item2);
				Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = projectFile.LanguageFiles.Where((Sdl.ProjectApi.Implementation.Xml.LanguageFile lf) => string.Equals(lf.LanguageCode, ((LanguageBase)language).IsoAbbreviation, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
				if (languageFile == null)
				{
					string filePath = ((LanguageBase)language).IsoAbbreviation + "\\" + file.Name;
					FileVersion latestXmlFileVersion = CreateXmlLanguageFileVersionForLC(Guid.Parse(file.LatestFileVersion), file.Name, filePath, 1);
					Sdl.ProjectApi.Implementation.Xml.LanguageFile item = CreateXmlLanguageFileForLC(language, Guid.Parse(file.Id), latestXmlFileVersion);
					projectFile.LanguageFiles.Add(item);
				}
			}
			return projectFile;
		}

		private ProjectFile AddOrUpdateSourceFileForLC(LightFile file)
		{
			Guid guid = Guid.Parse(file.Id);
			ProjectFile obj = new ProjectFile
			{
				Guid = guid,
				Name = file.Name,
				Role = file.MapFileRole(),
				Path = string.Empty
			};
			FileTypeDefinition fileTypeDefinition = file.FileTypeDefinition;
			obj.FilterDefinitionId = ((fileTypeDefinition != null) ? fileTypeDefinition.Id : null);
			return obj;
		}

		private void UpdateSourceFileVersions(LightFile sourceFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile sourceLanguageFile)
		{
			IEnumerable<FileVersion> enumerable = sourceFile.Versions.Where((FileVersion v) => !sourceLanguageFile.FileVersions.Any((FileVersion sv) => sv.Guid.Equals(Guid.Parse(v.FileId))));
			int versionIndex = sourceLanguageFile.FileVersions.Count;
			foreach (FileVersion item in enumerable)
			{
				AddSourceFileVersion(sourceFile, sourceLanguageFile, item, ref versionIndex);
			}
		}

		private void AddSourceFileVersion(LightFile sourceFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlSourceLanguageFile, FileVersion version, ref int versionIndex)
		{
			FileVersion item = CreateXmlLanguageFileVersionForLC(Guid.Parse(version.FileId), sourceFile.Name.EndsWith(".sdlxliff") ? sourceFile.Name : (sourceFile.Name + ".sdlxliff"), sourceFile.Name.EndsWith(".sdlxliff") ? (sourceFile.LanguageCode + "\\" + sourceFile.Name) : (sourceFile.LanguageCode + "\\" + sourceFile.Name + ".sdlxliff"), ++versionIndex);
			xmlSourceLanguageFile.FileVersions.Add(item);
		}
	}
}
