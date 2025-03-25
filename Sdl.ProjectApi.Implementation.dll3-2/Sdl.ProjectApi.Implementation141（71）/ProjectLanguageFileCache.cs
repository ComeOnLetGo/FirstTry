using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectLanguageFileCache
	{
		private class XmlLanguageFileRecord
		{
			public ProjectFile XmlProjectFile;

			public Sdl.ProjectApi.Implementation.Xml.LanguageFile XmlLanguageFile;
		}

		private Dictionary<Guid, XmlLanguageFileRecord> _xmlLanguageFileIndex;

		public ProjectLanguageFileCache()
		{
			_xmlLanguageFileIndex = new Dictionary<Guid, XmlLanguageFileRecord>();
		}

		public ProjectLanguageFileCache(List<ProjectFile> projectFiles)
			: this()
		{
			Load(projectFiles);
		}

		private void Load(List<ProjectFile> projectFiles)
		{
			_xmlLanguageFileIndex = new Dictionary<Guid, XmlLanguageFileRecord>();
			foreach (ProjectFile projectFile in projectFiles)
			{
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile in projectFile.LanguageFiles)
				{
					Add(projectFile, languageFile);
				}
			}
		}

		public bool GetXmlLanguageFile(Guid languageFileGuid, out ProjectFile xmlProjectFile, out Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			XmlLanguageFileRecord value;
			bool flag = _xmlLanguageFileIndex.TryGetValue(languageFileGuid, out value);
			xmlProjectFile = (flag ? value.XmlProjectFile : null);
			xmlLanguageFile = (flag ? value.XmlLanguageFile : null);
			return flag;
		}

		public void Add(ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			XmlLanguageFileRecord xmlLanguageFileRecord = new XmlLanguageFileRecord();
			xmlLanguageFileRecord.XmlProjectFile = xmlProjectFile;
			xmlLanguageFileRecord.XmlLanguageFile = xmlLanguageFile;
			_xmlLanguageFileIndex[xmlLanguageFile.Guid] = xmlLanguageFileRecord;
		}

		public void Add(ProjectFile xmlProjectFile)
		{
			XmlLanguageFileRecord xmlLanguageFileRecord = new XmlLanguageFileRecord();
			xmlLanguageFileRecord.XmlProjectFile = xmlProjectFile;
			xmlLanguageFileRecord.XmlLanguageFile = xmlProjectFile.LanguageFiles.FirstOrDefault();
			_xmlLanguageFileIndex[xmlLanguageFileRecord.XmlLanguageFile.Guid] = xmlLanguageFileRecord;
		}

		public void Remove(Guid languageFileGuid)
		{
			_xmlLanguageFileIndex.Remove(languageFileGuid);
		}
	}
}
