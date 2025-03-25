using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Globalization.LanguageRegistry;
using Sdl.MultiTerm.Client.TermAccess;
using Sdl.MultiTerm.Core.Common.Interfaces;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.TermbaseApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.Server.ProjectSyncOperations
{
	internal class TermbaseSettingsSync : IGroupshareSyncOperation
	{
		public void SyncData(IProject project, Sdl.ProjectApi.Implementation.Xml.Project xmlProject, IProjectRepository projectRepository)
		{
			if (xmlProject.TermbaseConfiguration != null)
			{
				IProjectTermbaseConfiguration val = ((ICopyable<IProjectTermbaseConfiguration>)(object)((IProjectConfiguration)project).TermbaseConfiguration).Copy();
				TermbaseConfiguration termbaseConfiguration = xmlProject.TermbaseConfiguration;
				IProjectTermbaseConfigurationFactory factory = val.Factory;
				val.TermbaseServer = ((termbaseConfiguration.TermbaseServer == null) ? null : factory.CreateTermbaseServer(new Uri(termbaseConfiguration.TermbaseServer.ServerConnectionUri)));
				CopyRecognitionOptions(termbaseConfiguration, val);
				CopyTermbases(termbaseConfiguration, val, factory);
				UpdateLanguageIndexes(termbaseConfiguration, val, xmlProject.LanguageDirections, factory);
				((IProjectConfiguration)project).TermbaseConfiguration = val;
			}
		}

		private void UpdateLanguageIndexes(TermbaseConfiguration updatedConfig, IProjectTermbaseConfiguration localConfig, List<Sdl.ProjectApi.Implementation.Xml.LanguageDirection> projectLangPairs, IProjectTermbaseConfigurationFactory factory)
		{
			if (updatedConfig.LanguageIndexMappings.Any())
			{
				CopyLanguageIndexes(updatedConfig, localConfig, factory);
				return;
			}
			((ICollection<IProjectTermbaseLanguageIndex>)localConfig.LanguageIndexes).Clear();
			if (projectLangPairs == null || !projectLangPairs.Any())
			{
				return;
			}
			((ICollection<IProjectTermbaseLanguageIndex>)localConfig.LanguageIndexes).Add((IProjectTermbaseLanguageIndex)(object)CreateProjectLanguageIndex(projectLangPairs[0].SourceLanguageCode));
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection projectLangPair in projectLangPairs)
			{
				((ICollection<IProjectTermbaseLanguageIndex>)localConfig.LanguageIndexes).Add((IProjectTermbaseLanguageIndex)(object)CreateProjectLanguageIndex(projectLangPair.TargetLanguageCode));
			}
			GuessLanguageIndexes(localConfig);
		}

		private void GuessLanguageIndexes(IProjectTermbaseConfiguration projectTermbaseConfiguration)
		{
			if (projectTermbaseConfiguration.IsDefaultTermbaseSpecified())
			{
				TermAccess cachedTermAccess = projectTermbaseConfiguration.GetCachedTermAccess();
				if (cachedTermAccess == null)
				{
					return;
				}
				foreach (IProjectTermbaseLanguageIndex item in (IEnumerable<IProjectTermbaseLanguageIndex>)projectTermbaseConfiguration.LanguageIndexes)
				{
					item.TermbaseIndex = null;
				}
				for (int num = ((List<ITermbaseInfo>)(object)cachedTermAccess.Termbases).Count - 1; num >= 0; num--)
				{
					ITermbaseInfo termbase = ((List<ITermbaseInfo>)(object)cachedTermAccess.Termbases)[num];
					ProjectTermbaseLanguageIndexGuessor projectTermbaseLanguageIndexGuessor = new ProjectTermbaseLanguageIndexGuessor(termbase);
					foreach (IProjectTermbaseLanguageIndex item2 in (IEnumerable<IProjectTermbaseLanguageIndex>)projectTermbaseConfiguration.LanguageIndexes)
					{
						Language language = item2.Language;
						IProjectTermbaseIndex val = projectTermbaseLanguageIndexGuessor.Guess(language);
						if (val != null)
						{
							item2.TermbaseIndex = val;
						}
					}
				}
				return;
			}
			foreach (IProjectTermbaseLanguageIndex item3 in (IEnumerable<IProjectTermbaseLanguageIndex>)projectTermbaseConfiguration.LanguageIndexes)
			{
				item3.TermbaseIndex = null;
			}
		}

		private ProjectTermbaseLanguageIndex CreateProjectLanguageIndex(string projectLanguage)
		{
			return new ProjectTermbaseLanguageIndex(LanguageRegistryApi.Instance.GetLanguage(projectLanguage), null);
		}

		private void CopyRecognitionOptions(TermbaseConfiguration updatedConfig, IProjectTermbaseConfiguration localConfig)
		{
			if (updatedConfig.RecognitionOptions != null)
			{
				localConfig.RecognitionOptions.MinimumMatchValue = updatedConfig.RecognitionOptions.MinimumMatchValue;
				localConfig.RecognitionOptions.SearchDepth = updatedConfig.RecognitionOptions.SearchDepth;
				if (updatedConfig.RecognitionOptions.SearchOrderSpecified)
				{
					int searchOrder = (int)updatedConfig.RecognitionOptions.SearchOrder;
					localConfig.RecognitionOptions.SearchOrder = (TermbaseSearchOrder)searchOrder;
				}
				if (updatedConfig.RecognitionOptions.ShowWithNoAvailableTranslationSpecified)
				{
					localConfig.RecognitionOptions.ShowWithNoAvailableTranslation = updatedConfig.RecognitionOptions.ShowWithNoAvailableTranslation;
				}
			}
		}

		private void CopyLanguageIndexes(TermbaseConfiguration updatedConfig, IProjectTermbaseConfiguration localConfig, IProjectTermbaseConfigurationFactory factory)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			((ICollection<IProjectTermbaseLanguageIndex>)localConfig.LanguageIndexes).Clear();
			foreach (TermbaseLanguageIndexMapping languageIndexMapping in updatedConfig.LanguageIndexMappings)
			{
				Language val = new Language(languageIndexMapping.Language);
				IProjectTermbaseIndex val2 = factory.CreateTermbaseIndex(languageIndexMapping.Index);
				((ICollection<IProjectTermbaseLanguageIndex>)localConfig.LanguageIndexes).Add(factory.CreateTermbaseLanguageIndex(val, val2));
			}
		}

		private void CopyTermbases(TermbaseConfiguration updatedConfig, IProjectTermbaseConfiguration localConfig, IProjectTermbaseConfigurationFactory factory)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			List<KeyValuePair<IProjectTermbase, string>> list = new List<KeyValuePair<IProjectTermbase, string>>();
			for (int i = 0; i < ((ICollection<IProjectTermbase>)localConfig.Termbases).Count; i++)
			{
				IProjectTermbase val = ((IList<IProjectTermbase>)localConfig.Termbases)[i];
				if (!val.IsServerTermbase())
				{
					list.Add(new KeyValuePair<IProjectTermbase, string>(val, (i > 0) ? ((IList<IProjectTermbase>)localConfig.Termbases)[i - 1].Name : null));
				}
				else
				{
					dictionary[val.Name] = val.Enabled;
				}
			}
			((ICollection<IProjectTermbase>)localConfig.Termbases).Clear();
			AddServerTermbases(updatedConfig, localConfig, factory, dictionary);
			AddFileBasedTermbases(localConfig, list);
		}

		private void AddServerTermbases(TermbaseConfiguration updatedConfig, IProjectTermbaseConfiguration localConfig, IProjectTermbaseConfigurationFactory factory, Dictionary<string, bool> enabledStates)
		{
			foreach (Termbase termbasis in updatedConfig.Termbases)
			{
				TermbaseSettings val = TermbaseSettings.FromXml(termbasis.SettingsXml);
				string settingsXml = val.ToXml();
				IProjectTermbaseFilter filter = ((termbasis.Filter != null) ? factory.CreateTermbaseFilter(termbasis.Filter.Id, termbasis.Filter.Name) : null);
				if (!enabledStates.TryGetValue(termbasis.Name, out var value))
				{
					value = true;
				}
				((ICollection<IProjectTermbase>)localConfig.Termbases).Add((IProjectTermbase)(object)new ProjectTermbase(termbasis.Name, settingsXml, filter, value));
			}
		}

		private void AddFileBasedTermbases(IProjectTermbaseConfiguration localConfig, List<KeyValuePair<IProjectTermbase, string>> localTermbases)
		{
			foreach (KeyValuePair<IProjectTermbase, string> localTermbasis in localTermbases)
			{
				int num;
				if (localTermbasis.Value != null && (num = FindTermbase((IList<IProjectTermbase>)localConfig.Termbases, localTermbasis.Value)) != -1)
				{
					((IList<IProjectTermbase>)localConfig.Termbases).Insert(num + 1, localTermbasis.Key);
				}
				else
				{
					((ICollection<IProjectTermbase>)localConfig.Termbases).Add(localTermbasis.Key);
				}
			}
		}

		private int FindTermbase(IList<IProjectTermbase> items, string name)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].Name.Equals(name))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
