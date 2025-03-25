using System;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal static class XmlSerializerFactory
	{
		private const string XmlSerializersAssembly = "Sdl.ProjectApi.Implementation.XmlSerialization";

		public static XmlSerializer CreateApplicationSerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.ApplicationSerialization.ApplicationSerializer");
		}

		public static XmlSerializer CreateProjectServerSerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.ProjectServerSerialization.ProjectServerSerializer");
		}

		public static XmlSerializer CreateProjectTemplateSerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.ProjectTemplateSerialization.ProjectTemplateSerializer");
		}

		public static XmlSerializer CreateProjectSerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.ProjectSerialization.ProjectSerializer");
		}

		public static XmlSerializer CreatePackageProjectSerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.PackageProjectSerialization.PackageProjectSerializer");
		}

		public static XmlSerializer CreateTranslationMemorySerializer()
		{
			return CreateSerializer("Sdl.ProjectApi.Implementation.XmlSerialization.TranslationMemorySerialization.TranslationMemorySerializer");
		}

		private static XmlSerializer CreateSerializer(string className)
		{
			Type type = Type.GetType(string.Format("{0}, {1}", className, "Sdl.ProjectApi.Implementation.XmlSerialization"), throwOnError: true);
			return (XmlSerializer)Activator.CreateInstance(type);
		}
	}
}
