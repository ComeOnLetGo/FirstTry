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
	public class ManualTaskTemplate : TaskTemplate
	{
		private List<ManualTaskTemplateTypes> supportedFileTypesField = new List<ManualTaskTemplateTypes>();

		private WorkflowStageType workflowStageTypeField;

		private TaskFileType generatedFileTypeField;

		private bool generatedFileTypeFieldSpecified;

		private bool isImportedField;

		private bool isLocallyExecutableField;

		[XmlArray(Order = 0)]
		[XmlArrayItem("Types", IsNullable = false)]
		public List<ManualTaskTemplateTypes> SupportedFileTypes
		{
			get
			{
				return supportedFileTypesField;
			}
			set
			{
				supportedFileTypesField = value;
			}
		}

		[XmlAttribute]
		public WorkflowStageType WorkflowStageType
		{
			get
			{
				return workflowStageTypeField;
			}
			set
			{
				workflowStageTypeField = value;
			}
		}

		[XmlAttribute]
		public TaskFileType GeneratedFileType
		{
			get
			{
				return generatedFileTypeField;
			}
			set
			{
				generatedFileTypeField = value;
			}
		}

		[XmlIgnore]
		public bool GeneratedFileTypeSpecified
		{
			get
			{
				return generatedFileTypeFieldSpecified;
			}
			set
			{
				generatedFileTypeFieldSpecified = value;
			}
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool IsImported
		{
			get
			{
				return isImportedField;
			}
			set
			{
				isImportedField = value;
			}
		}

		[XmlAttribute]
		[DefaultValue(true)]
		public bool IsLocallyExecutable
		{
			get
			{
				return isLocallyExecutableField;
			}
			set
			{
				isLocallyExecutableField = value;
			}
		}

		public ManualTaskTemplate()
		{
			isImportedField = false;
			isLocallyExecutableField = true;
		}

		public ManualTaskTemplate Copy()
		{
			return (ManualTaskTemplate)MemberwiseClone();
		}
	}
}
