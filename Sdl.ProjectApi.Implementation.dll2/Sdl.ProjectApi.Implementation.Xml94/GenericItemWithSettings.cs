using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(ProjectConfiguration))]
	[XmlInclude(typeof(Project))]
	[XmlInclude(typeof(LanguageFile))]
	[XmlInclude(typeof(LanguageDirection))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class GenericItemWithSettings : GenericItem
	{
		private Guid settingsBundleGuidField = Guid.Empty;

		[XmlAttribute]
		public Guid SettingsBundleGuid
		{
			get
			{
				return settingsBundleGuidField;
			}
			set
			{
				settingsBundleGuidField = value;
			}
		}
	}
}
