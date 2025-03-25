using System;
using System.Collections.Generic;
using System.IO;
using Sdl.Core.PluginFramework.Configuration;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public sealed class ThirdPartyPluginLocator : IPluginLocator, IDisposable
	{
		private const string PLUGIN_EXTENSION = "plugin.xml";

		private const string PLUGIN_PACKAGE_EXTENSION = ".sdlplugin";

		private readonly IFrameworkConfiguration _configuration;

		private readonly List<string> _thirdPartyPluginsDirectories;

		private readonly List<string> _thirdPartyPluginsPackagesDirectories;

		public ThirdPartyPluginLocator(IFrameworkConfiguration configuration)
		{
			_configuration = configuration;
			_thirdPartyPluginsDirectories = new List<string>();
			_thirdPartyPluginsPackagesDirectories = new List<string>();
			GetThirdPartyPluginsDirectories(_configuration, _thirdPartyPluginsDirectories, _thirdPartyPluginsPackagesDirectories);
			CreateThirdPartyPluginsDirectories(_thirdPartyPluginsDirectories, _thirdPartyPluginsPackagesDirectories);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public IPluginDescriptor[] GetPluginDescriptors()
		{
			if (_thirdPartyPluginsDirectories.Count == 0)
			{
				return null;
			}
			SyncPlugInPackages();
			List<IPluginDescriptor> list = new List<IPluginDescriptor>();
			foreach (string thirdPartyPluginsDirectory in _thirdPartyPluginsDirectories)
			{
				list.AddRange(GetThirdPartyPluginDescriptors(thirdPartyPluginsDirectory));
			}
			return list.ToArray();
		}

		private void CreateThirdPartyPluginsDirectories(List<string> plugins, List<string> packages)
		{
			foreach (string plugin in plugins)
			{
				try
				{
					if (!Directory.Exists(plugin))
					{
						Directory.CreateDirectory(plugin);
					}
				}
				catch
				{
				}
			}
			foreach (string package in packages)
			{
				try
				{
					if (!Directory.Exists(package))
					{
						Directory.CreateDirectory(package);
					}
				}
				catch
				{
				}
			}
		}

		private List<IPluginDescriptor> GetThirdPartyPluginDescriptors(string thirdPartyPluginsDirectory)
		{
			List<IPluginDescriptor> list = new List<IPluginDescriptor>();
			string[] directories = Directory.GetDirectories(thirdPartyPluginsDirectory);
			for (int i = 0; i < directories.Length; i++)
			{
				string[] files = Directory.GetFiles(directories[i], "*.plugin.xml");
				for (int j = 0; j < files.Length; j++)
				{
					FileBasedThirdPartyPluginDescriptor fileBasedThirdPartyPluginDescriptor = new FileBasedThirdPartyPluginDescriptor(files[j], _configuration.ProductVersions);
					if (fileBasedThirdPartyPluginDescriptor.Validated)
					{
						list.Add((IPluginDescriptor)(object)fileBasedThirdPartyPluginDescriptor);
					}
				}
			}
			return list;
		}

		private void GetThirdPartyPluginsDirectories(IFrameworkConfiguration configuration, List<string> plugins, List<string> packages)
		{
			if (!configuration.ThirdPartyPluginsEnabled || string.IsNullOrEmpty(configuration.ThirdPartyPluginsRelativePath) || string.IsNullOrEmpty(configuration.ThirdPartyPluginPackagesRelativePath))
			{
				return;
			}
			foreach (string item in new List<string>
			{
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			})
			{
				plugins.Add(Path.Combine(item, configuration.ThirdPartyPluginsRelativePath));
				packages.Add(Path.Combine(item, configuration.ThirdPartyPluginPackagesRelativePath));
			}
		}

		private void SyncPlugInPackages()
		{
			for (int i = 0; i < _thirdPartyPluginsDirectories.Count; i++)
			{
				SyncPlugInPackages(_thirdPartyPluginsDirectories[i], _thirdPartyPluginsPackagesDirectories[i]);
			}
		}

		private void SyncPlugInPackages(string thirdPartyPluginsDirectory, string thirdPartyPluginsPackagesDirectory)
		{
			string[] files = Directory.GetFiles(thirdPartyPluginsPackagesDirectory, "*.sdlplugin");
			Dictionary<string, Version> dictionary = new Dictionary<string, Version>();
			string[] array = files;
			foreach (string text in array)
			{
				using PluginPackage pluginPackage = new PluginPackage(text, FileAccess.Read);
				dictionary.Add(Path.GetFileNameWithoutExtension(text), pluginPackage.PackageManifest.IsValid(_configuration.ProductVersions) ? pluginPackage.PackageManifest.Version : null);
			}
			array = Directory.GetDirectories(thirdPartyPluginsDirectory);
			foreach (string text2 in array)
			{
				string fileName = Path.GetFileName(text2);
				PackageManifest packageManifest = new PackageManifest(Path.Combine(text2, "pluginpackage.manifest.xml"));
				Version value = null;
				bool flag = dictionary.TryGetValue(fileName, out value);
				if (flag && value != null)
				{
					if (packageManifest.LoadedSucessfully && (!packageManifest.IsValid(_configuration.ProductVersions) || packageManifest.Version < value))
					{
						using PluginPackage pluginPackage2 = new PluginPackage(Path.Combine(thirdPartyPluginsPackagesDirectory, fileName + ".sdlplugin"), FileAccess.Read);
						pluginPackage2.Extract(Path.Combine(thirdPartyPluginsDirectory, fileName));
					}
					dictionary.Remove(fileName);
				}
				else if (!flag)
				{
					Directory.Delete(text2, recursive: true);
				}
			}
			foreach (KeyValuePair<string, Version> item in dictionary)
			{
				if (item.Value != null)
				{
					using PluginPackage pluginPackage3 = new PluginPackage(Path.Combine(thirdPartyPluginsPackagesDirectory, item.Key + ".sdlplugin"), FileAccess.Read);
					pluginPackage3.Extract(Path.Combine(thirdPartyPluginsDirectory, item.Key));
				}
			}
		}
	}
}
