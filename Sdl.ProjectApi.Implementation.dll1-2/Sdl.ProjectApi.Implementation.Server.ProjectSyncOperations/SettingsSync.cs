using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Server.ProjectSyncOperations
{
	internal class SettingsSync : IGroupshareSyncOperation
	{
		private readonly IProjectSettingsUpdater _settingsUpdater;

		public SettingsSync(IProjectSettingsUpdater settingsUpdater)
		{
			_settingsUpdater = settingsUpdater;
		}

		public void SyncData(IProject project, Sdl.ProjectApi.Implementation.Xml.Project xmlProject, IProjectRepository projectRepository)
		{
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			SettingsBundlesList settingsBundlesList = new SettingsBundlesList(xmlProject.SettingsBundles);
			ISettingsBundle settings = ((IObjectWithSettings)project).Settings;
			ISettingsBundle settingsBundle = settingsBundlesList.GetSettingsBundle(xmlProject.SettingsBundleGuid, null);
			_settingsUpdater.SuppressSetting(settingsBundle, "FileViewServiceSettings", "SelectedTargetLanguageCode");
			if (xmlProject.GeneralProjectInfo.Name != "$$__missing__$$")
			{
				GeneralProjectInfo generalProjectInfo = xmlProject.GeneralProjectInfo;
				ApplySynchronizationInfo(generalProjectInfo.Name, generalProjectInfo.Description, generalProjectInfo.DueDateSpecified ? new DateTime?(generalProjectInfo.DueDate.ToLocalTime()) : null, (settingsBundle != null) ? Setting<string>.op_Implicit(settingsBundle.GetSettingsGroup<PublishProjectOperationSettings>().OrganizationPath) : string.Empty, EnumConvert.ConvertProjectStatus(generalProjectInfo.Status), generalProjectInfo.CompletedAtSpecified ? new DateTime?(generalProjectInfo.CompletedAt) : null, generalProjectInfo.Customer?.Name, generalProjectInfo.Customer?.Email, project);
			}
			if (settingsBundle != null && !settingsBundle.IsEmpty)
			{
				ISettingsGroup settingsGroup = settings.GetSettingsGroup("FileViewServiceSettings");
				_settingsUpdater.ImportSetting(settingsBundle, settingsGroup);
				((IProjectConfigurationRepository)projectRepository).SettingsBundles.ImportSettingsBundle(((IProjectConfigurationRepository)projectRepository).SettingsBundleGuid, settingsBundle);
				((IProjectConfigurationRepository)projectRepository).SettingsBundles.SaveAndClearCache();
			}
			foreach (Sdl.ProjectApi.Implementation.Xml.User user2 in xmlProject.Users)
			{
				User user = new User(user2);
				((IProjectConfigurationRepository)projectRepository).AddUser((IUser)(object)user);
			}
			AddOrUpdateLanguageDirections(project, xmlProject.LanguageDirections, (ISettingsBundlesList)(object)settingsBundlesList);
			if (xmlProject.AnalysisBands != null && xmlProject.AnalysisBands.Count > 0)
			{
				((IProjectConfigurationRepository)projectRepository).SetAnalysisBands(xmlProject.AnalysisBands.Select((Sdl.ProjectApi.Implementation.Xml.AnalysisBand b) => b.MinimumMatchValue).ToArray());
			}
			if (xmlProject.CascadeItem != null)
			{
				ProjectCascadeItem fromCascadeItem = xmlProject.CascadeItem.ToObject((IRelativePathManager)(object)((project is IRelativePathManager) ? project : null));
				UpdateTranslationProviderCascade(fromCascadeItem, ((IProjectConfiguration)project).CascadeItem);
			}
		}

		private void AddOrUpdateLanguageDirections(IProject project, IEnumerable<Sdl.ProjectApi.Implementation.Xml.LanguageDirection> languageDirections, ISettingsBundlesList settingsBundlesList)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection in languageDirections)
			{
				Language val = new Language(languageDirection.SourceLanguageCode);
				Language val2 = new Language(languageDirection.TargetLanguageCode);
				ILanguageDirection val3 = ((IProjectConfiguration)project).GetLanguageDirection(val, val2) ?? ((IProjectConfiguration)project).AddLanguageDirection(val, val2);
				if (settingsBundlesList.ContainsSettingsBundle(languageDirection.SettingsBundleGuid))
				{
					((IObjectWithSettings)val3).Settings = settingsBundlesList.GetSettingsBundle(languageDirection.SettingsBundleGuid, ((IObjectWithSettings)project).Settings);
				}
				if (languageDirection.AutoSuggestDictionaries != null)
				{
					UpdateAutoSuggestDictionaries(val3, languageDirection);
				}
				if (languageDirection.CascadeItem != null)
				{
					ProjectCascadeItem fromCascadeItem = languageDirection.CascadeItem.ToObject((IRelativePathManager)(object)((project is IRelativePathManager) ? project : null));
					UpdateTranslationProviderCascade(fromCascadeItem, val3.CascadeItem);
				}
			}
		}

		private void UpdateTranslationProviderCascade(ProjectCascadeItem fromCascadeItem, ProjectCascadeItem toCascadeItem)
		{
			bool flag = toCascadeItem.StopSearchingWhenResultsFound == fromCascadeItem.StopSearchingWhenResultsFound && toCascadeItem.OverrideParent == fromCascadeItem.OverrideParent;
			if (fromCascadeItem.OverrideParent)
			{
				if (fromCascadeItem.CascadeEntryItems.Count != toCascadeItem.CascadeEntryItems.Count)
				{
					flag = false;
				}
				else
				{
					foreach (ProjectCascadeEntryItem cascadeEntryItem in toCascadeItem.CascadeEntryItems)
					{
						ITranslationProviderItem mainTranslationProviderItem = cascadeEntryItem.MainTranslationProviderItem;
						if (mainTranslationProviderItem.IsFileBasedTranslationMemory())
						{
							continue;
						}
						bool flag2 = false;
						foreach (ProjectCascadeEntryItem cascadeEntryItem2 in fromCascadeItem.CascadeEntryItems)
						{
							ITranslationProviderItem mainTranslationProviderItem2 = cascadeEntryItem2.MainTranslationProviderItem;
							if (Uri.Compare(mainTranslationProviderItem.Uri, mainTranslationProviderItem2.Uri, UriComponents.PathAndQuery | UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(mainTranslationProviderItem.State, mainTranslationProviderItem2.State, StringComparison.InvariantCulture) == 0)
							{
								flag2 = true;
							}
						}
						flag = flag && flag2;
					}
				}
			}
			if (!flag)
			{
				EditCascadeItem(fromCascadeItem, toCascadeItem);
			}
		}

		private void EditCascadeItem(ProjectCascadeItem fromCascadeItem, ProjectCascadeItem toCascadeItem)
		{
			toCascadeItem.BeginEdit();
			toCascadeItem.StopSearchingWhenResultsFound = fromCascadeItem.StopSearchingWhenResultsFound;
			toCascadeItem.OverrideParent = fromCascadeItem.OverrideParent;
			if (!fromCascadeItem.OverrideParent)
			{
				toCascadeItem.EndEdit();
				return;
			}
			UpdateFileBasedTranslationProviderCascadeItems(fromCascadeItem, toCascadeItem);
			toCascadeItem.EndEdit();
		}

		private void UpdateAutoSuggestDictionaries(ILanguageDirection toLanguageDirection, Sdl.ProjectApi.Implementation.Xml.LanguageDirection fromLanguageDirection)
		{
			List<KeyValuePair<IAutoSuggestDictionary, string>> list = new List<KeyValuePair<IAutoSuggestDictionary, string>>();
			for (int i = 0; i < ((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Count; i++)
			{
				IAutoSuggestDictionary val = ((IList<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries)[i];
				if (IsLocal(val.FilePath))
				{
					list.Add(new KeyValuePair<IAutoSuggestDictionary, string>(val, (i > 0) ? ((IList<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries)[i - 1].FilePath : null));
				}
			}
			((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Clear();
			foreach (Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary autoSuggestDictionary in fromLanguageDirection.AutoSuggestDictionaries)
			{
				((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Add(toLanguageDirection.AutoSuggestDictionaries.CreateAutoSuggestDictionary(autoSuggestDictionary.FilePath));
			}
			foreach (KeyValuePair<IAutoSuggestDictionary, string> item in list)
			{
				int num;
				if (item.Value != null && (num = FindAutoSuggestDictionary(toLanguageDirection.AutoSuggestDictionaries, item.Value)) != -1)
				{
					((IList<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Insert(num + 1, item.Key);
				}
				else
				{
					((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Add(item.Key);
				}
			}
		}

		private int FindAutoSuggestDictionary(IAutoSuggestDictionaries items, string filePath)
		{
			for (int i = 0; i < ((ICollection<IAutoSuggestDictionary>)items).Count; i++)
			{
				if (((IList<IAutoSuggestDictionary>)items)[i].FilePath.Equals(filePath))
				{
					return i;
				}
			}
			return -1;
		}

		private bool IsLocal(string autoSuggestDictionaryPath)
		{
			return !autoSuggestDictionaryPath.StartsWith("\\\\");
		}

		private void UpdateFileBasedTranslationProviderCascadeItems(ProjectCascadeItem fromCascadeItem, ProjectCascadeItem toCascadeItem)
		{
			List<KeyValuePair<ProjectCascadeEntryItem, Uri>> fileBasedTMs = new List<KeyValuePair<ProjectCascadeEntryItem, Uri>>();
			Dictionary<Uri, bool> enabledStates = new Dictionary<Uri, bool>();
			FindFileBasedTMs(toCascadeItem, fileBasedTMs, enabledStates);
			toCascadeItem.CascadeEntryItems.Clear();
			UpdateEnabledState(fromCascadeItem, toCascadeItem, enabledStates);
			ReInsertFileBasedTMs(toCascadeItem, fileBasedTMs);
		}

		private void FindFileBasedTMs(ProjectCascadeItem toCascadeItem, List<KeyValuePair<ProjectCascadeEntryItem, Uri>> fileBasedTMs, Dictionary<Uri, bool> enabledStates)
		{
			for (int i = 0; i < toCascadeItem.CascadeEntryItems.Count; i++)
			{
				ProjectCascadeEntryItem val = toCascadeItem.CascadeEntryItems[i];
				if (IsFileBasedProvider(val.MainTranslationProviderItem))
				{
					fileBasedTMs.Add(new KeyValuePair<ProjectCascadeEntryItem, Uri>(val, (i > 0) ? toCascadeItem.CascadeEntryItems[i - 1].MainTranslationProviderItem.Uri : null));
					continue;
				}
				enabledStates[val.MainTranslationProviderItem.Uri] = val.MainTranslationProviderItem.Enabled;
				foreach (TranslationProviderItem item in val.ProjectTranslationProviderItems.OfType<TranslationProviderItem>())
				{
					enabledStates[item.Uri] = item.Enabled;
				}
			}
		}

		private bool IsAnyTmFileBasedProvider(Uri uri)
		{
			return uri.Scheme.Equals("anytm.sdltm.file", StringComparison.OrdinalIgnoreCase);
		}

		private void UpdateEnabledState(ProjectCascadeItem fromCascadeItem, ProjectCascadeItem toCascadeItem, Dictionary<Uri, bool> enabledStates)
		{
			foreach (ProjectCascadeEntryItem item in fromCascadeItem.CascadeEntryItems.Select((ProjectCascadeEntryItem item) => item.Copy()))
			{
				UpdateEnabledState(item.MainTranslationProviderItem, enabledStates);
				foreach (TranslationProviderItem item2 in item.ProjectTranslationProviderItems.OfType<TranslationProviderItem>())
				{
					UpdateEnabledState((ITranslationProviderItem)(object)item2, enabledStates);
				}
				toCascadeItem.CascadeEntryItems.Add(item);
			}
		}

		private void ReInsertFileBasedTMs(ProjectCascadeItem toCascadeItem, List<KeyValuePair<ProjectCascadeEntryItem, Uri>> fileBasedTMs)
		{
			foreach (KeyValuePair<ProjectCascadeEntryItem, Uri> fileBasedTM in fileBasedTMs)
			{
				int num;
				if (fileBasedTM.Value != null && (num = FindCascadeEntryItem(toCascadeItem.CascadeEntryItems, fileBasedTM.Value)) != -1)
				{
					toCascadeItem.CascadeEntryItems.Insert(num + 1, fileBasedTM.Key);
				}
				else
				{
					toCascadeItem.CascadeEntryItems.Add(fileBasedTM.Key);
				}
			}
		}

		private void UpdateEnabledState(ITranslationProviderItem translationProviderItem, Dictionary<Uri, bool> enabledStates)
		{
			translationProviderItem.Enabled = !enabledStates.TryGetValue(translationProviderItem.Uri, out var value) || value;
		}

		private int FindCascadeEntryItem(IList<ProjectCascadeEntryItem> items, Uri uri)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].MainTranslationProviderItem.Uri.Equals(uri))
				{
					return i;
				}
			}
			return -1;
		}

		private bool IsFileBasedProvider(ITranslationProviderItem providerItem)
		{
			if (providerItem != null)
			{
				if (!providerItem.IsFileBasedTranslationMemory())
				{
					return IsAnyTmFileBasedProvider(providerItem.Uri);
				}
				return true;
			}
			return false;
		}

		public void ApplySynchronizationInfo(string name, string description, DateTime? dueDate, string organizationPath, ProjectStatus status, DateTime? completedAt, string customerName, string customerEmail, IProject project)
		{
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected I4, but got Unknown
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			project.ChangeProjectName(name);
			project.Description = description;
			if (dueDate.HasValue && dueDate.Value.Kind != DateTimeKind.Local)
			{
				throw new ArgumentException(ErrorMessages.Project_ApplySynchronizationInfo_expects_dueDate_specified_lical_timezone, "dueDate");
			}
			project.DueDate = dueDate ?? DateTime.MaxValue;
			if (string.IsNullOrEmpty(project.PublishProjectOperation.OrganizationPath) || !string.IsNullOrEmpty(organizationPath))
			{
				project.PublishProjectOperation.OrganizationPath = organizationPath;
			}
			if (status != project.Status)
			{
				switch (status - 2)
				{
				case 0:
					project.Reactivate();
					break;
				case 1:
					(project as Project).MarkAsCompleted(completedAt.Value);
					break;
				case 2:
					project.UpdateStatus(status);
					break;
				}
			}
			ICustomer customer = project.Customer;
			string b = ((customer != null) ? customer.Name : null);
			if (!string.Equals(customerName, b))
			{
				project.ChangeCustomer((ICustomer)(object)((customerName != null) ? new Customer(customerName, customerEmail) : null));
			}
			else if (customerName != null)
			{
				project.Customer.Email = customerEmail;
			}
			((IProjectConfiguration)project).Save();
		}
	}
}
