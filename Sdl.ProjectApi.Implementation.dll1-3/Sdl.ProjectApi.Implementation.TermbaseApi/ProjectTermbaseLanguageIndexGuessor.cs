using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sdl.Core.Globalization;
using Sdl.Desktop.Logger;
using Sdl.MultiTerm.Core.Common;
using Sdl.MultiTerm.Core.Common.Interfaces;
using Sdl.MultiTerm.Core.Utilities;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseLanguageIndexGuessor
	{
		private readonly Lazy<IDictionary<string, string>> _languageIndexNameDictionary;

		private readonly ProjectTermbaseConfigurationFactory _factory = new ProjectTermbaseConfigurationFactory();

		private readonly ILogger _logger = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ProjectTermbaseLanguageIndexGuessor>(LogProvider.GetLoggerFactory());

		public ProjectTermbaseLanguageIndexGuessor(ITermbaseInfo termbase)
		{
			ProjectTermbaseLanguageIndexGuessor projectTermbaseLanguageIndexGuessor = this;
			_languageIndexNameDictionary = new Lazy<IDictionary<string, string>>(() => projectTermbaseLanguageIndexGuessor.GetLanguageIndexNameDictionary(termbase));
		}

		public ProjectTermbaseLanguageIndexGuessor(ITerminologyProvider tm)
		{
			ProjectTermbaseLanguageIndexGuessor projectTermbaseLanguageIndexGuessor = this;
			_languageIndexNameDictionary = new Lazy<IDictionary<string, string>>(() => projectTermbaseLanguageIndexGuessor.GetLanguageIndexNameDictionary(tm));
		}

		public ProjectTermbaseLanguageIndexGuessor(IEnumerable<ILanguage> languages)
		{
			ProjectTermbaseLanguageIndexGuessor projectTermbaseLanguageIndexGuessor = this;
			_languageIndexNameDictionary = new Lazy<IDictionary<string, string>>(() => projectTermbaseLanguageIndexGuessor.GetLanguageIndexNameDictionary(languages));
		}

		public IProjectTermbaseIndex Guess(Language language)
		{
			if (language != null && ((LanguageBase)language).IsoAbbreviation != null)
			{
				string text = ((LanguageBase)language).IsoAbbreviation.ToLower();
				if (_languageIndexNameDictionary.Value.ContainsKey(text))
				{
					string name = _languageIndexNameDictionary.Value[text];
					return _factory.CreateTermbaseIndex(name);
				}
				string key2 = GetLanguageCode(language).ToLower();
				if (_languageIndexNameDictionary.Value.ContainsKey(key2))
				{
					string name2 = _languageIndexNameDictionary.Value[key2];
					return _factory.CreateTermbaseIndex(name2);
				}
				string key3 = GetLanguageRegion(((LanguageBase)language).IsoAbbreviation).ToLower();
				if (_languageIndexNameDictionary.Value.ContainsKey(key3))
				{
					string name3 = _languageIndexNameDictionary.Value[key3];
					return _factory.CreateTermbaseIndex(name3);
				}
				string languageRegionCode = GetLanguageRegion(text);
				if (!string.IsNullOrEmpty(languageRegionCode))
				{
					string text2 = _languageIndexNameDictionary.Value.Keys.FirstOrDefault(delegate(string key)
					{
						bool result = false;
						string languageRegion = GetLanguageRegion(key);
						if (!string.IsNullOrEmpty(languageRegion))
						{
							result = string.Compare(languageRegion, languageRegionCode, StringComparison.InvariantCultureIgnoreCase) == 0;
						}
						return result;
					});
					if (!string.IsNullOrEmpty(text2))
					{
						string name4 = _languageIndexNameDictionary.Value[text2];
						return _factory.CreateTermbaseIndex(name4);
					}
				}
			}
			return null;
		}

		private IDictionary<string, string> GetLanguageIndexNameDictionary(ITerminologyProvider termbase)
		{
			return GetLanguageIndexNameDictionary((IEnumerable<ILanguage>)termbase.GetLanguages());
		}

		private IDictionary<string, string> GetLanguageIndexNameDictionary(IEnumerable<ILanguage> languages)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			IDictionary<string, string> dictionary = new Dictionary<string, string>();
			if (languages == null)
			{
				return dictionary;
			}
			try
			{
				foreach (ILanguage language in languages)
				{
					Language val = new Language(language.Locale.Name);
					string key = ((LanguageBase)val).IsoAbbreviation.ToLower();
					if (!dictionary.ContainsKey(key))
					{
						dictionary[key] = language.Name;
					}
					string key2 = GetLanguageCode(val).ToLower();
					if (!dictionary.ContainsKey(key2))
					{
						dictionary[key2] = language.Name;
					}
				}
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError(_logger, ex, "An error occured ", Array.Empty<object>());
			}
			return dictionary;
		}

		private IDictionary<string, string> GetLanguageIndexNameDictionary(ITermbaseInfo termbase)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			IDictionary<string, string> dictionary = new Dictionary<string, string>();
			if (termbase != null && termbase.ConnectionAlive)
			{
				TermbaseIndexList indexes = termbase.Indexes;
				foreach (ITermbaseIndex item in (List<ITermbaseIndex>)(object)indexes)
				{
					Language val = new Language(CultureUtilities.GetValidLocale(item.Locale));
					string key = ((LanguageBase)val).IsoAbbreviation.ToLower();
					if (!dictionary.ContainsKey(key))
					{
						dictionary[key] = item.Language;
					}
					string key2 = GetLanguageCode(val).ToLower();
					if (!dictionary.ContainsKey(key2))
					{
						dictionary[key2] = item.Language;
					}
				}
			}
			return dictionary;
		}

		private string GetLanguageCode(Language language)
		{
			if (language == null)
			{
				throw new ArgumentNullException("language");
			}
			if (!((LanguageData)language).IsNeutral)
			{
				string parentLanguageCode = ((LanguageData)language).ParentLanguageCode;
				if (!string.IsNullOrEmpty(parentLanguageCode) && parentLanguageCode.IndexOf("-", StringComparison.InvariantCulture) == -1)
				{
					return parentLanguageCode;
				}
			}
			string[] languageCodeRegionCode = GetLanguageCodeRegionCode(((LanguageBase)language).IsoAbbreviation);
			if (languageCodeRegionCode == null || languageCodeRegionCode.Length < 1)
			{
				return null;
			}
			return languageCodeRegionCode[0];
		}

		private string GetLanguageRegion(string isoAbbreviation)
		{
			string[] languageCodeRegionCode = GetLanguageCodeRegionCode(isoAbbreviation);
			if (languageCodeRegionCode == null || languageCodeRegionCode.Length < 2)
			{
				return null;
			}
			return languageCodeRegionCode[1];
		}

		private string[] GetLanguageCodeRegionCode(string isoAbbreviation)
		{
			if (string.IsNullOrEmpty(isoAbbreviation))
			{
				return null;
			}
			return isoAbbreviation.Split('-');
		}
	}
}
