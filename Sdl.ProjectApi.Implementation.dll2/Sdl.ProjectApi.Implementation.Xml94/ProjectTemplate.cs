using System;
using System.CodeDom.Compiler;
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
	public class ProjectTemplate : ProjectConfiguration
	{
		private GeneralProjectTemplateInfo generalProjectTemplateInfoField;

		private FilterConfiguration filterConfigurationField;

		private string versionField;

		private static XmlSerializer _serializer;

		[XmlElement(Order = 0)]
		public GeneralProjectTemplateInfo GeneralProjectTemplateInfo
		{
			get
			{
				return generalProjectTemplateInfoField;
			}
			set
			{
				generalProjectTemplateInfoField = value;
			}
		}

		[XmlElement(Order = 1)]
		public FilterConfiguration FilterConfiguration
		{
			get
			{
				return filterConfigurationField;
			}
			set
			{
				filterConfigurationField = value;
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
					_serializer = XmlSerializerFactory.CreateProjectTemplateSerializer();
				}
				return _serializer;
			}
		}

		public static ProjectTemplate Deserialize(string path)
		{
			using Stream stream = File.OpenRead(path);
			return (ProjectTemplate)Serializer.Deserialize(stream);
		}

		public void Serialize(string path)
		{
			using Stream stream = File.Create(path);
			Version = "4.0.0.0";
			Serializer.Serialize(stream, this);
		}

		public string Serialize()
		{
			using StringWriter stringWriter = new StringWriter();
			Version = "4.0.0.0";
			Serializer.Serialize(stringWriter, this);
			return stringWriter.ToString();
		}
	}
}
