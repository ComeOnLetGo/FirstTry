using System;
using Sdl.Core.Globalization;

namespace Sdl.ProjectApi.Implementation
{
	public class LanguageIndexMapping : ILanguageIndexMapping
	{
		private readonly Language _language;

		private readonly string _index;

		public Language Language => _language;

		public string Index => _index;

		public LanguageIndexMapping(Language language, string index)
		{
			if (language == null)
			{
				throw new ArgumentNullException("language");
			}
			_language = language;
			_index = index;
		}
	}
}
