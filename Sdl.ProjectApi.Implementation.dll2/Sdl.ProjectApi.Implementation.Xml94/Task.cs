using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(AutomaticTask))]
	[XmlInclude(typeof(ManualTask))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class Task : GenericItem
	{
		private List<string> taskTemplateIdsField = new List<string>();

		private List<TaskFile> filesField = new List<TaskFile>();

		private List<OutputFile> outputFilesField = new List<OutputFile>();

		private List<TaskReport> reportsField = new List<TaskReport>();

		private List<TaskParameter> parametersField = new List<TaskParameter>();

		private ExecutionResult resultField;

		private Guid predecessorTaskGuidField = Guid.Empty;

		private DateTime createdAtField;

		private string createdByField;

		private DateTime startedAtField;

		private bool startedAtFieldSpecified;

		private DateTime completedAtField;

		private bool completedAtFieldSpecified;

		private TaskStatus statusField;

		private int percentCompleteField;

		private string complexTaskTemplateIdField;

		private string commentField;

		private string nameField;

		private string descriptionField;

		[XmlArray(Order = 0)]
		[XmlArrayItem("TaskTemplateId", IsNullable = false)]
		public List<string> TaskTemplateIds
		{
			get
			{
				return taskTemplateIdsField;
			}
			set
			{
				taskTemplateIdsField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<TaskFile> Files
		{
			get
			{
				return filesField;
			}
			set
			{
				filesField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<OutputFile> OutputFiles
		{
			get
			{
				return outputFilesField;
			}
			set
			{
				outputFilesField = value;
			}
		}

		[XmlArray(Order = 3)]
		[XmlArrayItem("Report", IsNullable = false)]
		public List<TaskReport> Reports
		{
			get
			{
				return reportsField;
			}
			set
			{
				reportsField = value;
			}
		}

		[XmlArray(Order = 4)]
		[XmlArrayItem(IsNullable = false)]
		public List<TaskParameter> Parameters
		{
			get
			{
				return parametersField;
			}
			set
			{
				parametersField = value;
			}
		}

		[XmlElement(Order = 5)]
		public ExecutionResult Result
		{
			get
			{
				return resultField;
			}
			set
			{
				resultField = value;
			}
		}

		[XmlAttribute]
		public Guid PredecessorTaskGuid
		{
			get
			{
				return predecessorTaskGuidField;
			}
			set
			{
				predecessorTaskGuidField = value;
			}
		}

		[XmlAttribute]
		public DateTime CreatedAt
		{
			get
			{
				return createdAtField;
			}
			set
			{
				createdAtField = value;
			}
		}

		[XmlAttribute]
		public string CreatedBy
		{
			get
			{
				return createdByField;
			}
			set
			{
				createdByField = value;
			}
		}

		[XmlAttribute]
		public DateTime StartedAt
		{
			get
			{
				return startedAtField;
			}
			set
			{
				startedAtField = value;
			}
		}

		[XmlIgnore]
		public bool StartedAtSpecified
		{
			get
			{
				return startedAtFieldSpecified;
			}
			set
			{
				startedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public DateTime CompletedAt
		{
			get
			{
				return completedAtField;
			}
			set
			{
				completedAtField = value;
			}
		}

		[XmlIgnore]
		public bool CompletedAtSpecified
		{
			get
			{
				return completedAtFieldSpecified;
			}
			set
			{
				completedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public TaskStatus Status
		{
			get
			{
				return statusField;
			}
			set
			{
				statusField = value;
			}
		}

		[XmlAttribute]
		public int PercentComplete
		{
			get
			{
				return percentCompleteField;
			}
			set
			{
				percentCompleteField = value;
			}
		}

		[XmlAttribute]
		public string ComplexTaskTemplateId
		{
			get
			{
				return complexTaskTemplateIdField;
			}
			set
			{
				complexTaskTemplateIdField = value;
			}
		}

		[XmlAttribute]
		public string Comment
		{
			get
			{
				return commentField;
			}
			set
			{
				commentField = value;
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
	}
}
