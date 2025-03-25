using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	internal class DefaultProjectPackageImportEvents : IPackageImportEvents
	{
		public string GetLocalDataFolderForNewProject(IProjectPackageImport projectPackageImport, IProjectsProvider projectsProvider, string projectName)
		{
			string text = Path.Combine(projectsProvider.LocalDataFolder, projectName);
			if (!Directory.Exists(text))
			{
				return text;
			}
			int num = 1;
			while (Directory.Exists(text = Path.Combine(projectsProvider.LocalDataFolder, projectName + "_" + num)))
			{
				num++;
			}
			return text;
		}

		public OverwriteFileEventResult ShouldOverwriteLocalizableFile(IProject project, ILocalizableFile localizableFile)
		{
			return (OverwriteFileEventResult)1;
		}

		public OverwriteFileEventResult ShouldOverwriteAuxiliaryFile(IProject project, ITranslatableFile translatableFile, IAuxiliaryFile auxiliaryFile)
		{
			return (OverwriteFileEventResult)1;
		}

		public bool ShouldImportPackageAgain(IPackageImport projectPackageImport)
		{
			return false;
		}
	}
}
