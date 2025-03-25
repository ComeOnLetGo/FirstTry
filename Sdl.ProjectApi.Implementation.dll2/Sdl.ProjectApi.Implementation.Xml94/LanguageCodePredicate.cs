using System;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal class LanguageCodePredicate
	{
		private readonly string _languageCode;

		public LanguageCodePredicate(string languageCode)
		{
			_languageCode = languageCode;
		}

		public bool MatchLanguage(LanguageFile languageFile)
		{
			return string.Equals(languageFile.LanguageCode, _languageCode, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
