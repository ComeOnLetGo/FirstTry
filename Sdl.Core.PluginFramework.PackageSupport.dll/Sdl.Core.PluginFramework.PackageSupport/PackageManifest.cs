using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class PackageManifest : IEquatable<object>
	{
		public const string MANIFEST_FILENAME = "pluginpackage.manifest.xml";

		private const string THIRDPARTY_MANIFEST_NAMESPACE = "http://www.sdl.com/Plugins/PluginPackage/1.0";

		private const string ROOT_ELEMENT = "PluginPackage";

		private const string AUTHOR_ELEMENT = "Author";

		private const string DESCRIPTION_ELEMENT = "Description";

		private const string PLUGINNAME_ELEMENT = "PlugInName";

		private const string REQUIREDPRODUCT_ELEMENT = "RequiredProduct";

		private const string MINVERSION_ATTRIBUTE = "minversion";

		private const string MAXVERSION_ATTRIBUTE = "maxversion";

		private const string VERSION_ELEMENT = "Version";

		private const string PRODUCTNAME_ATTRIBUTE = "name";

		private const string INCLUDE = "Include";

		private const string FILE = "File";

		public bool LoadedSucessfully { get; }

		public string Author { get; set; }

		public string Description { get; set; }

		public string PlugInName { get; set; }

		public Version Version { get; set; }

		public Version MinRequiredProductVersion { get; set; }

		public Version MaxRequiredProductVersion { get; set; }

		public string RequiredProductName { get; set; }

		public IEnumerable<string> AdditionalFiles { get; set; }

		public List<string> ErrorMessages { get; set; }

		public PackageManifest()
		{
			ErrorMessages = new List<string>();
		}

		public PackageManifest(string packageManifestFile)
			: this()
		{
			if (string.IsNullOrWhiteSpace(packageManifestFile))
			{
				throw new ArgumentNullException("packageManifestFile");
			}
			if (!File.Exists(packageManifestFile))
			{
				return;
			}
			using FileStream manifestStream = File.Open(packageManifestFile, FileMode.Open, FileAccess.Read);
			LoadedSucessfully = LoadXmlManifest(manifestStream);
		}

		public PackageManifest(Stream pluginManifestStream)
			: this()
		{
			if (pluginManifestStream == null)
			{
				throw new ArgumentNullException("pluginManifestStream");
			}
			using (pluginManifestStream)
			{
				LoadedSucessfully = LoadXmlManifest(pluginManifestStream);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			PackageManifest packageManifest = (PackageManifest)obj;
			if (Author.Equals(packageManifest.Author) && object.Equals(Description, packageManifest.Description) && object.Equals(MinRequiredProductVersion, packageManifest.MinRequiredProductVersion) && object.Equals(MaxRequiredProductVersion, packageManifest.MaxRequiredProductVersion) && object.Equals(PlugInName, packageManifest.PlugInName) && object.Equals(RequiredProductName, packageManifest.RequiredProductName) && object.Equals(Version, packageManifest.Version))
			{
				return object.Equals(AdditionalFiles, packageManifest.AdditionalFiles);
			}
			return false;
		}

		public override int GetHashCode()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Author);
			stringBuilder.Append(Description);
			stringBuilder.Append(MinRequiredProductVersion);
			stringBuilder.Append(PlugInName);
			stringBuilder.Append(RequiredProductName);
			stringBuilder.Append(Version);
			return stringBuilder.ToString().GetHashCode();
		}

		public void Save(Stream toStream)
		{
			using XmlWriter xmlWriter = XmlWriter.Create(toStream, new XmlWriterSettings
			{
				Indent = true,
				Encoding = Encoding.UTF8
			});
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("PluginPackage", "http://www.sdl.com/Plugins/PluginPackage/1.0");
			xmlWriter.WriteElementString("PlugInName", "http://www.sdl.com/Plugins/PluginPackage/1.0", PlugInName);
			xmlWriter.WriteElementString("Version", "http://www.sdl.com/Plugins/PluginPackage/1.0", Version.ToString());
			xmlWriter.WriteElementString("Description", "http://www.sdl.com/Plugins/PluginPackage/1.0", Description);
			xmlWriter.WriteElementString("Author", "http://www.sdl.com/Plugins/PluginPackage/1.0", Author);
			xmlWriter.WriteStartElement("RequiredProduct");
			xmlWriter.WriteAttributeString("name", RequiredProductName);
			if (MinRequiredProductVersion != null)
			{
				xmlWriter.WriteAttributeString("minversion", MinRequiredProductVersion.ToString());
			}
			if (MaxRequiredProductVersion != null)
			{
				xmlWriter.WriteAttributeString("maxversion", MaxRequiredProductVersion.ToString());
			}
			xmlWriter.WriteEndElement();
			if (AdditionalFiles != null && AdditionalFiles.Any())
			{
				xmlWriter.WriteStartElement("Include");
				foreach (string additionalFile in AdditionalFiles)
				{
					xmlWriter.WriteStartElement("File");
					xmlWriter.WriteString(additionalFile);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
		}

		private bool LoadXmlManifest(Stream manifestStream)
		{
			ErrorMessages.Clear();
			XDocument xDocument;
			try
			{
				using XmlReader reader = XmlReader.Create(manifestStream);
				xDocument = XDocument.Load(reader);
			}
			catch
			{
				return false;
			}
			XElement root = xDocument.Root;
			if (root?.Name.LocalName != "PluginPackage")
			{
				return false;
			}
			Author = GetRequiredElementContent(root, "Author");
			Description = GetRequiredElementContent(root, "Description");
			PlugInName = GetPluginName(root);
			Version = GetPluginVersion(root);
			XElement xElement = root.Element(XName.Get("RequiredProduct", "http://www.sdl.com/Plugins/PluginPackage/1.0"));
			if (xElement == null)
			{
				ErrorMessages.Add(PackageSupportResFile.RequiredProduct_Missing_ErrorMessage);
			}
			else
			{
				RequiredProductName = GetRequiredProductName(xElement);
				MinRequiredProductVersion = GetVersionAttribute(xElement, "minversion");
				if (GetAttribute(xElement, "maxversion") != null)
				{
					MaxRequiredProductVersion = GetVersionAttribute(xElement, "maxversion");
				}
			}
			XElement xElement2 = root.Element(XName.Get("Include", "http://www.sdl.com/Plugins/PluginPackage/1.0"));
			if (xElement2 != null)
			{
				AdditionalFiles = (from el in xElement2.Elements(XName.Get("File", "http://www.sdl.com/Plugins/PluginPackage/1.0"))
					where !string.IsNullOrWhiteSpace(el.Value)
					select el.Value.Trim()).ToList();
			}
			else
			{
				AdditionalFiles = new List<string>();
			}
			return !ErrorMessages.Any();
		}

		public bool IsValid(Dictionary<string, Version> productVersions)
		{
			if (!LoadedSucessfully)
			{
				return false;
			}
			if (RequiredProductName == null)
			{
				return false;
			}
			if (!productVersions.TryGetValue(RequiredProductName, out var value))
			{
				return false;
			}
			if (MinRequiredProductVersion == null || MinRequiredProductVersion.Major > value.Major || (MinRequiredProductVersion.Major == value.Major && MinRequiredProductVersion.Minor > value.Minor))
			{
				return false;
			}
			if (MaxRequiredProductVersion != null && (MaxRequiredProductVersion.Major < value.Major || (MaxRequiredProductVersion.Major == value.Major && MaxRequiredProductVersion.Minor < value.Minor)))
			{
				return false;
			}
			return true;
		}

		private string GetRequiredProductName(XElement requiredProductElement)
		{
			string attribute = GetAttribute(requiredProductElement, "name");
			if (attribute == null)
			{
				ErrorMessages.Add(PackageSupportResFile.RequiredProductValue_Missing_ErrorMessage);
			}
			return attribute;
		}

		private string GetPluginName(XElement rootElement)
		{
			string requiredElementContent = GetRequiredElementContent(rootElement, "PlugInName");
			if (requiredElementContent == null)
			{
				ErrorMessages.Add(PackageSupportResFile.PluginName_Missing_ErrorMessage);
			}
			return requiredElementContent;
		}

		private Version GetPluginVersion(XElement rootElement)
		{
			string requiredElementContent = GetRequiredElementContent(rootElement, "Version");
			if (requiredElementContent == null)
			{
				ErrorMessages.Add(PackageSupportResFile.Version_Missing_ErrorMessage);
				return new Version();
			}
			if (!Version.TryParse(requiredElementContent, out var result))
			{
				ErrorMessages.Add(string.Format(PackageSupportResFile.PluginVersion_Incorrect_ErrorMessage, requiredElementContent));
			}
			return result;
		}

		private Version GetVersionAttribute(XElement requiredProductElement, string attributeName)
		{
			string attribute = GetAttribute(requiredProductElement, attributeName);
			if (GetAttribute(requiredProductElement, attributeName) == null)
			{
				ErrorMessages.Add(string.Format(PackageSupportResFile.AttributeVersion_Missing_ErrorMessage, attributeName));
			}
			if (!Version.TryParse(attribute, out var result))
			{
				ErrorMessages.Add(string.Format(PackageSupportResFile.MinOrMaxVersion_Incorrect_ErrorMessage, attributeName, attribute));
			}
			return result;
		}

		private string GetAttribute(XElement rootElement, string attributeName)
		{
			XAttribute xAttribute = rootElement.Attribute(XName.Get(attributeName));
			if (!string.IsNullOrEmpty(xAttribute?.Value))
			{
				return xAttribute.Value;
			}
			return null;
		}

		private string GetRequiredElementContent(XElement rootElement, string attributeId)
		{
			XElement xElement = rootElement.Element(XName.Get(attributeId, "http://www.sdl.com/Plugins/PluginPackage/1.0"));
			if (xElement != null && !string.IsNullOrEmpty(xElement.Value))
			{
				return xElement.Value;
			}
			return null;
		}
	}
}
