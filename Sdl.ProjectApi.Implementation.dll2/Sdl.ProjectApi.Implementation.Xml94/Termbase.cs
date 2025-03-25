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
	public class Termbase
	{
		private string nameField;

		private string settingsXmlField;

		private TermbaseFilter filterField;

		private bool enabledField;

		[XmlElement(Order = 0)]
		public string Name
		{
			get
			{
				return nameField;
			}
			set
			{
				nameField = value;
			}
		}

		[XmlElement(Order = 1)]
		public string SettingsXml
		{
			get
			{
				return settingsXmlField;
			}
			set
			{
				settingsXmlField = value;
			}
		}

		[XmlElement(Order = 2)]
		public TermbaseFilter Filter
		{
			get
			{
				return filterField;
			}
			set
			{
				filterField = value;
			}
		}

		[XmlElement(Order = 3)]
		public bool Enabled
		{
			get
			{
				return enabledField;
			}
			set
			{
				enabledField = value;
			}
		}

		public Termbase()
		{
			enabledField = true;
		}
	}
}
