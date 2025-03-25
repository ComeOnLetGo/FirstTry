using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ReferenceFile : LanguageFile, IReferenceFile, ILanguageFile, IProjectFile, IObjectWithSettings
	{
		public ReferenceFile(IProject project, ProjectFile xmlProjectFile, Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, IProjectPathUtil projectPathUtil)
			: base(project, xmlProjectFile, xmlLanguageFile, projectPathUtil)
		{
		}
	}
}
