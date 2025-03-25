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
	public class ProjectListItem : GenericItem
	{
		private GeneralProjectInfo projectInfoField;

		private SettingsBundle settingsBundleField;

		private string projectFilePathField;

		[XmlElement(Order = 0)]
		public GeneralProjectInfo ProjectInfo
		{
			get
			{
				return projectInfoField;
			}
			set
			{
				projectInfoField = value;
			}
		}

		[XmlElement(Order = 1)]
		public SettingsBundle SettingsBundle
		{
			get
			{
				return settingsBundleField;
			}
			set
			{
				settingsBundleField = value;
			}
		}

		[XmlAttribute]
		public string ProjectFilePath
		{
			get
			{
				return projectFilePathField;
			}
			set
			{
				projectFilePathField = value;
			}
		}
	}
}
