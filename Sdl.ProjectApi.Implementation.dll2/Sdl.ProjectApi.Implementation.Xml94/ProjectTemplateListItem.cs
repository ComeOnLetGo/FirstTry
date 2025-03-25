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
	public class ProjectTemplateListItem : GenericItem
	{
		private GeneralProjectTemplateInfo projectTemplateInfoField;

		private string projectTemplateFilePathField;

		[XmlElement(Order = 0)]
		public GeneralProjectTemplateInfo ProjectTemplateInfo
		{
			get
			{
				return projectTemplateInfoField;
			}
			set
			{
				projectTemplateInfoField = value;
			}
		}

		[XmlAttribute]
		public string ProjectTemplateFilePath
		{
			get
			{
				return projectTemplateFilePathField;
			}
			set
			{
				projectTemplateFilePathField = value;
			}
		}
	}
}
