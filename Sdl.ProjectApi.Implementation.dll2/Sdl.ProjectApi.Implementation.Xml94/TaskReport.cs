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
	public class TaskReport : GenericItem
	{
		private string nameField;

		private string descriptionField;

		private string taskTemplateIdField;

		private Guid languageDirectionGuidField = Guid.Empty;

		private string physicalPathField;

		private bool isCustomReportField;

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

		[XmlAttribute]
		public string Description
		{
			get
			{
				return descriptionField;
			}
			set
			{
				descriptionField = value;
			}
		}

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

		[XmlAttribute]
		public Guid LanguageDirectionGuid
		{
			get
			{
				return languageDirectionGuidField;
			}
			set
			{
				languageDirectionGuidField = value;
			}
		}

		[XmlAttribute]
		public string PhysicalPath
		{
			get
			{
				return physicalPathField;
			}
			set
			{
				physicalPathField = value;
			}
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool IsCustomReport
		{
			get
			{
				return isCustomReportField;
			}
			set
			{
				isCustomReportField = value;
			}
		}

		public TaskReport()
		{
			isCustomReportField = false;
		}
	}
}
