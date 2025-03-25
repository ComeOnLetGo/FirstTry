using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class FileBasedThirdPartyPluginDescriptor : IThirdPartyPluginDescriptor, IPluginDescriptor
	{
		private ResourceManager _lazyResourceManager;

		private readonly PackageManifest _xmlPackageManifest;

		private readonly Dictionary<string, Version> _availableProductVersions;

		public string PluginManifestFilePath { get; }

		public string Name => Path.GetFileName(PluginManifestFilePath);

		private ResourceManager ResourceManager => _lazyResourceManager ?? (_lazyResourceManager = ResourceManager.CreateFileBasedResourceManager(PluginBasename, PluginDirectory, null));

		private string PluginDirectory => Path.GetDirectoryName(PluginManifestFilePath);

		private string PluginBasename => Path.GetFileNameWithoutExtension(PluginManifestFilePath);

		public string ThirdPartyManifestFilePath { get; }

		public string Author => _xmlPackageManifest.Author;

		public string Description => _xmlPackageManifest.Description;

		public string PlugInName => _xmlPackageManifest.PlugInName;

		public Version Version => _xmlPackageManifest.Version;

		public bool Validated => _xmlPackageManifest.IsValid(_availableProductVersions);

		public List<InvalidSdlAssemblyReference> InvalidSdlAssemblyReferences { get; }

		public FileBasedThirdPartyPluginDescriptor(string pluginPackageManifestFilePath, Dictionary<string, Version> availableProductVersions)
		{
			PluginManifestFilePath = pluginPackageManifestFilePath;
			string directoryName = Path.GetDirectoryName(pluginPackageManifestFilePath);
			ThirdPartyManifestFilePath = Path.Combine(directoryName, "pluginpackage.manifest.xml");
			_xmlPackageManifest = new PackageManifest(ThirdPartyManifestFilePath);
			InvalidSdlAssemblyReferences = new List<InvalidSdlAssemblyReference>();
			_availableProductVersions = availableProductVersions;
		}

		public Stream GetPluginManifestStream()
		{
			return File.OpenRead(PluginManifestFilePath);
		}

		public object GetPluginResource(string name)
		{
			return ResourceManager.GetObject(name);
		}
	}
}
