using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;
using Sdl.StudioServer.ProjectServer.Package;

namespace Sdl.ProjectApi.Implementation.Server
{
	internal static class PackageTransforms
	{
		private static readonly ILogger Log = LogProvider.GetLoggerFactory().CreateLogger("PackageTransforms");

		private static readonly object _styleSheetLock = new object();

		private static XslCompiledTransform ProjectToPackageTransform;

		private static XslCompiledTransform _packageToProjectTransform;

		private static XslCompiledTransform PackageToProjectTransform
		{
			get
			{
				lock (_styleSheetLock)
				{
					if (_packageToProjectTransform == null)
					{
						_packageToProjectTransform = new XslCompiledTransform();
						_packageToProjectTransform.Load(LoadPackageToProjectStylesheet());
					}
					return _packageToProjectTransform;
				}
			}
		}

		public static void TransformPackageToProject(string projectXmlFile, Uri serverUri, string serverUserName, UserManagerTokenType serverUserType)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(projectXmlFile);
			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("serverUri", string.Empty, serverUri.ToString());
			xsltArgumentList.AddParam("serverUserName", string.Empty, serverUserName);
			xsltArgumentList.AddParam("serverUserType", string.Empty, ((object)(UserManagerTokenType)(ref serverUserType)).ToString());
			xsltArgumentList.AddExtensionObject("http://www.sdl.com/ProjectApiExtensions", new ProjectApiExtensions());
			using XmlWriter results = XmlWriter.Create(projectXmlFile);
			PackageToProjectTransform.Transform(xmlDocument, xsltArgumentList, results);
		}

		public static MemoryStream TransformPackageToProject(Stream packageManifestStream, Uri serverUri, string serverUserName, UserManagerTokenType serverUserType)
		{
			if (packageManifestStream == null)
			{
				throw new ArgumentNullException("packageManifestStream");
			}
			try
			{
				XPathDocument input = new XPathDocument(ProjectPackage.CreateValidatingManifestReader(packageManifestStream, (ServerVersion)2));
				XsltArgumentList xsltArgumentList = new XsltArgumentList();
				xsltArgumentList.AddParam("serverUri", string.Empty, serverUri.ToString());
				xsltArgumentList.AddParam("serverUserName", string.Empty, serverUserName);
				xsltArgumentList.AddParam("serverUserType", string.Empty, ((object)(UserManagerTokenType)(ref serverUserType)).ToString());
				xsltArgumentList.AddExtensionObject("http://www.sdl.com/ProjectApiExtensions", new ProjectApiExtensions());
				MemoryStream memoryStream = new MemoryStream();
				using (XmlWriter results = XmlWriter.Create(memoryStream))
				{
					PackageToProjectTransform.Transform(input, xsltArgumentList, results);
				}
				memoryStream.Seek(0L, SeekOrigin.Begin);
				return memoryStream;
			}
			catch (XmlSchemaValidationException ex)
			{
				LoggerExtensions.LogError(Log, (Exception)ex, ex.Message, Array.Empty<object>());
				throw;
			}
		}

		public static string TransformProjectToPackage(string projectXmlFile, Guid packageGuid, bool metaDataOnly)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(projectXmlFile);
			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("packageGuid", string.Empty, Guid.NewGuid().ToString());
			xsltArgumentList.AddParam("metaDataOnly", string.Empty, metaDataOnly.ToString().ToLower());
			xsltArgumentList.AddExtensionObject("http://www.sdl.com/ProjectApiExtensions", new ProjectApiExtensions());
			MemoryStream memoryStream = new MemoryStream();
			XslCompiledTransform projectToPackageTransform = GetProjectToPackageTransform();
			using (XmlWriter results = XmlWriter.Create(memoryStream, projectToPackageTransform.OutputSettings))
			{
				projectToPackageTransform.Transform(xmlDocument, xsltArgumentList, results);
			}
			return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}

		private static XPathDocument LoadPackageToProjectStylesheet()
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sdl.ProjectApi.Implementation.Server.PackageToProject.xslt");
			return new XPathDocument(stream);
		}

		private static XslCompiledTransform GetProjectToPackageTransform()
		{
			if (ProjectToPackageTransform != null)
			{
				return ProjectToPackageTransform;
			}
			lock (_styleSheetLock)
			{
				if (ProjectToPackageTransform == null)
				{
					ProjectToPackageTransform = new XslCompiledTransform();
					ProjectToPackageTransform.Load(LoadProjectToPackageStylesheet());
					return ProjectToPackageTransform;
				}
				return ProjectToPackageTransform;
			}
		}

		private static XPathDocument LoadProjectToPackageStylesheet()
		{
			string name = "Sdl.ProjectApi.Implementation.Server.ProjectToPackage.xslt";
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			return new XPathDocument(stream);
		}
	}
}
