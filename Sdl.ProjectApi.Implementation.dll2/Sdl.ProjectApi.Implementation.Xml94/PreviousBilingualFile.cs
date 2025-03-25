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
	public class PreviousBilingualFile
	{
		private string targetLanguageCodeField;

		private string physicalPathField;

		private bool isReviewedField;

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

		[XmlAttribute]
		public string PhysicalPath
		{
			get
			{
				return physicalPathField;
			}
			set
			{
				physicalPathField = value;
			}
		}

		[XmlAttribute]
		public bool IsReviewed
		{
			get
			{
				return isReviewedField;
			}
			set
			{
				isReviewedField = value;
			}
		}
	}
}
