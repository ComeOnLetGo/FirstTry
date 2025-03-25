using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class LanguageFileWithInfo
	{
		public LanguageFile LanguageFile { get; set; }

		public LanguageFileInfo LanguageFileInfo { get; set; }

		public LanguageFileWithInfo(LanguageFile languageFile, LanguageFileInfo languageFileInfo)
		{
			LanguageFile = languageFile;
			LanguageFileInfo = languageFileInfo;
		}
	}
}
