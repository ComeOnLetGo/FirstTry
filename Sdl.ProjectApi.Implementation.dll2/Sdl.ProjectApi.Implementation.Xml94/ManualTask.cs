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
	public class ManualTask : Task
	{
		private string assignedByField;

		private string assignedToField;

		private DateTime assignedAtField;

		private bool assignedAtFieldSpecified;

		private DateTime dueDateField;

		private bool dueDateFieldSpecified;

		[XmlAttribute]
		public string AssignedBy
		{
			get
			{
				return assignedByField;
			}
			set
			{
				assignedByField = value;
			}
		}

		[XmlAttribute]
		public string AssignedTo
		{
			get
			{
				return assignedToField;
			}
			set
			{
				assignedToField = value;
			}
		}

		[XmlAttribute]
		public DateTime AssignedAt
		{
			get
			{
				return assignedAtField;
			}
			set
			{
				assignedAtField = value;
			}
		}

		[XmlIgnore]
		public bool AssignedAtSpecified
		{
			get
			{
				return assignedAtFieldSpecified;
			}
			set
			{
				assignedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public DateTime DueDate
		{
			get
			{
				return dueDateField;
			}
			set
			{
				dueDateField = value;
			}
		}

		[XmlIgnore]
		public bool DueDateSpecified
		{
			get
			{
				return dueDateFieldSpecified;
			}
			set
			{
				dueDateFieldSpecified = value;
			}
		}
	}
}
