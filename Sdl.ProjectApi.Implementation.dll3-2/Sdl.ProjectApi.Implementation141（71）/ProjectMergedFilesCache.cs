using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectMergedFilesCache
	{
		private readonly Dictionary<Guid, List<Sdl.ProjectApi.Implementation.Xml.LanguageFile>> _xmlMergedFileIndex;

		public ProjectMergedFilesCache()
		{
			_xmlMergedFileIndex = new Dictionary<Guid, List<Sdl.ProjectApi.Implementation.Xml.LanguageFile>>();
		}

		public ProjectMergedFilesCache(List<ProjectFile> projectFiles)
			: this()
		{
			LoadMergedFilesCache(projectFiles);
		}

		private void LoadMergedFilesCache(List<ProjectFile> projectFiles)
		{
			foreach (ProjectFile projectFile in projectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if (!languageFile.MergeStateSpecified)
					{
						continue;
					}
					foreach (LanguageFileRef childFile in languageFile.ChildFiles)
					{
						AddToMergedFileIndex(childFile.LanguageFileGuid, languageFile);
					}
				}
			}
		}

		public void AddToMergedFileIndex(Guid languageFileGuid, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlMergedLanguageFile)
		{
			if (!_xmlMergedFileIndex.TryGetValue(languageFileGuid, out var value))
			{
				value = new List<Sdl.ProjectApi.Implementation.Xml.LanguageFile>();
				_xmlMergedFileIndex[languageFileGuid] = value;
			}
			value.Add(xmlMergedLanguageFile);
		}

		public void RemoveFromMergedFileIndex(Guid mergedLanguageFileGuid, Guid childLanguageFileGuid)
		{
			if (!_xmlMergedFileIndex.TryGetValue(childLanguageFileGuid, out var value))
			{
				return;
			}
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].Guid == mergedLanguageFileGuid)
				{
					value.RemoveAt(i);
					break;
				}
			}
			if (value.Count == 0)
			{
				_xmlMergedFileIndex.Remove(childLanguageFileGuid);
			}
		}

		public List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> GetChildFiles(Guid languageFileGuid)
		{
			List<Sdl.ProjectApi.Implementation.Xml.LanguageFile> value = new List<Sdl.ProjectApi.Implementation.Xml.LanguageFile>();
			if (!_xmlMergedFileIndex.TryGetValue(languageFileGuid, out value))
			{
				return new List<Sdl.ProjectApi.Implementation.Xml.LanguageFile>();
			}
			return value;
		}
	}
}
