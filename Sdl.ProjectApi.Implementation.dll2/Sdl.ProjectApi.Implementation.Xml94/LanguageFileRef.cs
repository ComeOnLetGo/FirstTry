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
	public class LanguageFileRef
	{
		private Guid languageFileGuidField = Guid.Empty;

		[XmlAttribute]
		public Guid LanguageFileGuid
		{
			get
			{
				return languageFileGuidField;
			}
			set
			{
				languageFileGuidField = value;
			}
		}
	}
}
