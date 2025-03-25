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
	public class AnalysisStatistics : WordCountStatistics
	{
		private CountData perfectField;

		private CountData inContextExactField;

		private CountData exactField;

		private List<CountData> fuzzyField = new List<CountData>();

		private CountData newField;

		private CountData lockedField;

		private ValueStatus analysisStatusField;

		private DateTime analysisFileTimeStampField;

		private bool analysisFileTimeStampFieldSpecified;

		[XmlElement(Order = 0)]
		public CountData Perfect
		{
			get
			{
				return perfectField;
			}
			set
			{
				perfectField = value;
			}
		}

		[XmlElement(Order = 1)]
		public CountData InContextExact
		{
			get
			{
				return inContextExactField;
			}
			set
			{
				inContextExactField = value;
			}
		}

		[XmlElement(Order = 2)]
		public CountData Exact
		{
			get
			{
				return exactField;
			}
			set
			{
				exactField = value;
			}
		}

		[XmlArray(Order = 3)]
		[XmlArrayItem(IsNullable = false)]
		public List<CountData> Fuzzy
		{
			get
			{
				return fuzzyField;
			}
			set
			{
				fuzzyField = value;
			}
		}

		[XmlElement(Order = 4)]
		public CountData New
		{
			get
			{
				return newField;
			}
			set
			{
				newField = value;
			}
		}

		[XmlElement(Order = 5)]
		public CountData Locked
		{
			get
			{
				return lockedField;
			}
			set
			{
				lockedField = value;
			}
		}

		[XmlAttribute]
		public ValueStatus AnalysisStatus
		{
			get
			{
				return analysisStatusField;
			}
			set
			{
				analysisStatusField = value;
			}
		}

		[XmlAttribute]
		public DateTime AnalysisFileTimeStamp
		{
			get
			{
				return analysisFileTimeStampField;
			}
			set
			{
				analysisFileTimeStampField = value;
			}
		}

		[XmlIgnore]
		public bool AnalysisFileTimeStampSpecified
		{
			get
			{
				return analysisFileTimeStampFieldSpecified;
			}
			set
			{
				analysisFileTimeStampFieldSpecified = value;
			}
		}
	}
}
