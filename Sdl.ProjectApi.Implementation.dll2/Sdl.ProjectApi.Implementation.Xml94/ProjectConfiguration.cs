using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using Sdl.Core.Globalization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(Project))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class ProjectConfiguration : GenericItemWithSettings
	{
		private List<LanguageDirection> languageDirectionsField = new List<LanguageDirection>();

		private TermbaseConfiguration termbaseConfigurationField;

		private List<SettingsBundle> settingsBundlesField = new List<SettingsBundle>();

		private ComplexTaskTemplate initialTaskTemplateField;

		private List<AnalysisBand> analysisBandsField = new List<AnalysisBand>();

		private List<ManualTaskTemplate> manualTaskTemplatesField = new List<ManualTaskTemplate>();

		private List<User> usersField = new List<User>();

		private CascadeItem cascadeItemField;

		private string languageResourceFilePathField;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<LanguageDirection> LanguageDirections
		{
			get
			{
				return languageDirectionsField;
			}
			set
			{
				languageDirectionsField = value;
			}
		}

		[XmlElement(Order = 1)]
		public TermbaseConfiguration TermbaseConfiguration
		{
			get
			{
				return termbaseConfigurationField;
			}
			set
			{
				termbaseConfigurationField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<SettingsBundle> SettingsBundles
		{
			get
			{
				return settingsBundlesField;
			}
			set
			{
				settingsBundlesField = value;
			}
		}

		[XmlElement(Order = 3)]
		public ComplexTaskTemplate InitialTaskTemplate
		{
			get
			{
				return initialTaskTemplateField;
			}
			set
			{
				initialTaskTemplateField = value;
			}
		}

		[XmlArray(Order = 4)]
		[XmlArrayItem(IsNullable = false)]
		public List<AnalysisBand> AnalysisBands
		{
			get
			{
				return analysisBandsField;
			}
			set
			{
				analysisBandsField = value;
			}
		}

		[XmlArray(Order = 5)]
		[XmlArrayItem(IsNullable = false)]
		public List<ManualTaskTemplate> ManualTaskTemplates
		{
			get
			{
				return manualTaskTemplatesField;
			}
			set
			{
				manualTaskTemplatesField = value;
			}
		}

		[XmlArray(Order = 6)]
		[XmlArrayItem(IsNullable = false)]
		public List<User> Users
		{
			get
			{
				return usersField;
			}
			set
			{
				usersField = value;
			}
		}

		[XmlElement(Order = 7)]
		public CascadeItem CascadeItem
		{
			get
			{
				return cascadeItemField;
			}
			set
			{
				cascadeItemField = value;
			}
		}

		[XmlAttribute]
		public string LanguageResourceFilePath
		{
			get
			{
				return languageResourceFilePathField;
			}
			set
			{
				languageResourceFilePathField = value;
			}
		}

		public ProjectConfiguration()
		{
			CascadeItem = new CascadeItem();
		}

		public LanguageDirection FindLanguageDirection(string sourceLanguageCode, string targetLanguageCode)
		{
			foreach (LanguageDirection languageDirection in LanguageDirections)
			{
				if (LanguageBase.Equals(languageDirection.SourceLanguageCode, sourceLanguageCode) && LanguageBase.Equals(languageDirection.TargetLanguageCode, targetLanguageCode))
				{
					return languageDirection;
				}
			}
			return null;
		}

		public SettingsBundle FindSettingsBundle(Guid guid)
		{
			return SettingsBundles.Find(new GuidPredicate(guid).MatchGuid);
		}
	}
}
