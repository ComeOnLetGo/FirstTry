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
	public class LanguageFile : GenericItemWithSettings
	{
		private List<FileVersion> fileVersionsField = new List<FileVersion>();

		private AnalysisStatistics analysisStatisticsField;

		private ConfirmationStatistics confirmationStatisticsField;

		private TQAData tQADataField;

		private List<LanguageFileRef> childFilesField = new List<LanguageFileRef>();

		private string languageCodeField;

		private MergeState mergeStateField;

		private bool mergeStateFieldSpecified;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<FileVersion> FileVersions
		{
			get
			{
				return fileVersionsField;
			}
			set
			{
				fileVersionsField = value;
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
		public TQAData TQAData
		{
			get
			{
				return tQADataField;
			}
			set
			{
				tQADataField = value;
			}
		}

		[XmlArray(Order = 4)]
		[XmlArrayItem(IsNullable = false)]
		public List<LanguageFileRef> ChildFiles
		{
			get
			{
				return childFilesField;
			}
			set
			{
				childFilesField = value;
			}
		}

		[XmlAttribute]
		public string LanguageCode
		{
			get
			{
				return languageCodeField;
			}
			set
			{
				languageCodeField = value;
			}
		}

		[XmlAttribute]
		public MergeState MergeState
		{
			get
			{
				return mergeStateField;
			}
			set
			{
				mergeStateField = value;
			}
		}

		[XmlIgnore]
		public bool MergeStateSpecified
		{
			get
			{
				return mergeStateFieldSpecified;
			}
			set
			{
				mergeStateFieldSpecified = value;
			}
		}
	}
}
