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
	public class TermbaseRecognitionOptions
	{
		private bool showWithNoAvailableTranslationField;

		private bool showWithNoAvailableTranslationFieldSpecified;

		private int minimumMatchValueField;

		private int searchDepthField;

		private TermbaseSearchOrder searchOrderField;

		private bool searchOrderFieldSpecified;

		[XmlElement(Order = 0)]
		public bool ShowWithNoAvailableTranslation
		{
			get
			{
				return showWithNoAvailableTranslationField;
			}
			set
			{
				showWithNoAvailableTranslationField = value;
			}
		}

		[XmlIgnore]
		public bool ShowWithNoAvailableTranslationSpecified
		{
			get
			{
				return showWithNoAvailableTranslationFieldSpecified;
			}
			set
			{
				showWithNoAvailableTranslationFieldSpecified = value;
			}
		}

		[XmlElement(Order = 1)]
		public int MinimumMatchValue
		{
			get
			{
				return minimumMatchValueField;
			}
			set
			{
				minimumMatchValueField = value;
			}
		}

		[XmlElement(Order = 2)]
		public int SearchDepth
		{
			get
			{
				return searchDepthField;
			}
			set
			{
				searchDepthField = value;
			}
		}

		[XmlElement(Order = 3)]
		public TermbaseSearchOrder SearchOrder
		{
			get
			{
				return searchOrderField;
			}
			set
			{
				searchOrderField = value;
			}
		}

		[XmlIgnore]
		public bool SearchOrderSpecified
		{
			get
			{
				return searchOrderFieldSpecified;
			}
			set
			{
				searchOrderFieldSpecified = value;
			}
		}
	}
}
