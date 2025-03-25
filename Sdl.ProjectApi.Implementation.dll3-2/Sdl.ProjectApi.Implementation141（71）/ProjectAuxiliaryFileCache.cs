using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectAuxiliaryFileCache
	{
		private readonly Dictionary<Guid, List<ProjectFile>> _xmlAuxiliaryFileIndex;

		public ProjectAuxiliaryFileCache()
		{
			_xmlAuxiliaryFileIndex = new Dictionary<Guid, List<ProjectFile>>();
		}

		public ProjectAuxiliaryFileCache(List<ProjectFile> projectFiles)
			: this()
		{
			LoadCache(projectFiles);
		}

		private void LoadCache(List<ProjectFile> projectFiles)
		{
			foreach (ProjectFile projectFile in projectFiles)
			{
				if (projectFile.ParentProjectFileGuid != Guid.Empty)
				{
					AddFile(projectFile.ParentProjectFileGuid, projectFile);
				}
			}
		}

		internal void AddFile(Guid parentProjectFileGuid, ProjectFile auxliaryXmlProjectFile)
		{
			if (!_xmlAuxiliaryFileIndex.TryGetValue(parentProjectFileGuid, out var value))
			{
				value = new List<ProjectFile>();
				_xmlAuxiliaryFileIndex[parentProjectFileGuid] = value;
			}
			value.Add(auxliaryXmlProjectFile);
		}

		internal void RemoveFile(ProjectFile xmlProjectFile)
		{
			if (xmlProjectFile.Role == FileRole.Auxiliary)
			{
				List<ProjectFile> list = _xmlAuxiliaryFileIndex[xmlProjectFile.ParentProjectFileGuid];
				list.Remove(xmlProjectFile);
			}
		}
	}
}
