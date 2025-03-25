using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class SettingsBundle : GenericItem
	{
		private XmlElement anyField;

		private string nameField;

		[XmlAnyElement(Order = 0)]
		public XmlElement Any
		{
			get
			{
				return anyField;
			}
			set
			{
				anyField = value;
			}
		}

		[XmlAttribute]
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

		public SettingsBundle Copy()
		{
			SettingsBundle settingsBundle = (SettingsBundle)MemberwiseClone();
			settingsBundle.Any = (XmlElement)Any.CloneNode(deep: true);
			return settingsBundle;
		}

		public ISettingsBundle LoadSettingsBundle(ISettingsBundle parent)
		{
			return SettingsUtil.DeserializeSettingsBundle((XmlReader)new XmlNodeReader(Any), parent);
		}

		public void SaveSettingsBundle(ISettingsBundle settingsBundle)
		{
			XmlDocument xmlDocument = new XmlDocument();
			using (XmlNodeWriter xmlNodeWriter = new XmlNodeWriter(xmlDocument, clearCurrentContents: true))
			{
				SettingsUtil.SerializeSettingsBundle((XmlWriter)xmlNodeWriter, settingsBundle);
			}
			Any = xmlDocument.DocumentElement;
		}
	}
}
