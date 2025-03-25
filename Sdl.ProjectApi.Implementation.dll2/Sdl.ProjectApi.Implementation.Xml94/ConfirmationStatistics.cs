using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class ConfirmationStatistics
	{
		private CountData unspecifiedField;

		private CountData draftField;

		private CountData translatedField;

		private CountData rejectedTranslationField;

		private CountData approvedTranslationField;

		private CountData rejectedSignOffField;

		private CountData approvedSignOffField;

		private ValueStatus statusField;

		private DateTime fileTimeStampField;

		private bool fileTimeStampFieldSpecified;

		[XmlElement(Order = 0)]
		public CountData Unspecified
		{
			get
			{
				return unspecifiedField;
			}
			set
			{
				unspecifiedField = value;
			}
		}

		[XmlElement(Order = 1)]
		public CountData Draft
		{
			get
			{
				return draftField;
			}
			set
			{
				draftField = value;
			}
		}

		[XmlElement(Order = 2)]
		public CountData Translated
		{
			get
			{
				return translatedField;
			}
			set
			{
				translatedField = value;
			}
		}

		[XmlElement(Order = 3)]
		public CountData RejectedTranslation
		{
			get
			{
				return rejectedTranslationField;
			}
			set
			{
				rejectedTranslationField = value;
			}
		}

		[XmlElement(Order = 4)]
		public CountData ApprovedTranslation
		{
			get
			{
				return approvedTranslationField;
			}
			set
			{
				approvedTranslationField = value;
			}
		}

		[XmlElement(Order = 5)]
		public CountData RejectedSignOff
		{
			get
			{
				return rejectedSignOffField;
			}
			set
			{
				rejectedSignOffField = value;
			}
		}

		[XmlElement(Order = 6)]
		public CountData ApprovedSignOff
		{
			get
			{
				return approvedSignOffField;
			}
			set
			{
				approvedSignOffField = value;
			}
		}

		[XmlAttribute]
		public ValueStatus Status
		{
			get
			{
				return statusField;
			}
			set
			{
				statusField = value;
			}
		}

		[XmlAttribute]
		public DateTime FileTimeStamp
		{
			get
			{
				return fileTimeStampField;
			}
			set
			{
				fileTimeStampField = value;
			}
		}

		[XmlIgnore]
		public bool FileTimeStampSpecified
		{
			get
			{
				return fileTimeStampFieldSpecified;
			}
			set
			{
				fileTimeStampFieldSpecified = value;
			}
		}
	}
}
