using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectFileFactory
	{
		private readonly IProjectPathUtil _projectPathUtil;

		public ProjectFileFactory(IProjectPathUtil projectPathUtil)
		{
			_projectPathUtil = projectPathUtil;
		}

		public ILocalizableFile CreateLocalizableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			if (xmlProjectFile.Role == FileRole.Translatable)
			{
				return (ILocalizableFile)(object)CreateTranslatableFile(project, xmlProjectFile, xmlLanguageFile);
			}
			return (ILocalizableFile)(object)new LocalizableFile(project, xmlProjectFile, xmlLanguageFile, _projectPathUtil);
		}

		public ILanguageFile CreateLanguageFile(IProject project, ProjectFile projectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			FileRole val = EnumConvert.ConvertFileRole(projectFile.Role);
			if ((int)val == 1 || (int)val == 0)
			{
				return (ILanguageFile)(object)CreateTranslatableFile(project, projectFile, languageFile);
			}
			if ((int)val == 3)
			{
				return (ILanguageFile)(object)new LocalizableFile(project, projectFile, languageFile, _projectPathUtil);
			}
			if ((int)val == 2)
			{
				return (ILanguageFile)(object)new ReferenceFile(project, projectFile, languageFile, _projectPathUtil);
			}
			return null;
		}

		public ITranslatableFile CreateTranslatableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile)
		{
			if (xmlLanguageFile.MergeStateSpecified)
			{
				return (ITranslatableFile)(object)new MergedTranslatableFile(project, xmlProjectFile, xmlLanguageFile, _projectPathUtil);
			}
			return (ITranslatableFile)(object)new TranslatableFile(project, xmlProjectFile, xmlLanguageFile, _projectPathUtil);
		}

		public ILanguageFile CreateProjectFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, List<ProjectFile> projectFiles)
		{
			ILanguageFile val = CreateLanguageFile(project, xmlProjectFile, xmlLanguageFile);
			if (val == null)
			{
				if (xmlProjectFile.Role == FileRole.Auxiliary)
				{
					ProjectFile xmlProjectFile2 = GetXmlProjectFile(xmlProjectFile.ParentProjectFileGuid, projectFiles);
					Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFileByLanguage = xmlProjectFile2.GetLanguageFileByLanguage(xmlLanguageFile.LanguageCode);
					IProjectFile projectFile = project.GetProjectFile(languageFileByLanguage.Guid);
					return (ILanguageFile)(object)new AuxiliaryFile(projectFile.Project, xmlProjectFile, xmlLanguageFile, _projectPathUtil);
				}
				throw new Exception("Unexpected file role: " + xmlProjectFile.Role);
			}
			return val;
		}

		internal ProjectFile GetXmlProjectFile(Guid projectFileGuid, List<ProjectFile> projectFiles)
		{
			foreach (ProjectFile projectFile in projectFiles)
			{
				if (projectFile.Guid == projectFileGuid)
				{
					return projectFile;
				}
			}
			return null;
		}
	}
}
