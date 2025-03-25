using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(AnalysisStatistics))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class WordCountStatistics
	{
		private CountData totalField;

		private CountData repetitionsField;

		private ValueStatus wordCountStatusField;

		private DateTime wordCountFileTimeStampField;

		private bool wordCountFileTimeStampFieldSpecified;

		[XmlElement(Order = 0)]
		public CountData Total
		{
			get
			{
				return totalField;
			}
			set
			{
				totalField = value;
			}
		}

		[XmlElement(Order = 1)]
		public CountData Repetitions
		{
			get
			{
				return repetitionsField;
			}
			set
			{
				repetitionsField = value;
			}
		}

		[XmlAttribute]
		public ValueStatus WordCountStatus
		{
			get
			{
				return wordCountStatusField;
			}
			set
			{
				wordCountStatusField = value;
			}
		}

		[XmlAttribute]
		public DateTime WordCountFileTimeStamp
		{
			get
			{
				return wordCountFileTimeStampField;
			}
			set
			{
				wordCountFileTimeStampField = value;
			}
		}

		[XmlIgnore]
		public bool WordCountFileTimeStampSpecified
		{
			get
			{
				return wordCountFileTimeStampFieldSpecified;
			}
			set
			{
				wordCountFileTimeStampFieldSpecified = value;
			}
		}
	}
}
