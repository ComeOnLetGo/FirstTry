using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class AuxiliaryFile : LanguageFile, IAuxiliaryFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		public AuxiliaryFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: base(project, xmlProjectFile, xmlLanguageFile, projectPathUtil)
		{
		}
	}
}
