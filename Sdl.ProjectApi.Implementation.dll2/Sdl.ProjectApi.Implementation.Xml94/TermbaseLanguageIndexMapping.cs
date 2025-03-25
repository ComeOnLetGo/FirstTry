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
	public class TermbaseLanguageIndexMapping
	{
		private string languageField;

		private string indexField;

		[XmlElement(Order = 0)]
		public string Language
		{
			get
			{
				return languageField;
			}
			set
			{
				languageField = value;
			}
		}

		[XmlElement(Order = 1)]
		public string Index
		{
			get
			{
				return indexField;
			}
			set
			{
				indexField = value;
			}
		}
	}
}
