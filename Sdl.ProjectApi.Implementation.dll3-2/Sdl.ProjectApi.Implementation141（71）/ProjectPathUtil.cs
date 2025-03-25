using System.IO;
using Sdl.Core.Globalization;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectPathUtil : IProjectPathUtil
	{
		public string MakeRelativePath(string path, string localDataFolder)
		{
			string fullPath = Path.GetFullPath(path);
			fullPath = fullPath.Replace('/', '\\');
			string text = NormalizeFullPath(localDataFolder);
			if (!fullPath.ToLower().StartsWith(text.ToLower()))
			{
				return fullPath;
			}
			return fullPath.Substring(text.Length);
		}

		public string MakeAbsolutePath(string path, string localDataFolder)
		{
			if (!Path.IsPathRooted(path))
			{
				return Path.Combine(localDataFolder, path);
			}
			return path;
		}

		public string GetProjectNameFromFilePath(string projectFilePath)
		{
			string fileName = Path.GetFileName(projectFilePath);
			return fileName?.Substring(0, fileName.Length - FileTypes.ProjectFileExtension.Length);
		}

		public string NormalizeFullPath(string fullPath)
		{
			string fullPath2 = Path.GetFullPath(fullPath);
			fullPath2 = fullPath2.Replace('/', '\\');
			if (!fullPath2.EndsWith("\\"))
			{
				fullPath2 += "\\";
			}
			return fullPath2;
		}

		public string NormalizeFolder(string folder)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			string text = folder.Replace('/', '\\');
			if (text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				throw new ProjectApiException("The specified project folder contains invalid characters.");
			}
			if (text != "\\")
			{
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
				if (text.StartsWith("\\"))
				{
					text = text.Substring(1);
				}
			}
			else
			{
				text = string.Empty;
			}
			return text;
		}

		public void ValidateAbsolutePath(string absoluteFilePath, bool validateExtensions)
		{
			string text = string.Empty;
			if (validateExtensions)
			{
				int count = "temp\\".Length + ".sdlxliff".Length + ".VerificationReport.xml".Length;
				text = new string('a', count);
			}
			string fullPath = Path.GetFullPath(absoluteFilePath + text);
		}

		public bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		public string GetProjectLanguageDirectory(bool isInPlace, string projectFilePath, string localDataFolder, Language language)
		{
			if (!isInPlace)
			{
				return Path.Combine(localDataFolder, ((LanguageBase)language).IsoAbbreviation);
			}
			return Path.GetDirectoryName(projectFilePath);
		}

		public string MakeRelativePath(IProject project, string path, bool isProjectFile = false)
		{
			string directoryName = Path.GetDirectoryName(project.ProjectFilePath);
			string localDataFolder = ((isProjectFile && project.IsInPlace) ? directoryName : project.LocalDataFolder);
			return MakeRelativePath(path, localDataFolder);
		}

		public string MakeAbsolutePath(IProject project, string path, bool isProjectFile)
		{
			string directoryName = Path.GetDirectoryName(project.ProjectFilePath);
			string localDataFolder = ((isProjectFile && project.IsInPlace) ? directoryName : project.LocalDataFolder);
			return MakeAbsolutePath(path, localDataFolder);
		}
	}
}
