using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(PackageImport))]
	[XmlInclude(typeof(ReturnPackageImport))]
	[XmlInclude(typeof(ProjectPackageImport))]
	[XmlInclude(typeof(PackageCreation))]
	[XmlInclude(typeof(ReturnPackageCreation))]
	[XmlInclude(typeof(ProjectPackageCreation))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class PackageOperation
	{
		private List<TaskRef> tasksField = new List<TaskRef>();

		private List<TaskFileRef> filesField = new List<TaskFileRef>();

		private ExecutionResult resultField;

		private Guid guidField = Guid.Empty;

		private PackageStatus statusField;

		private int percentCompleteField;

		private string packageNameField;

		private string commentField;

		private Guid packageGuidField = Guid.Empty;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<TaskRef> Tasks
		{
			get
			{
				return tasksField;
			}
			set
			{
				tasksField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<TaskFileRef> Files
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

		[XmlElement(Order = 2)]
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
		public Guid Guid
		{
			get
			{
				return guidField;
			}
			set
			{
				guidField = value;
			}
		}

		[XmlAttribute]
		public PackageStatus Status
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
		public string PackageName
		{
			get
			{
				return packageNameField;
			}
			set
			{
				packageNameField = value;
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
		public Guid PackageGuid
		{
			get
			{
				return packageGuidField;
			}
			set
			{
				packageGuidField = value;
			}
		}
	}
}
