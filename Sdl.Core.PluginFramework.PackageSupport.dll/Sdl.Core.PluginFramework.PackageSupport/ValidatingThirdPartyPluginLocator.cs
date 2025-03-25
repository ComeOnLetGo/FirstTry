using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Sdl.Core.PluginFramework.Configuration;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class ValidatingThirdPartyPluginLocator : IValidatingPluginLocator, IPluginLocator, IDisposable
	{
		private const string OPEN_EXCHANGE_CERT_PATH = "Sdl.Core.PluginFramework.PackageSupport.OpenXCert.cer";

		private const string PLUGIN_PACKAGE_EXTENSION = ".sdlplugin";

		private const string SECURE_MODE_CERT_PATH = "Sdl.Core.PluginFramework.PackageSupport.SecureModeCert.cer";

		private readonly IPluginLocator _baseLocator;

		private readonly bool _isSecureMode;

		private readonly X509Certificate _openExchangePublishingCertificate;

		private readonly X509Certificate _secureModeCertificate;

		public List<IPluginDescriptor> InvalidDescriptors { get; private set; }

		public List<IPluginDescriptor> ValidatedDescriptors { get; private set; }

		public ValidatingThirdPartyPluginLocator(IPluginLocator baseLocator, IFrameworkConfiguration configuration)
			: this(baseLocator, configuration, isSecureMode: false)
		{
		}

		public ValidatingThirdPartyPluginLocator(IPluginLocator baseLocator, IFrameworkConfiguration configuration, bool isSecureMode)
		{
			_baseLocator = baseLocator ?? throw new ArgumentNullException("baseLocator");
			_isSecureMode = isSecureMode;
			_openExchangePublishingCertificate = LoadCertificate("Sdl.Core.PluginFramework.PackageSupport.OpenXCert.cer");
			_secureModeCertificate = LoadCertificate("Sdl.Core.PluginFramework.PackageSupport.SecureModeCert.cer");
			ValidateThirdPartyPluginDescriptors(configuration);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public IPluginDescriptor[] GetPluginDescriptors()
		{
			if (ValidatedDescriptors == null)
			{
				throw new InvalidOperationException("_validatedDescriptors is null, must call ValidateThirdPartyPluginDescriptors()");
			}
			return ValidatedDescriptors.ToArray();
		}

		private X509Certificate LoadCertificate(string certificatePath)
		{
			X509Certificate x509Certificate = null;
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(certificatePath))
			{
				if (stream != null)
				{
					byte[] array = new byte[stream.Length];
					stream.Read(array, 0, (int)stream.Length);
					x509Certificate = new X509Certificate();
					x509Certificate.Import(array);
				}
			}
			return x509Certificate;
		}

		private bool ThirdPartyPlugInReferencesAllowed(IFrameworkConfiguration configuration, IThirdPartyPluginDescriptor thirdPartyFileDescriptor)
		{
			string directoryName = Path.GetDirectoryName(thirdPartyFileDescriptor.ThirdPartyManifestFilePath);
			bool result = true;
			string[] files = Directory.GetFiles(directoryName, "*.dll", SearchOption.AllDirectories);
			foreach (string assemblyFile in files)
			{
				try
				{
					Assembly loadedDll = Assembly.LoadFrom(assemblyFile);
					if (!ValidateDllReferences(configuration, loadedDll, thirdPartyFileDescriptor))
					{
						result = false;
					}
				}
				catch (Exception)
				{
				}
			}
			return result;
		}

		internal bool ValidateDllReferences(IFrameworkConfiguration configuration, _Assembly loadedDll, IThirdPartyPluginDescriptor descriptor)
		{
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			AssemblyName[] referencedAssemblies = loadedDll.GetReferencedAssemblies();
			bool result = true;
			AssemblyName[] array = referencedAssemblies;
			foreach (AssemblyName referenced in array)
			{
				if (referenced.FullName.IndexOf("PublicKeyToken=c28cdb26c445c888") == -1)
				{
					continue;
				}
				InvalidSdlAssemblyReference val = null;
				KeyValuePair<string, Version> keyValuePair = configuration.ApiVersions.FirstOrDefault((KeyValuePair<string, Version> v) => referenced.FullName.IndexOf(v.Key + ",") != -1);
				if (keyValuePair.Value == null)
				{
					if (loadedDll.FullName.IndexOf("PublicKeyToken=c28cdb26c445c888") == -1)
					{
						val = new InvalidSdlAssemblyReference(referenced, (SdlAssemblyReferenceError)0);
					}
				}
				else if (referenced.Version.Major > keyValuePair.Value.Major || (referenced.Version.Major == keyValuePair.Value.Major && referenced.Version.Minor > keyValuePair.Value.Minor))
				{
					val = new InvalidSdlAssemblyReference(referenced, (SdlAssemblyReferenceError)2);
				}
				else if (referenced.Version.Major < keyValuePair.Value.Major)
				{
					val = new InvalidSdlAssemblyReference(referenced, (SdlAssemblyReferenceError)1);
				}
				if (val != null)
				{
					descriptor.InvalidSdlAssemblyReferences.Add(val);
					result = false;
				}
			}
			return result;
		}

		private void ValidateThirdPartyPluginDescriptors(IFrameworkConfiguration configuration)
		{
			ValidatedDescriptors = new List<IPluginDescriptor>();
			InvalidDescriptors = new List<IPluginDescriptor>();
			IPluginDescriptor[] pluginDescriptors = _baseLocator.GetPluginDescriptors();
			foreach (IPluginDescriptor val in pluginDescriptors)
			{
				IThirdPartyPluginDescriptor val2 = (IThirdPartyPluginDescriptor)(object)((val is IThirdPartyPluginDescriptor) ? val : null);
				if (val2 != null)
				{
					string directoryName = Path.GetDirectoryName(val2.ThirdPartyManifestFilePath);
					string fileName = Path.GetFileName(directoryName);
					if (string.IsNullOrEmpty(configuration.ThirdPartyPluginPackagesRelativePath))
					{
						continue;
					}
					int length = directoryName.IndexOf(configuration.ThirdPartyPluginsRelativePath);
					string text = Path.Combine(Path.Combine(directoryName.Substring(0, length), configuration.ThirdPartyPluginPackagesRelativePath), fileName + ".sdlplugin");
					if (!File.Exists(text) || !Directory.Exists(directoryName) || _openExchangePublishingCertificate == null || _secureModeCertificate == null)
					{
						continue;
					}
					bool flag = false;
					try
					{
						using PluginPackage pluginPackage = new PluginPackage(text, FileAccess.Read);
						if (((!_isSecureMode && pluginPackage.ValidateSignatures(_openExchangePublishingCertificate)) || pluginPackage.ValidateSignatures(_secureModeCertificate)) && pluginPackage.ComparePackageContentsTo(directoryName))
						{
							flag = true;
						}
					}
					catch (Exception)
					{
						throw;
					}
					if (ThirdPartyPlugInReferencesAllowed(configuration, val2))
					{
						if (flag)
						{
							ValidatedDescriptors.Add((IPluginDescriptor)(object)val2);
						}
						else
						{
							InvalidDescriptors.Add(val);
						}
					}
					else
					{
						InvalidDescriptors.Add(val);
					}
				}
				else
				{
					ValidatedDescriptors.Add(val);
				}
			}
		}
	}
}
