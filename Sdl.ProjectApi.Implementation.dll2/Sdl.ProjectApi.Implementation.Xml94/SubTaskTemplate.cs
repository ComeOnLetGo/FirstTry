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
	public class SubTaskTemplate
	{
		private string taskTemplateIdField;

		[XmlAttribute]
		public string TaskTemplateId
		{
			get
			{
				return taskTemplateIdField;
			}
			set
			{
				taskTemplateIdField = value;
			}
		}

		public SubTaskTemplate()
		{
		}

		public SubTaskTemplate(string taskTemplateId)
		{
			taskTemplateIdField = taskTemplateId;
		}

		public SubTaskTemplate Copy()
		{
			return (SubTaskTemplate)MemberwiseClone();
		}
	}
}
