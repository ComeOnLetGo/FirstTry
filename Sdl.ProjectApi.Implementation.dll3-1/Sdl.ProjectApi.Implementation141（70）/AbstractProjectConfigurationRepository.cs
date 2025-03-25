using System;
using System.Collections.Generic;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.TermbaseApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class AbstractProjectConfigurationRepository : IProjectConfigurationRepository
	{
		protected internal readonly IApplication Application;

		private ISettingsBundlesList _lazySettingsBundlesList;

		protected abstract ProjectConfiguration XmlConfiguration { get; }

		public bool IsInitialized { get; private set; }

		public string LanguageResourceFilePath
		{
			get
			{
				return XmlConfiguration.LanguageResourceFilePath;
			}
			set
			{
				XmlConfiguration.LanguageResourceFilePath = value;
			}
		}

		public Guid SettingsBundleGuid
		{
			get
			{
				return XmlConfiguration.SettingsBundleGuid;
			}
			set
			{
				XmlConfiguration.SettingsBundleGuid = value;
			}
		}

		public ISettingsBundlesList SettingsBundles
		{
			get
			{
				if (_lazySettingsBundlesList == null)
				{
					_lazySettingsBundlesList = (ISettingsBundlesList)(object)new SettingsBundlesList(XmlConfiguration.SettingsBundles);
				}
				return _lazySettingsBundlesList;
			}
			set
			{
				_lazySettingsBundlesList = value;
				XmlConfiguration.SettingsBundles = (_lazySettingsBundlesList as SettingsBundlesList).XmlSettingsBundles;
			}
		}

		public AbstractProjectConfigurationRepository(IApplication application)
		{
			Application = application;
		}

		protected void MarkAsInitialized()
		{
			IsInitialized = true;
		}

		public List<ILanguageDirection> GetLanguageDirections(IProjectConfiguration projectConfig)
		{
			List<ILanguageDirection> list = new List<ILanguageDirection>();
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection in XmlConfiguration.LanguageDirections)
			{
				list.Add((ILanguageDirection)(object)new LanguageDirection(projectConfig, languageDirection));
			}
			return list;
		}

		public void RemoveLanguageDirection(ILanguageDirection languageDirection)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection2 in XmlConfiguration.LanguageDirections)
			{
				if (languageDirection2.Guid == languageDirection.Guid)
				{
					XmlConfiguration.LanguageDirections.Remove(languageDirection2);
					break;
				}
			}
		}

		public Guid GetLanguageDirectionSettingsBundleGuid(ILanguageDirection languageDirection)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection2 in XmlConfiguration.LanguageDirections)
			{
				if (languageDirection2.Guid == languageDirection.Guid)
				{
					return languageDirection2.SettingsBundleGuid;
				}
			}
			return default(Guid);
		}

		public ILanguageDirection AddLanguageDirection(IProjectConfiguration projectConfig, Language sourceLanguage, Language targetLanguage)
		{
			Sdl.ProjectApi.Implementation.Xml.LanguageDirection languageDirection = new Sdl.ProjectApi.Implementation.Xml.LanguageDirection();
			languageDirection.SourceLanguageCode = ((LanguageBase)sourceLanguage).IsoAbbreviation;
			languageDirection.TargetLanguageCode = ((LanguageBase)targetLanguage).IsoAbbreviation;
			languageDirection.AssignNewGuid();
			XmlConfiguration.LanguageDirections.Add(languageDirection);
			LanguageDirection result = new LanguageDirection(projectConfig, languageDirection);
			languageDirection.SettingsBundleGuid = Guid.NewGuid();
			return (ILanguageDirection)(object)result;
		}

		public IComplexTaskTemplate GetInitialComplexTaskTemplate(IWorkflow workflow)
		{
			if (XmlConfiguration.InitialTaskTemplate == null)
			{
				return null;
			}
			return (IComplexTaskTemplate)(object)new ComplexTaskTemplate(workflow, XmlConfiguration.InitialTaskTemplate);
		}

		public void SetInitialComplexTaskTemplate(IComplexTaskTemplate complexTaskTemplate)
		{
			XmlConfiguration.InitialTaskTemplate = ((ComplexTaskTemplate)(object)complexTaskTemplate)?.XmlComplexTaskTemplate.Copy();
		}

		public IProjectTermbaseConfiguration GetTermbaseConfiguration(IRelativePathManager pathManager)
		{
			ISettingsBundle settingsBundle = SettingsBundles.GetSettingsBundle(SettingsBundleGuid, (ISettingsBundle)null);
			TerminologyProviderSettings settingsGroup = settingsBundle.GetSettingsGroup<TerminologyProviderSettings>();
			ProjectTermbaseConfigurationProvider projectTermbaseConfigurationProvider = new ProjectTermbaseConfigurationProvider(Application.TerminologyProviderCredentialStore, settingsGroup, pathManager);
			return projectTermbaseConfigurationProvider.GetTermbaseConfiguration(XmlConfiguration.TermbaseConfiguration);
		}

		public void SetTermbaseConfiguration(IProjectTermbaseConfiguration termbaseConfiguration, IRelativePathManager pathManager)
		{
			ISettingsBundle settingsBundle = SettingsBundles.GetSettingsBundle(SettingsBundleGuid, (ISettingsBundle)null);
			TerminologyProviderSettings settingsGroup = settingsBundle.GetSettingsGroup<TerminologyProviderSettings>();
			ProjectTermbaseConfigurationProvider projectTermbaseConfigurationProvider = new ProjectTermbaseConfigurationProvider(Application.TerminologyProviderCredentialStore, settingsGroup, pathManager);
			XmlConfiguration.TermbaseConfiguration = projectTermbaseConfigurationProvider.GetTermbaseConfigurationXml(termbaseConfiguration);
		}

		public virtual IAnalysisBand[] GetAnalysisBands()
		{
			if (XmlConfiguration.AnalysisBands.Count == 0)
			{
				XmlConfiguration.AnalysisBands.Add(new Sdl.ProjectApi.Implementation.Xml.AnalysisBand(50));
				XmlConfiguration.AnalysisBands.Add(new Sdl.ProjectApi.Implementation.Xml.AnalysisBand(75));
				XmlConfiguration.AnalysisBands.Add(new Sdl.ProjectApi.Implementation.Xml.AnalysisBand(85));
				XmlConfiguration.AnalysisBands.Add(new Sdl.ProjectApi.Implementation.Xml.AnalysisBand(95));
			}
			IAnalysisBand[] array = (IAnalysisBand[])(object)new IAnalysisBand[XmlConfiguration.AnalysisBands.Count];
			for (int i = 0; i < array.Length; i++)
			{
				int minimumMatchValue = XmlConfiguration.AnalysisBands[i].MinimumMatchValue;
				int maximumMatchValue = ((i < array.Length - 1) ? (XmlConfiguration.AnalysisBands[i + 1].MinimumMatchValue - 1) : 99);
				array[i] = (IAnalysisBand)(object)new AnalysisBand(minimumMatchValue, maximumMatchValue);
			}
			return array;
		}

		public virtual void SetAnalysisBands(int[] minimumMatchValues)
		{
			if (minimumMatchValues == null)
			{
				throw new ArgumentNullException("minimumMatchValues");
			}
			List<Sdl.ProjectApi.Implementation.Xml.AnalysisBand> list = new List<Sdl.ProjectApi.Implementation.Xml.AnalysisBand>();
			int num = -1;
			for (int i = 0; i < minimumMatchValues.Length; i++)
			{
				if (minimumMatchValues[i] < num + 1 || minimumMatchValues[i] > 99)
				{
					throw new ArgumentOutOfRangeException("minimumMatchValues");
				}
				num = minimumMatchValues[i];
				Sdl.ProjectApi.Implementation.Xml.AnalysisBand analysisBand = new Sdl.ProjectApi.Implementation.Xml.AnalysisBand();
				analysisBand.MinimumMatchValue = minimumMatchValues[i];
				list.Add(analysisBand);
			}
			XmlConfiguration.AnalysisBands = list;
		}

		public void ValidateLanguageResources(IProjectConfiguration projectConfiguration, IServerEvents serverEvents)
		{
			if (!string.IsNullOrEmpty(XmlConfiguration.LanguageResourceFilePath))
			{
				string languageResourcesFilePath = XmlConfiguration.LanguageResourceFilePath;
				if (ValidateLanguageResourcesFilePath(projectConfiguration, serverEvents, ref languageResourcesFilePath))
				{
					XmlConfiguration.LanguageResourceFilePath = languageResourcesFilePath;
				}
				else
				{
					XmlConfiguration.LanguageResourceFilePath = null;
				}
			}
		}

		private bool ValidateLanguageResourcesFilePath(IProjectConfiguration projectConfiguration, IServerEvents serverEvents, ref string languageResourcesFilePath)
		{
			if (File.Exists(languageResourcesFilePath))
			{
				return true;
			}
			string languagesResourcesLocation = serverEvents.GetLanguagesResourcesLocation(projectConfiguration, languageResourcesFilePath);
			if (languagesResourcesLocation != null && File.Exists(languagesResourcesLocation))
			{
				languageResourcesFilePath = languagesResourcesLocation;
				return true;
			}
			return false;
		}

		public IManualTaskTemplate CreateManualTaskTemplate(string id)
		{
			Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate manualTaskTemplate = XmlConfiguration.ManualTaskTemplates.Find((Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate t) => t.Id == id);
			if (manualTaskTemplate == null)
			{
				return (IManualTaskTemplate)(object)new ManualTaskTemplate(new Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate
				{
					Id = id,
					Name = string.Format(StringResources.InvalidManualTask, id),
					IsLocallyExecutable = false
				});
			}
			return (IManualTaskTemplate)(object)new ManualTaskTemplate(manualTaskTemplate);
		}

		public void AddUsers(IUser[] users)
		{
			if (users != null)
			{
				foreach (IUser user in users)
				{
					AddUser(user);
				}
			}
		}

		public void AddUser(IUser user)
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.User user2 = GetXmlUser(user.UserId);
			if (user2 == null)
			{
				user2 = new Sdl.ProjectApi.Implementation.Xml.User();
				user2.UserId = user.UserId;
				XmlConfiguration.Users.Add(user2);
			}
			user2.FullName = user.FullName;
			user2.Description = user.Description;
			user2.PhoneNumber = user.PhoneNumber;
			user2.Email = user.Email;
			user2.EmailType = EnumConvert.ConvertEmailType(user.EmailType);
		}

		public IUser GetUserById(IProjectsProvider projectsProvider, string userId)
		{
			Sdl.ProjectApi.Implementation.Xml.User user = GetXmlUser(userId);
			if (user == null)
			{
				if (string.IsNullOrEmpty(userId))
				{
					return null;
				}
				user = new Sdl.ProjectApi.Implementation.Xml.User();
				user.UserId = userId;
				user.FullName = userId;
			}
			return (IUser)(object)new User(user);
		}

		private Sdl.ProjectApi.Implementation.Xml.User GetXmlUser(string userId)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.User user in XmlConfiguration.Users)
			{
				if (string.Compare(user.UserId, userId, ignoreCase: true) == 0)
				{
					return user;
				}
			}
			return null;
		}

		public ProjectCascadeItem GetCascadeItem(IRelativePathManager pathManager)
		{
			return XmlConfiguration.CascadeItem.ToObject(pathManager);
		}

		public void SetCascadeItem(IRelativePathManager pathManager, ProjectCascadeItem cascadeItem)
		{
			XmlConfiguration.CascadeItem = cascadeItem.ToXml(pathManager);
		}

		public virtual void DiscardData()
		{
			IsInitialized = false;
		}

		public virtual void Reset()
		{
			_lazySettingsBundlesList = null;
		}
	}
}
