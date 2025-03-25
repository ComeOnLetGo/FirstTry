using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.Implementation.TermbaseApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectTermbaseConfigurationProvider
	{
		private readonly ITerminologyProviderCredentialStore _credentialStore;

		private readonly TerminologyProviderSettings _settings;

		private readonly IRelativePathManager _pathManager;

		public ProjectTermbaseConfigurationProvider(ITerminologyProviderCredentialStore credentialStore, TerminologyProviderSettings settings, IRelativePathManager pathManager)
		{
			_pathManager = pathManager;
			_settings = settings;
			_credentialStore = credentialStore;
		}

		public IProjectTermbaseConfiguration GetTermbaseConfiguration(TermbaseConfiguration termbaseConfigurationXml)
		{
			IProjectTermbases termbases = GetTermbases(termbaseConfigurationXml);
			IProjectTermbaseServer termbaseServer = (termbases.HasServerTermbase() ? GetTermbaseServer(termbaseConfigurationXml) : null);
			IProjectTermbaseLanguageIndexes languageIndexes = GetLanguageIndexes(termbaseConfigurationXml);
			IProjectTermbaseRecognitionOptions recognitionOptions = GetRecognitionOptions(termbaseConfigurationXml);
			return (IProjectTermbaseConfiguration)(object)new ProjectTermbaseConfiguration(termbaseServer, termbases, languageIndexes, recognitionOptions, _credentialStore);
		}

		private IProjectTermbases GetTermbases(TermbaseConfiguration termbaseConfigurationXml)
		{
			IProjectTermbases val = (IProjectTermbases)(object)new ProjectTermbases(_settings);
			IEnumerable<Termbase> enumerable = new List<Termbase>();
			if (termbaseConfigurationXml != null)
			{
				enumerable = termbaseConfigurationXml.Termbases;
			}
			Setting<List<Termbase>> termbases = _settings.Termbases;
			if (termbases != null && termbases.Value?.Count > 0)
			{
				enumerable = enumerable.Concat(_settings.Termbases.Value);
			}
			if (enumerable == null)
			{
				return val;
			}
			enumerable = OrderTermbases(_settings, enumerable);
			foreach (Termbase item2 in enumerable)
			{
				string name = ReviewTermbaseNames(item2.Name);
				string absoluteSettingsXml = GetAbsoluteSettingsXml(item2.SettingsXml);
				IProjectTermbaseFilter filter = (IProjectTermbaseFilter)(object)((item2.Filter != null) ? new ProjectTermbaseFilter(item2.Filter.Id, item2.Filter.Name) : null);
				bool enabled = item2.Enabled;
				IProjectTermbase item = (IProjectTermbase)(object)new ProjectTermbase(name, absoluteSettingsXml, filter, enabled);
				((ICollection<IProjectTermbase>)val).Add(item);
			}
			return val;
		}

		private IEnumerable<Termbase> OrderTermbases(TerminologyProviderSettings settings, IEnumerable<Termbase> allTermbases)
		{
			TerminologyProviderSettings terminologyProviderSettings = settings;
			if (terminologyProviderSettings != null && terminologyProviderSettings.TermbasesOrder?.Value?.Count > 0)
			{
				allTermbases = allTermbases.OrderBy((Termbase a) => settings.TermbasesOrder.Value.IndexOf(a.Name));
			}
			else
			{
				if (settings == null)
				{
					settings = new TerminologyProviderSettings();
				}
				if (settings.TermbasesOrder?.Value == null)
				{
					settings.TermbasesOrder.Value = new List<string>();
				}
				settings.TermbasesOrder.Value = allTermbases.Select((Termbase termbaseXml) => termbaseXml.Name).ToList();
			}
			return allTermbases;
		}

		private string ReviewTermbaseNames(string termbaseName)
		{
			if (termbaseName.Contains(" (Beta)"))
			{
				termbaseName = termbaseName.Replace(" (Beta)", "");
			}
			return termbaseName;
		}

		private string GetAbsoluteSettingsXml(string settingsXml)
		{
			TermbaseSettings val = TermbaseSettings.FromXml(settingsXml);
			if (val.Local && !val.IsCustom && _pathManager != null)
			{
				val.Path = _pathManager.MakeAbsolutePath(val.Path);
				return val.ToXml();
			}
			return settingsXml;
		}

		private IProjectTermbaseServer GetTermbaseServer(TermbaseConfiguration termbaseConfigurationXml)
		{
			if (termbaseConfigurationXml != null && termbaseConfigurationXml.TermbaseServer != null)
			{
				string serverConnectionUri = termbaseConfigurationXml.TermbaseServer.ServerConnectionUri;
				if (!string.IsNullOrEmpty(serverConnectionUri))
				{
					return (IProjectTermbaseServer)(object)new ProjectTermbaseServer(new Uri(serverConnectionUri));
				}
			}
			return null;
		}

		private IProjectTermbaseLanguageIndexes GetLanguageIndexes(TermbaseConfiguration termbaseConfigurationXml)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			IProjectTermbaseLanguageIndexes val = (IProjectTermbaseLanguageIndexes)(object)new ProjectTermbaseLanguageIndexes();
			if (termbaseConfigurationXml != null)
			{
				foreach (TermbaseLanguageIndexMapping languageIndexMapping in termbaseConfigurationXml.LanguageIndexMappings)
				{
					Language language = new Language(languageIndexMapping.Language);
					IProjectTermbaseIndex termbaseIndex = (IProjectTermbaseIndex)(object)((languageIndexMapping.Index != null) ? new ProjectTermbaseIndex(languageIndexMapping.Index) : null);
					IProjectTermbaseLanguageIndex item = (IProjectTermbaseLanguageIndex)(object)new ProjectTermbaseLanguageIndex(language, termbaseIndex);
					((ICollection<IProjectTermbaseLanguageIndex>)val).Add(item);
				}
			}
			return val;
		}

		private IProjectTermbaseRecognitionOptions GetRecognitionOptions(TermbaseConfiguration termbaseConfigurationXml)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (termbaseConfigurationXml != null && termbaseConfigurationXml.RecognitionOptions != null)
			{
				bool showWithNoAvailableTranslation = termbaseConfigurationXml.RecognitionOptions.ShowWithNoAvailableTranslationSpecified && termbaseConfigurationXml.RecognitionOptions.ShowWithNoAvailableTranslation;
				int minimumMatchValue = termbaseConfigurationXml.RecognitionOptions.MinimumMatchValue;
				int searchDepth = termbaseConfigurationXml.RecognitionOptions.SearchDepth;
				TermbaseSearchOrder searchOrder = (TermbaseSearchOrder)((!termbaseConfigurationXml.RecognitionOptions.SearchOrderSpecified) ? 2 : ((int)EnumConvert.ConvertTermbaseSearchOrder(termbaseConfigurationXml.RecognitionOptions.SearchOrder)));
				return (IProjectTermbaseRecognitionOptions)(object)new ProjectTermbaseRecognitionOptions(showWithNoAvailableTranslation, minimumMatchValue, searchDepth, searchOrder);
			}
			return (IProjectTermbaseRecognitionOptions)(object)new ProjectTermbaseRecognitionOptions();
		}

		public TermbaseConfiguration GetTermbaseConfigurationXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			TermbaseConfiguration termbaseConfiguration2 = new TermbaseConfiguration();
			termbaseConfiguration2.Termbases = GetTermbasesXml(termbaseConfiguration);
			termbaseConfiguration2.TermbaseServer = (termbaseConfiguration.Termbases.HasServerTermbase() ? GetTermbaseServerXml(termbaseConfiguration) : null);
			termbaseConfiguration2.LanguageIndexMappings = GetLanguageIndexMappingsXml(termbaseConfiguration);
			termbaseConfiguration2.RecognitionOptions = GetRecognitionOptionsXml(termbaseConfiguration);
			_settings.Termbases.Value = GetCustomTermbasesXml(termbaseConfiguration);
			return termbaseConfiguration2;
		}

		private TermbaseServer GetTermbaseServerXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			TermbaseServer termbaseServer = null;
			if (termbaseConfiguration.TermbaseServer != null && termbaseConfiguration.TermbaseServer.ServerConnectionUri != null)
			{
				termbaseServer = new TermbaseServer();
				termbaseServer.ServerConnectionUri = TermbaseSettings.GetProviderUnqualifiedUri(termbaseConfiguration.TermbaseServer.ServerConnectionUri).ToString();
			}
			return termbaseServer;
		}

		private List<Termbase> GetTermbasesXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			List<Termbase> list = new List<Termbase>();
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbaseConfiguration.Termbases)
			{
				TermbaseSettings val = TermbaseSettings.FromXml(item.SettingsXml);
				if (!val.IsCustom)
				{
					Termbase termbase = new Termbase();
					termbase.Name = item.Name;
					termbase.SettingsXml = GetRelativeSettingsXml(item.SettingsXml);
					TermbaseFilter termbaseFilter = null;
					if (item.Filter != null)
					{
						termbaseFilter = new TermbaseFilter();
						termbaseFilter.Id = item.Filter.Id;
						termbaseFilter.Name = item.Filter.Name;
					}
					termbase.Filter = termbaseFilter;
					termbase.Enabled = item.Enabled;
					list.Add(termbase);
				}
			}
			return list;
		}

		private List<TermbaseLanguageIndexMapping> GetLanguageIndexMappingsXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			List<TermbaseLanguageIndexMapping> list = new List<TermbaseLanguageIndexMapping>();
			foreach (IProjectTermbaseLanguageIndex item in (IEnumerable<IProjectTermbaseLanguageIndex>)termbaseConfiguration.LanguageIndexes)
			{
				TermbaseLanguageIndexMapping termbaseLanguageIndexMapping = new TermbaseLanguageIndexMapping();
				termbaseLanguageIndexMapping.Language = ((LanguageBase)item.Language).IsoAbbreviation;
				termbaseLanguageIndexMapping.Index = ((item.TermbaseIndex != null) ? item.TermbaseIndex.Name : null);
				list.Add(termbaseLanguageIndexMapping);
			}
			return list;
		}

		private TermbaseRecognitionOptions GetRecognitionOptionsXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			TermbaseRecognitionOptions termbaseRecognitionOptions = null;
			if (termbaseConfiguration.RecognitionOptions != null)
			{
				bool showWithNoAvailableTranslation = termbaseConfiguration.RecognitionOptions.ShowWithNoAvailableTranslation;
				int minimumMatchValue = termbaseConfiguration.RecognitionOptions.MinimumMatchValue;
				int searchDepth = termbaseConfiguration.RecognitionOptions.SearchDepth;
				TermbaseSearchOrder searchOrder = termbaseConfiguration.RecognitionOptions.SearchOrder;
				termbaseRecognitionOptions = new TermbaseRecognitionOptions();
				termbaseRecognitionOptions.ShowWithNoAvailableTranslation = showWithNoAvailableTranslation;
				termbaseRecognitionOptions.ShowWithNoAvailableTranslationSpecified = true;
				termbaseRecognitionOptions.MinimumMatchValue = minimumMatchValue;
				termbaseRecognitionOptions.SearchDepth = searchDepth;
				termbaseRecognitionOptions.SearchOrder = EnumConvert.ConvertTermbaseSearchOrder(searchOrder);
				termbaseRecognitionOptions.SearchOrderSpecified = true;
			}
			return termbaseRecognitionOptions;
		}

		private List<Termbase> GetCustomTermbasesXml(IProjectTermbaseConfiguration termbaseConfiguration)
		{
			List<Termbase> list = new List<Termbase>();
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbaseConfiguration.Termbases)
			{
				TermbaseSettings val = TermbaseSettings.FromXml(item.SettingsXml);
				if (val.IsCustom)
				{
					Termbase termbase = new Termbase
					{
						Name = item.Name,
						SettingsXml = GetRelativeSettingsXml(item.SettingsXml)
					};
					TermbaseFilter filter = null;
					if (item.Filter != null)
					{
						filter = new TermbaseFilter
						{
							Id = item.Filter.Id,
							Name = item.Filter.Name
						};
					}
					termbase.Filter = filter;
					termbase.Enabled = item.Enabled;
					list.Add(termbase);
				}
			}
			return list;
		}

		protected internal string GetRelativeSettingsXml(string settingsXml)
		{
			TermbaseSettings val = TermbaseSettings.FromXml(settingsXml);
			if (val.Local && !val.IsCustom && _pathManager != null)
			{
				val.Path = _pathManager.MakeRelativePath(val.Path);
				return val.ToXml();
			}
			return settingsXml;
		}
	}
}
