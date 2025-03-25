using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sdl.Core.Globalization;
using Sdl.Core.Globalization.LanguageRegistry;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.MultiTerm.Client.TerminologyProvider.TerminologyPrioviders;
using Sdl.MultiTerm.Client.TerminologySearch;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;
using Sdl.Terminology.TerminologyProvider.Core.Exceptions;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	internal class ProjectTermbaseProvider : IProjectTermbaseProvider
	{
		private readonly IProjectTermbaseConfiguration _projectTermbaseConfiguration;

		private readonly ITerminologyProviderCredentialStore _terminologyProviderCredentialStore;

		private readonly ILogger _logger = (ILogger)(object)LoggerFactoryExtensions.CreateLogger<ProjectTermbaseProvider>(LogProvider.GetLoggerFactory());

		internal ProjectTermbaseProvider(IProjectTermbaseConfiguration projectTermbaseConfiguration)
		{
			_projectTermbaseConfiguration = projectTermbaseConfiguration;
			UpdateTerminologyProviderCredentialStore(IdentityInfoCache.Default);
		}

		internal ProjectTermbaseProvider(IProjectTermbaseConfiguration projectTermbaseConfiguration, ITerminologyProviderCredentialStore terminologyProviderCredentialStore)
		{
			_projectTermbaseConfiguration = projectTermbaseConfiguration;
			_terminologyProviderCredentialStore = terminologyProviderCredentialStore;
			UpdateTerminologyProviderCredentialStore(IdentityInfoCache.Default);
		}

		public ITerminologyProvider GetDefaultTermbase()
		{
			IProjectTermbase defaultTermbase = _projectTermbaseConfiguration.Termbases.GetDefaultTermbase();
			if (defaultTermbase != null)
			{
				return GetProviderTermbase(defaultTermbase);
			}
			return null;
		}

		public ITerminologyProvider GetTermbase(IProjectTermbase projectTermbase)
		{
			return GetProviderTermbase(projectTermbase);
		}

		public ITerminologyProvider GetTermbase(IProjectTermbase projectTermbase, TerminologyProviderCredential credential)
		{
			ITerminologyProvider result = null;
			TermbaseSettings val = TermbaseSettings.FromXml(projectTermbase.SettingsXml);
			try
			{
				TerminologyProviderManager.DefaultTerminologyCredentialStore.AddOrUpdateCredential(val.GetProviderUnqualifiedUri(), credential);
				ITerminologyProvider terminologyProvider = TerminologyProviderManager.Instance.GetTerminologyProvider(val.GetProviderUri());
				if (terminologyProvider != null && TryConnect(terminologyProvider, val, credential))
				{
					result = terminologyProvider;
				}
			}
			catch (Exception arg)
			{
				LoggerExtensions.LogInformation(_logger, $"Failed get termbase {val.GetId()} from provider {val.GetProviderUri()}. Exception : {arg}", Array.Empty<object>());
			}
			return result;
		}

		private bool TryConnect(ITerminologyProvider prov, TermbaseSettings settings, TerminologyProviderCredential credential = null)
		{
			IMultiTermTerminologyProvider val = (IMultiTermTerminologyProvider)(object)((prov is IMultiTermTerminologyProvider) ? prov : null);
			if (val != null)
			{
				if (credential == null)
				{
					return val.Connect(settings);
				}
				return val.Connect(credential, settings);
			}
			IConnectionAwareTerminologyProvider val2 = (IConnectionAwareTerminologyProvider)(object)((prov is IConnectionAwareTerminologyProvider) ? prov : null);
			if (val2 != null)
			{
				return val2.Status.IsConnected;
			}
			return true;
		}

		public Task<ITerminologyProvider> GetTermbaseAsync(IProjectTermbase projectTermbase)
		{
			throw new NotImplementedException();
		}

		public void UpdateProjectLanguageIndexes(IList<Language> languages)
		{
			ITerminologyProvider termbase = null;
			IProjectTermbase defaultTermbase = _projectTermbaseConfiguration.Termbases.GetDefaultTermbase();
			if (defaultTermbase != null)
			{
				TryGetTermbase(defaultTermbase, out termbase);
			}
			new ProjectTermbaseLanguageIndexUpdater(_projectTermbaseConfiguration, termbase).Update(languages);
		}

		public void GuessLanguageIndexes()
		{
			if (_projectTermbaseConfiguration.IsDefaultTermbaseSpecified())
			{
				List<IProjectTermbase> list = ((IEnumerable<IProjectTermbase>)_projectTermbaseConfiguration.Termbases).Reverse().ToList();
				{
					foreach (IProjectTermbase item in list)
					{
						if (!item.Enabled || !TryGetTermbase(item, out var termbase))
						{
							continue;
						}
						ProjectTermbaseLanguageIndexGuessor projectTermbaseLanguageIndexGuessor = new ProjectTermbaseLanguageIndexGuessor(termbase);
						foreach (IProjectTermbaseLanguageIndex item2 in (IEnumerable<IProjectTermbaseLanguageIndex>)_projectTermbaseConfiguration.LanguageIndexes)
						{
							IProjectTermbaseIndex val = projectTermbaseLanguageIndexGuessor.Guess(item2.Language);
							if (val != null)
							{
								item2.TermbaseIndex = val;
							}
						}
					}
					return;
				}
			}
			foreach (IProjectTermbaseLanguageIndex item3 in (IEnumerable<IProjectTermbaseLanguageIndex>)_projectTermbaseConfiguration.LanguageIndexes)
			{
				item3.TermbaseIndex = null;
			}
		}

		public ITermbaseRecognitionEngine CreateTermVerifierEngine(ITerminologyProvider termbase, CultureInfo source, CultureInfo target, int minMatch)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			TermbaseContext item = new TermbaseContext
			{
				Termbase = termbase,
				SourceIndex = GetTermbaseProviderBestIndex(termbase, source),
				TargetIndex = GetTermbaseProviderBestIndex(termbase, target)
			};
			return (ITermbaseRecognitionEngine)new TermbaseRecognitionEngine
			{
				TermbaseContextList = new List<TermbaseContext> { item },
				MinMatch = minMatch,
				SearchSplitTerms = 0,
				SearchMode = (SearchMode)1,
				SearchOrder = (SearchOrder)1,
				TargetRequired = false,
				SearchDepth = 0,
				IgnoreFilters = false
			};
		}

		private bool TryGetTermbase(IProjectTermbase projectTermbase, out ITerminologyProvider termbase)
		{
			TermbaseSettings val = TermbaseSettings.FromXml(projectTermbase.SettingsXml);
			try
			{
				ITerminologyProvider terminologyProvider = TerminologyProviderManager.Instance.GetTerminologyProvider(val.GetProviderUri());
				if (terminologyProvider != null && TryConnect(terminologyProvider, val))
				{
					termbase = terminologyProvider;
					return true;
				}
			}
			catch (Exception arg)
			{
				LoggerExtensions.LogInformation(_logger, $"Failed get termbase {val.GetId()} from provider {val.GetProviderUri()}. Exception : {arg}", Array.Empty<object>());
			}
			termbase = null;
			return false;
		}

		private ILanguage GetTermbaseProviderBestIndex(ITerminologyProvider termbase, CultureInfo source)
		{
			IList<string> preferredLocales = GetPreferredLocales(source.Name);
			foreach (ILanguage language in termbase.GetLanguages())
			{
				foreach (string item in preferredLocales)
				{
					if (language.Locale.TwoLetterISOLanguageName.Equals(item, StringComparison.CurrentCultureIgnoreCase))
					{
						return language;
					}
				}
			}
			return null;
		}

		private IList<string> GetPreferredLocales(string lcName)
		{
			IList<string> list = new List<string>();
			Language language;
			try
			{
				language = LanguageRegistryApi.Instance.GetLanguage(lcName);
			}
			catch (UnsupportedLanguageException)
			{
				LoggerExtensions.LogError(_logger, "Unsuported langauge " + lcName, Array.Empty<object>());
				return list;
			}
			list.Add(((LanguageBase)language).IsoAbbreviation);
			IList<Language> regionalVariants;
			if (((LanguageData)language).IsNeutral)
			{
				regionalVariants = language.RegionalVariants;
			}
			else
			{
				list.Add(((LanguageData)language).ParentLanguageCode);
				regionalVariants = LanguageRegistryApi.Instance.GetLanguage(((LanguageData)language).ParentLanguageCode).RegionalVariants;
			}
			foreach (Language item2 in regionalVariants)
			{
				if (!list.Contains(((LanguageBase)item2).IsoAbbreviation))
				{
					list.Add(((LanguageBase)item2).IsoAbbreviation);
				}
			}
			int num = ((LanguageBase)language).IsoAbbreviation.IndexOf("-");
			if (-1 != num)
			{
				string item = ((LanguageBase)language).IsoAbbreviation.Substring(0, num);
				if (!list.Contains(item))
				{
					list.Add(((LanguageBase)language).IsoAbbreviation.Substring(0, num));
				}
			}
			return list;
		}

		private void UpdateTerminologyProviderCredentialStore(IdentityInfoCache identityInfo)
		{
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)_projectTermbaseConfiguration.Termbases)
			{
				TermbaseSettings val = TermbaseSettings.FromXml(item.SettingsXml);
				ITerminologyProviderCredentialStore val2 = (val.IsCustom ? _terminologyProviderCredentialStore : TerminologyProviderManager.DefaultTerminologyCredentialStore);
				if (!val.Local && val2.GetCredential(val.GetProviderUnqualifiedUri()) == null && identityInfo.ContainsKey(val.GetProviderUnqualifiedUri().ToString()))
				{
					UpdateTerminologyProviderCredentialStore(identityInfo, val, val2);
				}
			}
		}

		private void UpdateTerminologyProviderCredentialStore(IdentityInfoCache identityInfo, TermbaseSettings termbaseSetting, ITerminologyProviderCredentialStore credentialStore)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			UserCredentials userCredentials = identityInfo.GetUserCredentials(termbaseSetting.GetProviderUnqualifiedUri().ToString());
			if (userCredentials != null)
			{
				TerminologyUserCredentials val = (((int)userCredentials.UserType != 3) ? new TerminologyUserCredentials(userCredentials.UserName, userCredentials.Password, (TerminologyUserManagerTokenType)userCredentials.UserType) : new TerminologyUserCredentials(userCredentials.UserName, userCredentials.SamlToken, userCredentials.AuthToken, userCredentials.ExpirationDate, (TerminologyUserManagerTokenType)userCredentials.UserType));
				credentialStore.AddCredential(termbaseSetting.GetProviderUnqualifiedUri(), new TerminologyProviderCredential(val, false));
			}
		}

		private ITerminologyProvider GetProviderTermbase(IProjectTermbase projectTermbase)
		{
			//IL_004a: Expected O, but got Unknown
			ITerminologyProvider result = null;
			TermbaseSettings val = TermbaseSettings.FromXml(projectTermbase.SettingsXml);
			try
			{
				ITerminologyProviderCredentialStore val2 = (val.IsCustom ? _terminologyProviderCredentialStore : TerminologyProviderManager.DefaultTerminologyCredentialStore);
				ITerminologyProvider terminologyProvider = TerminologyProviderManager.Instance.GetTerminologyProvider(val.GetProviderUri(), val2);
				if (terminologyProvider != null && TryConnect(terminologyProvider, val))
				{
					result = terminologyProvider;
				}
			}
			catch (MissingTerminologyProviderException val3)
			{
				MissingTerminologyProviderException val4 = val3;
				LoggerExtensions.LogInformation(_logger, $"{((Exception)(object)val4).Message} Exception : {val4}", Array.Empty<object>());
				return null;
			}
			catch (Exception arg)
			{
				LoggerExtensions.LogInformation(_logger, $"Failed get termbase {val.GetId()} from provider {val.GetProviderUri()}. Exception : {arg}", Array.Empty<object>());
				return null;
			}
			return result;
		}
	}
}
