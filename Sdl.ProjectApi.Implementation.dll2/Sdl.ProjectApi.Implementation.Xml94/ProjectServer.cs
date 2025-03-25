using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(Namespace = "", IsNullable = false)]
	public class ProjectServer
	{
		private Workflow workflowField;

		private List<ProjectListItem> projectsField = new List<ProjectListItem>();

		private List<ProjectTemplateListItem> projectTemplatesField = new List<ProjectTemplateListItem>();

		private List<User> usersField = new List<User>();

		private List<Customer> customersField = new List<Customer>();

		private List<ManualTaskIndexEntry> activeManualTaskIndexField = new List<ManualTaskIndexEntry>();

		private string versionField;

		private static XmlSerializer _serializer;

		[XmlElement(Order = 0)]
		public Workflow Workflow
		{
			get
			{
				return workflowField;
			}
			set
			{
				workflowField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<ProjectListItem> Projects
		{
			get
			{
				return projectsField;
			}
			set
			{
				projectsField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<ProjectTemplateListItem> ProjectTemplates
		{
			get
			{
				return projectTemplatesField;
			}
			set
			{
				projectTemplatesField = value;
			}
		}

		[XmlArray(Order = 3)]
		[XmlArrayItem(IsNullable = false)]
		public List<User> Users
		{
			get
			{
				return usersField;
			}
			set
			{
				usersField = value;
			}
		}

		[XmlArray(Order = 4)]
		[XmlArrayItem(IsNullable = false)]
		public List<Customer> Customers
		{
			get
			{
				return customersField;
			}
			set
			{
				customersField = value;
			}
		}

		[XmlArray(Order = 5)]
		[XmlArrayItem(IsNullable = false)]
		public List<ManualTaskIndexEntry> ActiveManualTaskIndex
		{
			get
			{
				return activeManualTaskIndexField;
			}
			set
			{
				activeManualTaskIndexField = value;
			}
		}

		[XmlAttribute]
		public string Version
		{
			get
			{
				return versionField;
			}
			set
			{
				versionField = value;
			}
		}

		public static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
				{
					_serializer = XmlSerializerFactory.CreateProjectServerSerializer();
				}
				return _serializer;
			}
		}

		public static ProjectServer Deserialize(string path)
		{
			using Stream stream = File.OpenRead(path);
			return Deserialize(stream);
		}

		public static ProjectServer Deserialize(Stream stream)
		{
			return (ProjectServer)Serializer.Deserialize(stream);
		}

		public void Serialize(string path)
		{
			using Stream stream = File.Create(path);
			Version = "3.2.0.0";
			Serializer.Serialize(stream, this);
		}
	}
}
