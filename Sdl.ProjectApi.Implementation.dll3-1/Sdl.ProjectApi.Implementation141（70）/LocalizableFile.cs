using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class LocalizableFile : LanguageFile, ILocalizableFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		public LocalizableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: this(project, xmlProjectFile, xmlLanguageFile, (IFile)new FileWrapper(), projectPathUtil)
		{
		}//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown


		public LocalizableFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IFile fileWrapper, IProjectPathUtil projectPathUtil)
			: base(project, xmlProjectFile, xmlLanguageFile, fileWrapper, projectPathUtil)
		{
		}
	}
}
