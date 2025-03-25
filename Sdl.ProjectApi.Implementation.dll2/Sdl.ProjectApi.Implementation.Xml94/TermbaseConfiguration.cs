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
	public class TermbaseConfiguration
	{
		private TermbaseServer termbaseServerField;

		private List<Termbase> termbasesField = new List<Termbase>();

		private List<TermbaseLanguageIndexMapping> languageIndexMappingsField = new List<TermbaseLanguageIndexMapping>();

		private TermbaseRecognitionOptions recognitionOptionsField;

		[XmlElement(Order = 0)]
		public TermbaseServer TermbaseServer
		{
			get
			{
				return termbaseServerField;
			}
			set
			{
				termbaseServerField = value;
			}
		}

		[XmlElement("Termbases", Order = 1)]
		public List<Termbase> Termbases
		{
			get
			{
				return termbasesField;
			}
			set
			{
				termbasesField = value;
			}
		}

		[XmlElement("LanguageIndexMappings", Order = 2)]
		public List<TermbaseLanguageIndexMapping> LanguageIndexMappings
		{
			get
			{
				return languageIndexMappingsField;
			}
			set
			{
				languageIndexMappingsField = value;
			}
		}

		[XmlElement(Order = 3)]
		public TermbaseRecognitionOptions RecognitionOptions
		{
			get
			{
				return recognitionOptionsField;
			}
			set
			{
				recognitionOptionsField = value;
			}
		}
	}
}
