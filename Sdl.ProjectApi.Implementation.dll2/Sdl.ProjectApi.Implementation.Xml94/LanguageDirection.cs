using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class LanguageDirection : GenericItemWithSettings
	{
		private List<AutoSuggestDictionary> autoSuggestDictionariesField = new List<AutoSuggestDictionary>();

		private AnalysisStatistics analysisStatisticsField;

		private ConfirmationStatistics confirmationStatisticsField;

		private CascadeItem cascadeItemField;

		private string sourceLanguageCodeField;

		private string targetLanguageCodeField;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<AutoSuggestDictionary> AutoSuggestDictionaries
		{
			get
			{
				return autoSuggestDictionariesField;
			}
			set
			{
				autoSuggestDictionariesField = value;
			}
		}

		[XmlElement(Order = 1)]
		public AnalysisStatistics AnalysisStatistics
		{
			get
			{
				return analysisStatisticsField;
			}
			set
			{
				analysisStatisticsField = value;
			}
		}

		[XmlElement(Order = 2)]
		public ConfirmationStatistics ConfirmationStatistics
		{
			get
			{
				return confirmationStatisticsField;
			}
			set
			{
				confirmationStatisticsField = value;
			}
		}

		[XmlElement(Order = 3)]
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
		public string SourceLanguageCode
		{
			get
			{
				return sourceLanguageCodeField;
			}
			set
			{
				sourceLanguageCodeField = value;
			}
		}

		[XmlAttribute]
		public string TargetLanguageCode
		{
			get
			{
				return targetLanguageCodeField;
			}
			set
			{
				targetLanguageCodeField = value;
			}
		}

		public LanguageDirection()
		{
			CascadeItem = new CascadeItem();
		}

		public LanguageDirection Copy()
		{
			LanguageDirection languageDirection = (LanguageDirection)MemberwiseClone();
			languageDirection.AnalysisStatistics = null;
			languageDirection.ConfirmationStatistics = null;
			if (CascadeItem != null)
			{
				languageDirection.CascadeItem = CascadeItem.Copy();
			}
			return languageDirection;
		}
	}
}
