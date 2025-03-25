using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlRoot(Namespace = "", IsNullable = false)]
	public class Project : ProjectConfiguration
	{
		private GeneralProjectInfo generalProjectInfoField;

		private string sourceLanguageCodeField;

		private List<ProjectFile> projectFilesField = new List<ProjectFile>();

		private WordCountStatistics wordCountStatisticsField;

		private ProjectTasks tasksField;

		private PackageOperations packageOperationsField;

		private Guid projectTemplateGuidField = Guid.Empty;

		private Guid referenceProjectGuidField = Guid.Empty;

		private string versionField;

		private string ownerField;

		private static XmlSerializer _serializer;

		[XmlElement(Order = 0)]
		public GeneralProjectInfo GeneralProjectInfo
		{
			get
			{
				return generalProjectInfoField;
			}
			set
			{
				generalProjectInfoField = value;
			}
		}

		[XmlElement(Order = 1)]
		public string SourceLanguageCode
		{
			get
			{
				return sourceLanguageCodeField;
			}
			set
			{
				sourceLanguageCodeField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<ProjectFile> ProjectFiles
		{
			get
			{
				return projectFilesField;
			}
			set
			{
				projectFilesField = value;
			}
		}

		[XmlElement(Order = 3)]
		public WordCountStatistics WordCountStatistics
		{
			get
			{
				return wordCountStatisticsField;
			}
			set
			{
				wordCountStatisticsField = value;
			}
		}

		[XmlElement(Order = 4)]
		public ProjectTasks Tasks
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

		[XmlElement(Order = 5)]
		public PackageOperations PackageOperations
		{
			get
			{
				return packageOperationsField;
			}
			set
			{
				packageOperationsField = value;
			}
		}

		[XmlAttribute]
		public Guid ProjectTemplateGuid
		{
			get
			{
				return projectTemplateGuidField;
			}
			set
			{
				projectTemplateGuidField = value;
			}
		}

		[XmlAttribute]
		public Guid ReferenceProjectGuid
		{
			get
			{
				return referenceProjectGuidField;
			}
			set
			{
				referenceProjectGuidField = value;
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

		[XmlAttribute]
		public string Owner
		{
			get
			{
				return ownerField;
			}
			set
			{
				ownerField = value;
			}
		}

		public static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
				{
					_serializer = XmlSerializerFactory.CreateProjectSerializer();
				}
				return _serializer;
			}
		}

		public Project()
		{
			Tasks = new ProjectTasks();
		}

		public static Project Deserialize(string path)
		{
			using Stream stream = File.OpenRead(path);
			return (Project)Serializer.Deserialize(stream);
		}

		public static Project Deserialize(XmlDocument xmlDocument)
		{
			XPathNavigator xPathNavigator = xmlDocument.CreateNavigator();
			using XmlReader xmlReader = xPathNavigator.ReadSubtree();
			return (Project)Serializer.Deserialize(xmlReader);
		}

		public static Project Deserialize(Stream stream)
		{
			return (Project)Serializer.Deserialize(stream);
		}

		public static Project Deserialize(StringReader reader)
		{
			return (Project)Serializer.Deserialize(reader);
		}

		public virtual void Serialize(string path)
		{
			using Stream stream = File.Create(path);
			Version = "4.0.0.0";
			Serializer.Serialize(stream, this);
		}

		public virtual string Serialize()
		{
			using StringWriter stringWriter = new StringWriter();
			Version = "4.0.0.0";
			Serializer.Serialize(stringWriter, this);
			return stringWriter.ToString();
		}

		public virtual XmlDocument SerializeToXmlDocument()
		{
			XmlDocument xmlDocument = new XmlDocument();
			XPathNavigator xPathNavigator = xmlDocument.CreateNavigator();
			using XmlWriter xmlWriter = xPathNavigator.AppendChild();
			Version = "4.0.0.0";
			Serializer.Serialize(xmlWriter, this);
			return xmlDocument;
		}

		public virtual void Serialize(StreamWriter stream)
		{
			Version = "4.0.0.0";
			Serializer.Serialize(stream, this);
		}

		public bool GetLanguageFile(Guid languageFileId, out ProjectFile xmlProjectFile, out LanguageFile xmlLanguageFile)
		{
			foreach (ProjectFile projectFile in ProjectFiles)
			{
				foreach (LanguageFile languageFile in projectFile.LanguageFiles)
				{
					if (languageFile.Guid == languageFileId)
					{
						xmlProjectFile = projectFile;
						xmlLanguageFile = languageFile;
						return true;
					}
				}
			}
			xmlProjectFile = null;
			xmlLanguageFile = null;
			return false;
		}
	}
}
