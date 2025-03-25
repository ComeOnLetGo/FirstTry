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
	public class ManualTaskIndexEntry
	{
		private Guid taskGuidField = Guid.Empty;

		private Guid projectGuidField = Guid.Empty;

		private DateTime taskDueDateField;

		private bool taskDueDateFieldSpecified;

		[XmlAttribute]
		public Guid TaskGuid
		{
			get
			{
				return taskGuidField;
			}
			set
			{
				taskGuidField = value;
			}
		}

		[XmlAttribute]
		public Guid ProjectGuid
		{
			get
			{
				return projectGuidField;
			}
			set
			{
				projectGuidField = value;
			}
		}

		[XmlAttribute]
		public DateTime TaskDueDate
		{
			get
			{
				return taskDueDateField;
			}
			set
			{
				taskDueDateField = value;
			}
		}

		[XmlIgnore]
		public bool TaskDueDateSpecified
		{
			get
			{
				return taskDueDateFieldSpecified;
			}
			set
			{
				taskDueDateFieldSpecified = value;
			}
		}
	}
}
