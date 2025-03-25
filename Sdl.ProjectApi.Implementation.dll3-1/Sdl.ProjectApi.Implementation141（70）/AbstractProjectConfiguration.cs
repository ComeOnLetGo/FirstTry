using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.FileTypeSupport.Framework;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class AbstractProjectConfiguration : IProjectConfiguration, ISettingsBundleProvider, IObjectWithSettings, INotifyPropertyChanged
	{
		private List<ILanguageDirection> _lazyLanguageDirections;

		private IProjectTermbaseConfiguration _lazyTermbaseConfiguration;

		private IAnalysisBand[] _lazyAnalysisBands;

		private IComplexTaskTemplate _lazyStartTaskTemplate;

		private FileTypeConfiguration _lazyFileTypeConfiguration;

		private ITranslationProviderCache _translationProviderCache;

		private ProjectCascadeItem _lazyCascadeItem;

		public IProjectPathUtil PathUtil { get; }

		public abstract IProjectConfigurationRepository Repository { get; }

		public virtual ILanguageDirection[] LanguageDirections => LanguageDirectionsList.ToArray();

		protected List<ILanguageDirection> LanguageDirectionsList
		{
			get
			{
				if (_lazyLanguageDirections == null)
				{
					_lazyLanguageDirections = Repository.GetLanguageDirections((IProjectConfiguration)(object)this);
					_lazyLanguageDirections.ForEach(delegate(ILanguageDirection ld)
					{
						AttachLanguageDirectionEventHandlers(ld);
					});
					ValidateLanguageDirections(_lazyLanguageDirections);
				}
				return _lazyLanguageDirections;
			}
		}

		public IFileTypeConfiguration FileTypeConfiguration => (IFileTypeConfiguration)(object)FileTypeConfigurationImpl;

		internal FileTypeConfiguration FileTypeConfigurationImpl
		{
			get
			{
				if (_lazyFileTypeConfiguration == null)
				{
					_lazyFileTypeConfiguration = new FileTypeConfiguration(this);
					_lazyFileTypeConfiguration.FilterSettingsChanged += AbstractProjectConfiguration_FilterSettingsChanged;
				}
				return _lazyFileTypeConfiguration;
			}
		}

		public ILanguageResourcesTemplate LanguageResources
		{
			get
			{
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected O, but got Unknown
				Repository.ValidateLanguageResources((IProjectConfiguration)(object)this, ProjectsProvider.Application.ServerEvents);
				if (!string.IsNullOrEmpty(Repository.LanguageResourceFilePath))
				{
					return (ILanguageResourcesTemplate)new FileBasedLanguageResourcesTemplate(Repository.LanguageResourceFilePath);
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					Repository.LanguageResourceFilePath = null;
					return;
				}
				FileBasedLanguageResourcesTemplate val = (FileBasedLanguageResourcesTemplate)(object)((value is FileBasedLanguageResourcesTemplate) ? value : null);
				if (val == null)
				{
					throw new ArgumentException("Only file-based language resource groups are currently supported.");
				}
				Repository.LanguageResourceFilePath = val.FilePath;
			}
		}

		public ISettingsBundle Settings
		{
			get
			{
				return GetSettingsBundle(Repository.SettingsBundleGuid, null);
			}
			set
			{
				SettingsBundlesList.ImportSettingsBundle(Repository.SettingsBundleGuid, value);
				SettingsBundlesList.SaveAndClearCache();
			}
		}

		public ISettingsBundlesList SettingsBundlesList => Repository.SettingsBundles;

		public IComplexTaskTemplate StartTaskTemplate
		{
			get
			{
				return _lazyStartTaskTemplate ?? (_lazyStartTaskTemplate = Repository.GetInitialComplexTaskTemplate((IWorkflow)(object)(ProjectsProvider.Workflow as Workflow)));
			}
			set
			{
				_lazyStartTaskTemplate = value;
				Repository.SetInitialComplexTaskTemplate((IComplexTaskTemplate)(object)(_lazyStartTaskTemplate as ComplexTaskTemplate));
			}
		}

		public IProjectTermbaseConfiguration TermbaseConfiguration
		{
			get
			{
				if (_lazyTermbaseConfiguration == null)
				{
					_lazyTermbaseConfiguration = Repository.GetTermbaseConfiguration((IRelativePathManager)(object)((this is IRelativePathManager) ? this : null));
				}
				return _lazyTermbaseConfiguration;
			}
			set
			{
				if (_lazyTermbaseConfiguration == null)
				{
					if (value == null)
					{
						return;
					}
				}
				else if (((object)_lazyTermbaseConfiguration).Equals((object)value))
				{
					return;
				}
				IProjectTermbaseConfiguration lazyTermbaseConfiguration = _lazyTermbaseConfiguration;
				_lazyTermbaseConfiguration = value;
				Repository.SetTermbaseConfiguration(_lazyTermbaseConfiguration, (IRelativePathManager)(object)((this is IRelativePathManager) ? this : null));
				OnTermbaseConfigurationChanged(lazyTermbaseConfiguration, value);
			}
		}

		public IAnalysisBand[] AnalysisBands
		{
			get
			{
				if (_lazyAnalysisBands == null)
				{
					_lazyAnalysisBands = Repository.GetAnalysisBands();
				}
				return _lazyAnalysisBands;
			}
		}

		public ITranslationProviderCache TranslationProviderCache
		{
			get
			{
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				if (_translationProviderCache == null)
				{
					ITranslationProviderCache defaultTranslationProviderCache = ProjectsProvider.Application.DefaultTranslationProviderCache;
					_translationProviderCache = (ITranslationProviderCache)(((object)defaultTranslationProviderCache) ?? ((object)new TranslationProviderCache()));
				}
				return _translationProviderCache;
			}
		}

		public ProjectCascadeItem CascadeItem => _lazyCascadeItem ?? (_lazyCascadeItem = Repository.GetCascadeItem(GetRelativePathManager()));

		public IProjectsProvider ProjectsProvider { get; private set; }

		protected ProjectsProvider ProjectsProviderImp => ProjectsProvider as ProjectsProvider;

		public event EventHandler BeforeSave;

		public event EventHandler<LanguageDirectionEventArgs> LanguageDirectionRemoved;

		public event EventHandler<LanguageDirectionEventArgs> LanguageDirectionAdded;

		public event EventHandler<TermbaseConfigurationChangedEventArgs> TermbaseConfigurationChanged;

		public event EventHandler FilterSettingsChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		public AbstractProjectConfiguration(IProjectsProvider projectsProvider, IProjectPathUtil pathUtil)
		{
			BeforeSave += AbstractProjectConfiguration_BeforeSave;
			PathUtil = pathUtil;
			ProjectsProvider = projectsProvider;
		}

		public abstract void Save();

		public abstract void Check();

		public virtual void DiscardCachedData()
		{
			ResetLazyObjects();
		}

		protected virtual void ResetLazyObjects()
		{
			if (_lazyFileTypeConfiguration != null)
			{
				_lazyFileTypeConfiguration.FilterSettingsChanged -= AbstractProjectConfiguration_FilterSettingsChanged;
			}
			if (_lazyLanguageDirections != null)
			{
				foreach (LanguageDirection lazyLanguageDirection in _lazyLanguageDirections)
				{
					lazyLanguageDirection.Dispose();
					DetachLanguageDirectionEventHandlers((ILanguageDirection)(object)lazyLanguageDirection);
				}
			}
			_lazyLanguageDirections = null;
			_lazyTermbaseConfiguration = null;
			Repository.Reset();
			_lazyAnalysisBands = null;
			_lazyStartTaskTemplate = null;
			_lazyCascadeItem = null;
			_lazyFileTypeConfiguration = null;
		}

		private void ValidateLanguageDirections(List<ILanguageDirection> languageDirections)
		{
			foreach (ILanguageDirection languageDirection in languageDirections)
			{
				RemoveDuplicateEntries(languageDirection.CascadeItem);
			}
		}

		private void RemoveDuplicateEntries(ProjectCascadeItem cascade)
		{
			if (cascade == null)
			{
				return;
			}
			IList<ProjectCascadeEntryItem> list = new List<ProjectCascadeEntryItem>();
			for (int i = 0; i < cascade.CascadeEntryItems.Count; i++)
			{
				if (!list.Contains(cascade.CascadeEntryItems[i]))
				{
					list.Add(cascade.CascadeEntryItems[i]);
					continue;
				}
				cascade.CascadeEntryItems.RemoveAt(i);
				LoggerExtensions.LogWarning(Logging.DefaultLog, "Removing duplicate IProjectTranslationProviderCascade entry", Array.Empty<object>());
				i--;
			}
		}

		public ILanguageDirection GetLanguageDirection(Language sourceLanguage, Language targetLanguage)
		{
			foreach (ILanguageDirection languageDirections in LanguageDirectionsList)
			{
				if (((object)languageDirections.SourceLanguage).Equals((object)sourceLanguage) && ((object)languageDirections.TargetLanguage).Equals((object)targetLanguage))
				{
					return languageDirections;
				}
			}
			return null;
		}

		public ILanguageDirection AddLanguageDirection(Language sourceLanguage, Language targetLanguage)
		{
			if (GetLanguageDirection(sourceLanguage, targetLanguage) != null)
			{
				throw new ArgumentException($"The language direction '{sourceLanguage.DisplayName}->{targetLanguage.DisplayName}' already exists.");
			}
			ILanguageDirection val = Repository.AddLanguageDirection((IProjectConfiguration)(object)this, sourceLanguage, targetLanguage);
			AttachLanguageDirectionEventHandlers(val);
			LanguageDirectionsList.Add((ILanguageDirection)(object)(val as LanguageDirection));
			SettingsBundlesList.AddSettingsBundle(Repository.GetLanguageDirectionSettingsBundleGuid(val), SettingsUtil.CreateSettingsBundle(Settings));
			OnLanguageDirectionAdded(val);
			OnPropertyChanged("LanguageDirections");
			return val;
		}

		public void RemoveLanguageDirection(ILanguageDirection languageDirection)
		{
			Guid languageDirectionSettingsBundleGuid = Repository.GetLanguageDirectionSettingsBundleGuid(languageDirection);
			Repository.RemoveLanguageDirection(languageDirection);
			SettingsBundlesList.RemoveSettingsBundle(languageDirectionSettingsBundleGuid);
			DetachLanguageDirectionEventHandlers(languageDirection);
			LanguageDirectionsList.Remove((ILanguageDirection)(object)(LanguageDirection)(object)languageDirection);
			OnLanguageDirectionRemoved(languageDirection);
			OnPropertyChanged("LanguageDirections");
		}

		protected virtual void OnLanguageDirectionAdded(ILanguageDirection languageDirection)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			this.LanguageDirectionAdded?.Invoke(this, new LanguageDirectionEventArgs(languageDirection));
		}

		protected virtual void OnLanguageDirectionRemoved(ILanguageDirection languageDirection)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			this.LanguageDirectionRemoved?.Invoke(this, new LanguageDirectionEventArgs(languageDirection));
		}

		private void AttachLanguageDirectionEventHandlers(ILanguageDirection languageDirection)
		{
			languageDirection.SourceLanguageChanged += OnSourceLanguageChanged;
			languageDirection.TargetLanguageChanged += OnTargetLanguageChanged;
		}

		private void DetachLanguageDirectionEventHandlers(ILanguageDirection languageDirection)
		{
			languageDirection.SourceLanguageChanged -= OnSourceLanguageChanged;
			languageDirection.TargetLanguageChanged -= OnTargetLanguageChanged;
		}

		protected virtual void OnSourceLanguageChanged(object sender, LanguageDirectionChangeEventArgs e)
		{
		}

		protected virtual void OnTargetLanguageChanged(object sender, LanguageDirectionChangeEventArgs e)
		{
		}

		internal void CopyAutoSuggestDictionaries(ILanguageDirection fromLanguageDirection, ILanguageDirection toLanguageDirection)
		{
			((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Clear();
			foreach (IAutoSuggestDictionary item in (IEnumerable<IAutoSuggestDictionary>)fromLanguageDirection.AutoSuggestDictionaries)
			{
				((ICollection<IAutoSuggestDictionary>)toLanguageDirection.AutoSuggestDictionaries).Add((IAutoSuggestDictionary)(object)new AutoSuggestDictionary(item.FilePath));
			}
		}

		private void AbstractProjectConfiguration_FilterSettingsChanged(object sender, EventArgs e)
		{
			this.FilterSettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void ResetFilterManagerToDefaults()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			IFileTypeDefinition[] fileTypeDefinitions = FileTypeConfiguration.FilterManager.FileTypeDefinitions;
			foreach (IFileTypeDefinition val in fileTypeDefinitions)
			{
				FileTypeDefinitionId fileTypeDefinitionId = val.FileTypeInformation.FileTypeDefinitionId;
				string id = ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id;
				if (Settings.ContainsSettingsGroup(id))
				{
					ISettingsGroup settingsGroup = Settings.GetSettingsGroup(id);
					if (settingsGroup != null)
					{
						settingsGroup.Reset();
					}
				}
			}
			_lazyFileTypeConfiguration?.Discard();
		}

		protected internal string GetAbsoluteSettingsXml(string settingsXml)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			TermbaseSettings val = TermbaseSettings.FromXml(settingsXml);
			if (val.Local && !val.IsCustom && this is IRelativePathManager)
			{
				val.Path = ((IRelativePathManager)this).MakeAbsolutePath(val.Path);
				return val.ToXml();
			}
			return settingsXml;
		}

		protected virtual void OnTermbaseConfigurationChanged(IProjectTermbaseConfiguration oldTermbaseConfiguration, IProjectTermbaseConfiguration newTermbaseConfiguration)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			if (this.TermbaseConfigurationChanged != null)
			{
				TermbaseConfigurationChangedEventArgs e = new TermbaseConfigurationChangedEventArgs(oldTermbaseConfiguration, newTermbaseConfiguration);
				this.TermbaseConfigurationChanged(this, e);
			}
		}

		internal void SetAnalysisBands(IAnalysisBand[] bands)
		{
			int[] array = new int[bands.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = bands[i].MinimumMatchValue;
			}
			SetAnalysisBands(array);
		}

		public virtual void SetAnalysisBands(int[] minimumMatchValues)
		{
			Repository.SetAnalysisBands(minimumMatchValues);
			ResetAnalysisBands();
		}

		public void ResetAnalysisBands()
		{
			_lazyAnalysisBands = null;
			OnPropertyChanged("AnalysisBands");
		}

		public bool VerifyExternalResources()
		{
			return VerifyExternalResources(ProjectsProvider.Application.ServerEvents);
		}

		public bool VerifyExternalResources(IServerEvents serverEvents)
		{
			ValidateLocalTranslationMemoryPaths(serverEvents);
			Repository.ValidateLanguageResources((IProjectConfiguration)(object)this, serverEvents);
			return true;
		}

		private void ValidateLocalTranslationMemoryPaths(IServerEvents serverEvents)
		{
			if (CascadeItem != null)
			{
				ProjectCascadeItemValidator projectCascadeItemValidator = new ProjectCascadeItemValidator((IProjectConfiguration)(object)this, serverEvents);
				projectCascadeItemValidator.Validate(CascadeItem);
			}
			ILanguageDirection[] languageDirections = LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				val.ValidateLocalTranslationMemoryPaths(serverEvents);
			}
		}

		public ISettingsBundle GetSettingsBundle(Guid guid, ISettingsBundle parent)
		{
			return Repository.SettingsBundles.GetSettingsBundle(guid, parent);
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public IManualTaskTemplate GetManualTaskTemplate(string id)
		{
			ITaskTemplate taskTemplateById = ProjectsProvider.Workflow.GetTaskTemplateById(id);
			IManualTaskTemplate val = (IManualTaskTemplate)(object)((taskTemplateById is IManualTaskTemplate) ? taskTemplateById : null);
			if (taskTemplateById != null && val == null)
			{
				throw new ArgumentException("The template with id '" + id + "' is not a manual task template.");
			}
			if (val != null)
			{
				return val;
			}
			return Repository.CreateManualTaskTemplate(id);
		}

		public void AddUsersToCache(IUser[] users)
		{
			Repository.AddUsers(users);
		}

		public void AddUserToCache(IUser user)
		{
			Repository.AddUser(user);
		}

		public IUser GetUserById(string userId)
		{
			IUser userById = ProjectsProvider.UserProvider.GetUserById(userId);
			if (userById != null)
			{
				return userById;
			}
			return Repository.GetUserById(ProjectsProvider, userId);
		}

		public bool HasProjectConfigurationSpecificProviders()
		{
			bool result = false;
			ILanguageDirection[] languageDirections = LanguageDirections;
			foreach (ILanguageDirection val in languageDirections)
			{
				IEnumerable<ITranslationProviderItem> source = val.CascadeItem.CascadeEntryItems.Select((ProjectCascadeEntryItem cs) => cs.MainTranslationProviderItem);
				bool overrideParent = val.CascadeItem.OverrideParent;
				if (source.Any() && overrideParent)
				{
					result = true;
				}
			}
			return result;
		}

		private void AbstractProjectConfiguration_BeforeSave(object sender, EventArgs e)
		{
			if (_lazyCascadeItem != null)
			{
				Repository.SetCascadeItem(GetRelativePathManager(), _lazyCascadeItem);
			}
		}

		private IRelativePathManager GetRelativePathManager()
		{
			return (IRelativePathManager)(object)((this is IRelativePathManager) ? this : null);
		}

		protected void OnBeforeSerialization()
		{
			this.BeforeSave?.Invoke(this, EventArgs.Empty);
		}
	}
}
