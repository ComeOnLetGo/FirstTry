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
	public class PackageProject : Project
	{
		private List<TaskRef> packageTasksField = new List<TaskRef>();

		private PackageLicenseInfo packageLicenseInfoField;

		private Guid packageGuidField = Guid.Empty;

		private PackageType packageTypeField;

		private string commentField;

		private DateTime packageCreatedAtField;

		private string packageCreatedByField;

		private static XmlSerializer _serializer;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<TaskRef> PackageTasks
		{
			get
			{
				return packageTasksField;
			}
			set
			{
				packageTasksField = value;
			}
		}

		[XmlElement(Order = 1)]
		public PackageLicenseInfo PackageLicenseInfo
		{
			get
			{
				return packageLicenseInfoField;
			}
			set
			{
				packageLicenseInfoField = value;
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

		[XmlAttribute]
		public PackageType PackageType
		{
			get
			{
				return packageTypeField;
			}
			set
			{
				packageTypeField = value;
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
		public DateTime PackageCreatedAt
		{
			get
			{
				return packageCreatedAtField;
			}
			set
			{
				packageCreatedAtField = value;
			}
		}

		[XmlAttribute]
		public string PackageCreatedBy
		{
			get
			{
				return packageCreatedByField;
			}
			set
			{
				packageCreatedByField = value;
			}
		}

		public new static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
				{
					_serializer = XmlSerializerFactory.CreatePackageProjectSerializer();
				}
				return _serializer;
			}
		}

		public new static PackageProject Deserialize(string path)
		{
			using Stream stream = File.OpenRead(path);
			return (PackageProject)Serializer.Deserialize(stream);
		}

		public override void Serialize(string path)
		{
			using Stream stream = File.Create(path);
			base.Version = "4.0.0.0";
			Serializer.Serialize(stream, this);
		}

		public override string Serialize()
		{
			using StringWriter stringWriter = new StringWriter();
			base.Version = "4.0.0.0";
			Serializer.Serialize(stringWriter, this);
			return stringWriter.ToString();
		}
	}
}
