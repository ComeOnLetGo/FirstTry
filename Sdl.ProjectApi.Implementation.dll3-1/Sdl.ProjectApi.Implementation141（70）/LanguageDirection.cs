using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class LanguageDirection : ILanguageDirection, IObjectWithSettings, IDisposable
	{
		private IProjectConfiguration _projectConfig;

		private Language _sourceLanguage;

		private Language _targetLanguage;

		private int _eventsSuspended;

		private bool _shouldRaiseTranslationMemoriesChanged;

		private IAutoSuggestDictionaries _lazyAutoSuggestDictionaries;

		private ProjectCascadeItem _lazyCascadeItem;

		public Guid Guid => XmlLanguageDirection.Guid;

		public Guid SettingsBundleGuid => XmlLanguageDirection.SettingsBundleGuid;

		public IProjectConfiguration Configuration => _projectConfig;

		public Language SourceLanguage
		{
			get
			{
				return _sourceLanguage;
			}
			set
			{
				Language sourceLanguage = _sourceLanguage;
				SuspendEvents();
				CascadeItem.BeginEdit();
				CascadeItem.CascadeEntryItems.Clear();
				CascadeItem.EndEdit();
				_sourceLanguage = value;
				XmlLanguageDirection.SourceLanguageCode = ((LanguageBase)value).IsoAbbreviation;
				ResumeEvents(raiseEvents: false);
				OnSourceLanguageChanged((ILanguageDirection)(object)this, sourceLanguage);
			}
		}

		public Language TargetLanguage
		{
			get
			{
				return _targetLanguage;
			}
			set
			{
				Language targetLanguage = _targetLanguage;
				SuspendEvents();
				CascadeItem.BeginEdit();
				CascadeItem.CascadeEntryItems.Clear();
				CascadeItem.EndEdit();
				_targetLanguage = value;
				XmlLanguageDirection.TargetLanguageCode = ((LanguageBase)value).IsoAbbreviation;
				ResumeEvents(raiseEvents: false);
				OnTargetLanguageChanged((ILanguageDirection)(object)this, targetLanguage);
			}
		}

		public ISettingsBundle Settings
		{
			get
			{
				if (XmlLanguageDirection.SettingsBundleGuid == Guid.Empty)
				{
					XmlLanguageDirection.SettingsBundleGuid = Guid.NewGuid();
				}
				return _projectConfig.SettingsBundlesList.GetSettingsBundle(XmlLanguageDirection.SettingsBundleGuid, ((IObjectWithSettings)_projectConfig).Settings);
			}
			set
			{
				if (XmlLanguageDirection.SettingsBundleGuid == Guid.Empty)
				{
					XmlLanguageDirection.SettingsBundleGuid = Guid.NewGuid();
				}
				_projectConfig.SettingsBundlesList.ImportSettingsBundle(XmlLanguageDirection.SettingsBundleGuid, value);
				_projectConfig.SettingsBundlesList.SaveAndClearCache();
			}
		}

		public bool EventsSuspended => _eventsSuspended > 0;

		public Sdl.ProjectApi.Implementation.Statistics.AnalysisStatistics AnalysisStatistics
		{
			get
			{
				if (!(Configuration is Project project))
				{
					throw new InvalidOperationException("This property is only valid for project language directions.");
				}
				XmlLanguageDirection.AnalysisStatistics = ComputeAnalysisStatistics((IProject)(object)project, project.GetTranslatableFiles(TargetLanguage));
				return new Sdl.ProjectApi.Implementation.Statistics.AnalysisStatistics((IProject)(object)project, (ILanguageDirection)(object)this, null, XmlLanguageDirection.AnalysisStatistics, canUpdate: false);
			}
		}

		internal ConfirmationStatisticsRepository ConfirmationStatistics
		{
			get
			{
				if (!(Configuration is Project project))
				{
					throw new InvalidOperationException("This property is only valid for project language directions.");
				}
				XmlLanguageDirection.ConfirmationStatistics = ComputeConfirmationStatistics(project.GetTranslatableFiles(TargetLanguage));
				return new ConfirmationStatisticsRepository((ILanguageDirection)(object)this, null, XmlLanguageDirection.ConfirmationStatistics, canUpdate: false);
			}
		}

		internal Sdl.ProjectApi.Implementation.Xml.LanguageDirection XmlLanguageDirection { get; }

		public IAutoSuggestDictionaries AutoSuggestDictionaries => _lazyAutoSuggestDictionaries ?? (_lazyAutoSuggestDictionaries = DeserializeAutoSuggestDictionaries(XmlLanguageDirection.AutoSuggestDictionaries));

		public ProjectCascadeItem CascadeItem
		{
			get
			{
				if (_lazyCascadeItem == null)
				{
					ref ProjectCascadeItem lazyCascadeItem = ref _lazyCascadeItem;
					CascadeItem cascadeItem = XmlLanguageDirection.CascadeItem;
					IProjectConfiguration configuration = Configuration;
					lazyCascadeItem = cascadeItem.ToObject((IRelativePathManager)(object)((configuration is IRelativePathManager) ? configuration : null));
				}
				return _lazyCascadeItem;
			}
		}

		public event EventHandler<LanguageDirectionChangeEventArgs> SourceLanguageChanged;

		public event EventHandler<LanguageDirectionChangeEventArgs> TargetLanguageChanged;

		public event EventHandler TranslationMemoryProviderCascadeChanged;

		public LanguageDirection(IProjectConfiguration projectConfig, Sdl.ProjectApi.Implementation.Xml.LanguageDirection xmlLanguageDirection)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			_projectConfig = projectConfig;
			_projectConfig.BeforeSave += _projectConfig_BeforeSave;
			XmlLanguageDirection = xmlLanguageDirection;
			_sourceLanguage = new Language(xmlLanguageDirection.SourceLanguageCode);
			_targetLanguage = new Language(xmlLanguageDirection.TargetLanguageCode);
		}

		public void ValidateLocalTranslationMemoryPaths(IServerEvents serverEvents)
		{
			if (CascadeItem != null)
			{
				ProjectCascadeItemValidator projectCascadeItemValidator = new ProjectCascadeItemValidator(Configuration, serverEvents);
				projectCascadeItemValidator.Validate(CascadeItem);
			}
		}

		private string GetAbsoluteFilePath(string relativeFilePath)
		{
			IProjectConfiguration configuration = Configuration;
			IRelativePathManager val = (IRelativePathManager)(object)((configuration is IRelativePathManager) ? configuration : null);
			if (val != null)
			{
				return val.MakeAbsolutePath(relativeFilePath);
			}
			return relativeFilePath;
		}

		private string GetRelativeFilePath(string absoluteFilePath)
		{
			IProjectConfiguration configuration = Configuration;
			IRelativePathManager val = (IRelativePathManager)(object)((configuration is IRelativePathManager) ? configuration : null);
			if (val != null)
			{
				return val.MakeRelativePath(absoluteFilePath);
			}
			return absoluteFilePath;
		}

		public void SuspendEvents()
		{
			_eventsSuspended++;
		}

		public bool ResumeEvents(bool raiseEvents)
		{
			if (_eventsSuspended > 0)
			{
				_eventsSuspended--;
				if (_eventsSuspended == 0 && _shouldRaiseTranslationMemoriesChanged && raiseEvents)
				{
					OnTranslationMemoryProviderCascadeChanged();
				}
			}
			return _eventsSuspended == 0;
		}

		public void InitializeSettingsFromConfiguration(IProjectConfiguration projectConfig, bool copyTms)
		{
			ILanguageDirection languageDirection = projectConfig.GetLanguageDirection(SourceLanguage, TargetLanguage);
			if (languageDirection != null)
			{
				CascadeItem.CascadeEntryItems.Clear();
				if (copyTms)
				{
					CascadeItem.Update(languageDirection.CascadeItem);
				}
				Settings = ((IObjectWithSettings)languageDirection).Settings;
			}
			else
			{
				_projectConfig.SettingsBundlesList.DiscardCachedSettingsBundle(XmlLanguageDirection.SettingsBundleGuid);
			}
		}

		private static Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics ComputeAnalysisStatistics(IProject project, ITranslatableFile[] files)
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics analysisStatistics = new Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics();
			analysisStatistics.Exact = new Sdl.ProjectApi.Implementation.Xml.CountData();
			IAnalysisBand[] analysisBands = ((IProjectConfiguration)project).AnalysisBands;
			foreach (IAnalysisBand val in analysisBands)
			{
				analysisStatistics.Fuzzy.Add(new Sdl.ProjectApi.Implementation.Xml.CountData());
			}
			analysisStatistics.InContextExact = new Sdl.ProjectApi.Implementation.Xml.CountData();
			analysisStatistics.Perfect = new Sdl.ProjectApi.Implementation.Xml.CountData();
			analysisStatistics.New = new Sdl.ProjectApi.Implementation.Xml.CountData();
			analysisStatistics.Repetitions = new Sdl.ProjectApi.Implementation.Xml.CountData();
			analysisStatistics.Total = new Sdl.ProjectApi.Implementation.Xml.CountData();
			analysisStatistics.Locked = new Sdl.ProjectApi.Implementation.Xml.CountData();
			ValueStatus val2 = (ValueStatus)3;
			ValueStatus val3 = (ValueStatus)3;
			bool flag = false;
			bool flag2 = false;
			foreach (ITranslatableFile val4 in files)
			{
				IAnalysisStatistics analysisStatistics2 = val4.AnalysisStatistics;
				ValueStatus wordCountStatus = ((IWordCountStatistics)analysisStatistics2).WordCountStatus;
				val2 = val2.CombineValueStatus(wordCountStatus);
				if ((int)wordCountStatus != 0)
				{
					Increment(analysisStatistics.Total, ((IWordCountStatistics)analysisStatistics2).Total);
					Increment(analysisStatistics.Repetitions, ((IWordCountStatistics)analysisStatistics2).Repetitions);
					flag = true;
				}
				ValueStatus analysisStatus = analysisStatistics2.AnalysisStatus;
				val3 = val3.CombineValueStatus(analysisStatus);
				if ((int)analysisStatus != 0)
				{
					Increment(analysisStatistics.Exact, analysisStatistics2.Exact);
					Increment(analysisStatistics.InContextExact, analysisStatistics2.InContextExact);
					Increment(analysisStatistics.New, analysisStatistics2.New);
					Increment(analysisStatistics.Locked, analysisStatistics2.Locked);
					Increment(analysisStatistics.Perfect, analysisStatistics2.Perfect);
					for (int k = 0; k < analysisStatistics.Fuzzy.Count; k++)
					{
						Increment(analysisStatistics.Fuzzy[k], (ICountData)(object)analysisStatistics2.Fuzzy[k]);
					}
					flag2 = true;
				}
			}
			analysisStatistics.WordCountStatus = (flag ? EnumConvert.ConvertValueStatus(val2) : ValueStatus.None);
			analysisStatistics.AnalysisStatus = (flag2 ? EnumConvert.ConvertValueStatus(val3) : ValueStatus.None);
			return analysisStatistics;
		}

		private static Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics ComputeConfirmationStatistics(ITranslatableFile[] files)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics confirmationStatistics = new Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics();
			confirmationStatistics.Unspecified = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.Draft = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.Translated = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.RejectedTranslation = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.ApprovedTranslation = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.RejectedSignOff = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.ApprovedSignOff = new Sdl.ProjectApi.Implementation.Xml.CountData();
			confirmationStatistics.Status = ValueStatus.None;
			ValueStatus val = (ValueStatus)3;
			bool flag = false;
			foreach (ITranslatableFile val2 in files)
			{
				IConfirmationStatistics confirmationStatistics2 = val2.ConfirmationStatistics;
				ValueStatus status = confirmationStatistics2.Status;
				val = val.CombineValueStatus(status);
				if ((int)status != 0)
				{
					Increment(confirmationStatistics.Unspecified, confirmationStatistics2[(ConfirmationLevel)0]);
					Increment(confirmationStatistics.Draft, confirmationStatistics2[(ConfirmationLevel)1]);
					Increment(confirmationStatistics.Translated, confirmationStatistics2[(ConfirmationLevel)2]);
					Increment(confirmationStatistics.RejectedTranslation, confirmationStatistics2[(ConfirmationLevel)3]);
					Increment(confirmationStatistics.ApprovedTranslation, confirmationStatistics2[(ConfirmationLevel)4]);
					Increment(confirmationStatistics.RejectedSignOff, confirmationStatistics2[(ConfirmationLevel)5]);
					Increment(confirmationStatistics.ApprovedSignOff, confirmationStatistics2[(ConfirmationLevel)6]);
					flag = true;
				}
			}
			if (!flag && files.Length != 0)
			{
				confirmationStatistics.Status = ValueStatus.None;
			}
			else
			{
				confirmationStatistics.Status = EnumConvert.ConvertValueStatus(val);
			}
			return confirmationStatistics;
		}

		private static void Increment(Sdl.ProjectApi.Implementation.Xml.CountData data, ICountData update)
		{
			data.Characters += update.Characters;
			data.Words += update.Words;
			data.Segments += update.Segments;
			data.Placeables += update.Placeables;
			data.Tags += update.Tags;
		}

		internal void NotifyAnalysisStatisticsChanged()
		{
			XmlLanguageDirection.AnalysisStatistics = null;
		}

		internal void NotifyConfirmationLevelStatisticsChanged()
		{
			XmlLanguageDirection.ConfirmationStatistics = null;
		}

		public override string ToString()
		{
			return $"{_sourceLanguage.DisplayName}->{_targetLanguage.DisplayName}";
		}

		public override bool Equals(object obj)
		{
			ILanguageDirection val = (ILanguageDirection)((obj is ILanguageDirection) ? obj : null);
			if (val == null)
			{
				return false;
			}
			if (LanguageBase.Equals((LanguageBase)(object)val.SourceLanguage, (LanguageBase)(object)SourceLanguage))
			{
				return LanguageBase.Equals((LanguageBase)(object)val.TargetLanguage, (LanguageBase)(object)TargetLanguage);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		protected virtual void OnSourceLanguageChanged(ILanguageDirection languageDirection, Language oldSourceLanguage)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (this.SourceLanguageChanged != null)
			{
				this.SourceLanguageChanged(this, new LanguageDirectionChangeEventArgs(languageDirection, oldSourceLanguage));
			}
		}

		protected virtual void OnTargetLanguageChanged(ILanguageDirection languageDirection, Language oldTargetLanguage)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (this.TargetLanguageChanged != null)
			{
				this.TargetLanguageChanged(this, new LanguageDirectionChangeEventArgs(languageDirection, oldTargetLanguage));
			}
		}

		internal void OnTranslationMemoryProviderCascadeChanged()
		{
			if (!EventsSuspended)
			{
				_shouldRaiseTranslationMemoriesChanged = false;
				if (this.TranslationMemoryProviderCascadeChanged != null)
				{
					this.TranslationMemoryProviderCascadeChanged(this, EventArgs.Empty);
				}
			}
			else
			{
				_shouldRaiseTranslationMemoriesChanged = true;
			}
		}

		private void _projectConfig_BeforeSave(object sender, EventArgs e)
		{
			Serialize();
		}

		private void Serialize()
		{
			if (_lazyCascadeItem != null)
			{
				Sdl.ProjectApi.Implementation.Xml.LanguageDirection xmlLanguageDirection = XmlLanguageDirection;
				ProjectCascadeItem lazyCascadeItem = _lazyCascadeItem;
				IProjectConfiguration configuration = Configuration;
				xmlLanguageDirection.CascadeItem = lazyCascadeItem.ToXml((IRelativePathManager)(object)((configuration is IRelativePathManager) ? configuration : null));
			}
			if (_lazyAutoSuggestDictionaries != null)
			{
				XmlLanguageDirection.AutoSuggestDictionaries = SerializeAutoSuggestDictionaries(_lazyAutoSuggestDictionaries);
			}
		}

		private List<Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary> SerializeAutoSuggestDictionaries(IAutoSuggestDictionaries autoSuggestDictionaries)
		{
			List<Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary> list = new List<Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary>();
			if (autoSuggestDictionaries != null)
			{
				list.AddRange(from autoSuggestDictionary in (IEnumerable<IAutoSuggestDictionary>)autoSuggestDictionaries
					select autoSuggestDictionary.FilePath into absoluteFilePath
					select GetRelativeFilePath(absoluteFilePath) into relativeFilePath
					select new Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary
					{
						FilePath = relativeFilePath
					});
			}
			return list;
		}

		private IAutoSuggestDictionaries DeserializeAutoSuggestDictionaries(IEnumerable<Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary> xmlAutoSuggestDictionaries)
		{
			IAutoSuggestDictionaries val = (IAutoSuggestDictionaries)(object)new AutoSuggestDictionaries();
			foreach (Sdl.ProjectApi.Implementation.Xml.AutoSuggestDictionary xmlAutoSuggestDictionary in xmlAutoSuggestDictionaries)
			{
				string filePath = xmlAutoSuggestDictionary.FilePath;
				string absoluteFilePath = GetAbsoluteFilePath(filePath);
				IAutoSuggestDictionary item = val.CreateAutoSuggestDictionary(absoluteFilePath);
				((ICollection<IAutoSuggestDictionary>)val).Add(item);
			}
			return val;
		}

		public void Dispose()
		{
			_projectConfig.BeforeSave -= _projectConfig_BeforeSave;
		}
	}
}
