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
	public class Application
	{
		private List<LocalProjectServerInfo> localProjectServersField = new List<LocalProjectServerInfo>();

		private string versionField;

		private static XmlSerializer _serializer;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<LocalProjectServerInfo> LocalProjectServers
		{
			get
			{
				return localProjectServersField;
			}
			set
			{
				localProjectServersField = value;
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
					_serializer = XmlSerializerFactory.CreateApplicationSerializer();
				}
				return _serializer;
			}
		}

		public static Application Deserialize(string path)
		{
			using Stream stream = File.OpenRead(path);
			return Deserialize(stream);
		}

		public static Application Deserialize(Stream stream)
		{
			return (Application)Serializer.Deserialize(stream);
		}

		public void Serialize(string path)
		{
			using Stream stream = File.Create(path);
			Version = "3.0.0.0";
			Serializer.Serialize(stream, this);
		}
	}
}
