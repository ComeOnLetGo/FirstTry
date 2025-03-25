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
	public class PackageOperations
	{
		private List<ProjectPackageCreation> projectPackageCreationOperationsField = new List<ProjectPackageCreation>();

		private List<ProjectPackageImport> projectPackageImportOperationsField = new List<ProjectPackageImport>();

		private List<ReturnPackageCreation> returnPackageCreationOperationsField = new List<ReturnPackageCreation>();

		private List<ReturnPackageImport> returnPackageImportOperationsField = new List<ReturnPackageImport>();

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<ProjectPackageCreation> ProjectPackageCreationOperations
		{
			get
			{
				return projectPackageCreationOperationsField;
			}
			set
			{
				projectPackageCreationOperationsField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<ProjectPackageImport> ProjectPackageImportOperations
		{
			get
			{
				return projectPackageImportOperationsField;
			}
			set
			{
				projectPackageImportOperationsField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<ReturnPackageCreation> ReturnPackageCreationOperations
		{
			get
			{
				return returnPackageCreationOperationsField;
			}
			set
			{
				returnPackageCreationOperationsField = value;
			}
		}

		[XmlArray(Order = 3)]
		[XmlArrayItem(IsNullable = false)]
		public List<ReturnPackageImport> ReturnPackageImportOperations
		{
			get
			{
				return returnPackageImportOperationsField;
			}
			set
			{
				returnPackageImportOperationsField = value;
			}
		}
	}
}
