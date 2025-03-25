using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.MultiTerm.Client.TermAccess;
using Sdl.MultiTerm.Core.Common.Interfaces;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseLanguageIndexUpdater
	{
		private readonly IProjectTermbaseConfiguration _termbaseConfiguration;

		private readonly ITerminologyProvider _termbaseProvider;

		private readonly Lazy<ProjectTermbaseLanguageIndexGuessor> _indexGuessor;

		public ProjectTermbaseLanguageIndexUpdater(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			if (termbaseConfiguration == null)
			{
				throw new ArgumentNullException("termbaseConfiguration");
			}
			_termbaseConfiguration = termbaseConfiguration;
			_indexGuessor = new Lazy<ProjectTermbaseLanguageIndexGuessor>(CreateIndexGuessor);
		}

		public ProjectTermbaseLanguageIndexUpdater(IProjectTermbaseConfiguration termbaseConfiguration, ITerminologyProvider terminologyProvider)
		{
			if (termbaseConfiguration == null)
			{
				throw new ArgumentNullException("termbaseConfiguration");
			}
			_termbaseConfiguration = termbaseConfiguration;
			_termbaseProvider = terminologyProvider;
			_indexGuessor = new Lazy<ProjectTermbaseLanguageIndexGuessor>(CreateIndexGuessor);
		}

		private ProjectTermbaseLanguageIndexGuessor CreateIndexGuessor()
		{
			return CreateIndexGuessor(_termbaseConfiguration) ?? CreateIndexGuessor(_termbaseProvider);
		}

		private static ProjectTermbaseLanguageIndexGuessor CreateIndexGuessor(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			TermAccess cachedTermAccess = termbaseConfiguration.GetCachedTermAccess();
			if (cachedTermAccess != null && termbaseConfiguration.IsDefaultTermbaseSpecified() && termbaseConfiguration.IsDefaultTermbaseConnected(cachedTermAccess))
			{
				return new ProjectTermbaseLanguageIndexGuessor(((List<ITermbaseInfo>)(object)cachedTermAccess.Termbases)[0]);
			}
			return null;
		}

		private ProjectTermbaseLanguageIndexGuessor CreateIndexGuessor(ITerminologyProvider tm)
		{
			if (tm != null && _termbaseConfiguration != null && _termbaseConfiguration.IsDefaultTermbaseSpecified())
			{
				return new ProjectTermbaseLanguageIndexGuessor(tm);
			}
			return null;
		}

		public void Update(IList<Language> projectLanguages)
		{
			if (projectLanguages == null)
			{
				throw new ArgumentNullException("projectLanguages");
			}
			AddLanguageIndexes(GetLanguagesWithoutALanguageIndex(projectLanguages));
			RemoveLanguageIndexes(GetLanguageIndexesWithoutALanguage(projectLanguages));
		}

		private void AddLanguageIndexes(IList<Language> languages)
		{
			foreach (Language language in languages)
			{
				IProjectTermbaseIndex val = null;
				if (_indexGuessor.Value != null)
				{
					val = _indexGuessor.Value.Guess(language);
				}
				((ICollection<IProjectTermbaseLanguageIndex>)_termbaseConfiguration.LanguageIndexes).Add(_termbaseConfiguration.Factory.CreateTermbaseLanguageIndex(language, val));
			}
		}

		private IList<Language> GetLanguagesWithoutALanguageIndex(IList<Language> languages)
		{
			IList<Language> list = new List<Language>();
			foreach (Language language in languages)
			{
				if (!Contains(language))
				{
					list.Add(language);
				}
			}
			return list;
		}

		private bool Contains(Language language)
		{
			foreach (IProjectTermbaseLanguageIndex item in (IEnumerable<IProjectTermbaseLanguageIndex>)_termbaseConfiguration.LanguageIndexes)
			{
				if (((object)item.Language).Equals((object)language))
				{
					return true;
				}
			}
			return false;
		}

		private void RemoveLanguageIndexes(IList<IProjectTermbaseLanguageIndex> languageIndexes)
		{
			foreach (IProjectTermbaseLanguageIndex languageIndex in languageIndexes)
			{
				((ICollection<IProjectTermbaseLanguageIndex>)_termbaseConfiguration.LanguageIndexes).Remove(languageIndex);
			}
		}

		private IList<IProjectTermbaseLanguageIndex> GetLanguageIndexesWithoutALanguage(IList<Language> languages)
		{
			IList<IProjectTermbaseLanguageIndex> list = new List<IProjectTermbaseLanguageIndex>();
			foreach (IProjectTermbaseLanguageIndex item in (IEnumerable<IProjectTermbaseLanguageIndex>)_termbaseConfiguration.LanguageIndexes)
			{
				if (!languages.Contains(item.Language))
				{
					list.Add(item);
				}
			}
			return list;
		}
	}
}
