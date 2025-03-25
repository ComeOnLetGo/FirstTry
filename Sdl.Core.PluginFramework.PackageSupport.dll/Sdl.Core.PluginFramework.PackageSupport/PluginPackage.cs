using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class PluginPackage : IDisposable
	{
		private const string DigitalSignatureUri = "/package/services/digital-signature/_rels/origin.psdsor.rels";

		private Package _package;

		private PackagePart _manifestPackagePart;

		public PackageManifest PackageManifest { get; set; }

		public string FilePath { get; }

		public PluginPackage(string filePath, FileAccess fileAccess)
		{
			FilePath = filePath;
			if (File.Exists(FilePath))
			{
				OpenPackage(fileAccess);
			}
			else
			{
				CreatePackage();
			}
		}

		private void OpenPackage(FileAccess fileAccess)
		{
			_package = Package.Open(FilePath, FileMode.Open, fileAccess);
			_manifestPackagePart = GetManifestPackagePart(_package);
			using Stream pluginManifestStream = _manifestPackagePart.GetStream(FileMode.Open, FileAccess.Read);
			PackageManifest = new PackageManifest(pluginManifestStream);
		}

		private void CreatePackage()
		{
			Uri partUri = PackUriHelper.CreatePartUri(new Uri("pluginpackage.manifest.xml", UriKind.Relative));
			_package = Package.Open(FilePath, FileMode.Create);
			_manifestPackagePart = _package.CreatePart(partUri, "text/xml");
			_package.CreateRelationship(_manifestPackagePart.Uri, TargetMode.Internal, "http://www.sdl.com/OpenExchange/2010/03/sdlplugin/root-manifest-xml");
			PackageManifest = new PackageManifest();
		}

		public PluginPackagePart[] GetParts()
		{
			List<PluginPackagePart> list = new List<PluginPackagePart>();
			foreach (PackageRelationship item2 in _manifestPackagePart.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/required-resource"))
			{
				Uri partUri = PackUriHelper.ResolvePartUri(_manifestPackagePart.Uri, item2.TargetUri);
				PackagePart part = _package.GetPart(partUri);
				PluginPackagePart item = new PluginPackagePart(item2, part);
				list.Add(item);
			}
			return list.ToArray();
		}

		public PluginPackagePart[] AddFolder(string folder, string targetDirectory, bool recursive)
		{
			return AddFolder(folder, targetDirectory, recursive, addInSubFolder: true);
		}

		public PluginPackagePart[] AddFolderContents(string folder, string targetDirectory, bool recursive)
		{
			return AddFolder(folder, targetDirectory, recursive, addInSubFolder: false);
		}

		public PluginPackagePart[] AddFolder(string folder, string targetDirectory, bool recursive, bool addInSubFolder)
		{
			string text = (targetDirectory.EndsWith("\\") ? targetDirectory.Substring(0, targetDirectory.Length - 1) : targetDirectory);
			List<PluginPackagePart> list = new List<PluginPackagePart>();
			DirectoryInfo directoryInfo = (addInSubFolder ? Directory.GetParent(folder) : new DirectoryInfo(folder));
			string[] files = Directory.GetFiles(folder, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; i++)
			{
				string fullPath = Path.GetFullPath(files[i]);
				string text2 = Path.GetDirectoryName(fullPath).Substring(directoryInfo.FullName.Length);
				if (text2.StartsWith("\\"))
				{
					text2 = text2.Substring(1);
				}
				string targetDirectory2 = ((text != string.Empty) ? text : text2);
				list.Add(AddPart(fullPath, targetDirectory2));
			}
			return list.ToArray();
		}

		public PluginPackagePart AddPart(string filePath, string targetDirectory)
		{
			Uri uri = PackUriHelper.CreatePartUri(new Uri(Path.Combine(targetDirectory, Path.GetFileName(filePath)), UriKind.Relative));
			PackagePart packagePart = _package.CreatePart(uri, "application/octet-stream");
			using (FileStream source = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				StreamUtils.CopyStream(source, packagePart.GetStream());
			}
			return new PluginPackagePart(_manifestPackagePart.CreateRelationship(uri, TargetMode.Internal, "http://www.sdl.com/OpenExchange/2010/03/sdlplugin/required-resource"), packagePart);
		}

		public void RemovePart(PluginPackagePart part)
		{
			_manifestPackagePart.DeleteRelationship(part.Relationship.Id);
			_package.DeletePart(part.Part.Uri);
		}

		public string Extract(string targetDirectory)
		{
			Uri uri = null;
			if (targetDirectory[targetDirectory.Length - 1] != '\\')
			{
				targetDirectory += "\\";
			}
			PackagePart packagePart = null;
			foreach (PackageRelationship item in _package.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/root-manifest-xml"))
			{
				uri = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), item.TargetUri);
				packagePart = _package.GetPart(uri);
				ExtractPart(packagePart, targetDirectory);
			}
			if (packagePart == null)
			{
				throw new PluginPackageException("Could not find plug-in package manifest.");
			}
			foreach (PackageRelationship item2 in packagePart.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/required-resource"))
			{
				Uri partUri = PackUriHelper.ResolvePartUri(packagePart.Uri, item2.TargetUri);
				ExtractPart(_package.GetPart(partUri), targetDirectory);
			}
			return targetDirectory + uri.ToString().TrimStart('/');
		}

		public void Sign(string signingCertificateFileName, string password)
		{
			List<Uri> list = new List<Uri>();
			foreach (PackagePart part in _package.GetParts())
			{
				list.Add(part.Uri);
			}
			Uri item = PackUriHelper.CreatePartUri(new Uri("/package/services/digital-signature/_rels/origin.psdsor.rels", UriKind.Relative));
			list.Add(item);
			PackageDigitalSignatureManager packageDigitalSignatureManager = new PackageDigitalSignatureManager(_package);
			packageDigitalSignatureManager.CertificateOption = CertificateEmbeddingOption.InSignaturePart;
			X509Certificate2 certificate = new X509Certificate2(signingCertificateFileName, password);
			packageDigitalSignatureManager.Sign(list, certificate);
		}

		public bool ValidateSignatures(string verififyingCertificateFileName)
		{
			X509Certificate certificate = new X509Certificate(verififyingCertificateFileName);
			return ValidateSignatures(certificate);
		}

		public bool ValidateSignatures(X509Certificate certificate)
		{
			return true;
		}

		public bool IsSigned()
		{
			if (_package == null)
			{
				return false;
			}
			return new PackageDigitalSignatureManager(_package).IsSigned;
		}

		public void RemoveAllSignatures()
		{
			if (_package != null)
			{
				PackageDigitalSignatureManager packageDigitalSignatureManager = new PackageDigitalSignatureManager(_package);
				if (packageDigitalSignatureManager.IsSigned)
				{
					packageDigitalSignatureManager.RemoveAllSignatures();
				}
			}
		}

		private static List<Uri> ListPackagePartsForVerification(Package package)
		{
			PackagePart packagePart = null;
			List<Uri> list = new List<Uri>();
			foreach (PackageRelationship item2 in package.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/root-manifest-xml"))
			{
				Uri uri = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), item2.TargetUri);
				packagePart = package.GetPart(uri);
				list.Add(uri);
			}
			if (packagePart == null)
			{
				return null;
			}
			foreach (PackageRelationship item3 in packagePart.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/required-resource"))
			{
				Uri item = PackUriHelper.ResolvePartUri(packagePart.Uri, item3.TargetUri);
				list.Add(item);
			}
			return list;
		}

		public bool ComparePackageContentsTo(string targetDirectory)
		{
			Uri uri = null;
			if (targetDirectory[targetDirectory.Length - 1] != '\\')
			{
				targetDirectory += "\\";
			}
			try
			{
				PackagePart packagePart = null;
				foreach (PackageRelationship item in _package.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/root-manifest-xml"))
				{
					uri = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), item.TargetUri);
					packagePart = _package.GetPart(uri);
					if (!ComparePart(packagePart, targetDirectory))
					{
						return false;
					}
				}
				if (packagePart == null)
				{
					return false;
				}
				foreach (PackageRelationship item2 in packagePart.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/required-resource"))
				{
					Uri partUri = PackUriHelper.ResolvePartUri(packagePart.Uri, item2.TargetUri);
					if (!ComparePart(_package.GetPart(partUri), targetDirectory))
					{
						return false;
					}
				}
			}
			catch (Exception)
			{
			}
			return uri != null;
		}

		private static bool ComparePart(PackagePart packagePart, string targetDirectory)
		{
			string text = targetDirectory + packagePart.Uri.ToString().TrimStart('/').Replace('/', '\\');
			if (!File.Exists(text))
			{
				return false;
			}
			if (IsExcludedFile(text))
			{
				return true;
			}
			try
			{
				using FileStream target = new FileStream(text, FileMode.Open, FileAccess.Read);
				return StreamUtils.CompareStream(packagePart.GetStream(), target);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static bool IsExcludedFile(string filepath)
		{
			if (filepath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (filepath.EndsWith(".plugin.xml", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (filepath.EndsWith(".manifest.xml", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (filepath.EndsWith(".resource", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return true;
		}

		private static PackagePart GetManifestPackagePart(Package package)
		{
			using (IEnumerator<PackageRelationship> enumerator = package.GetRelationshipsByType("http://www.sdl.com/OpenExchange/2010/03/sdlplugin/root-manifest-xml").GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					PackageRelationship current = enumerator.Current;
					Uri partUri = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), current.TargetUri);
					return package.GetPart(partUri);
				}
			}
			return null;
		}

		private static void ExtractPart(PackagePart packagePart, string targetDirectory)
		{
			string text = Uri.UnescapeDataString(packagePart.Uri.ToString());
			string path = targetDirectory + text.TrimStart('/').Replace('/', '\\');
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using FileStream target = new FileStream(path, FileMode.Create);
			StreamUtils.CopyStream(packagePart.GetStream(), target);
		}

		public void Save()
		{
			SaveManifest();
		}

		public void Close()
		{
			_package.Close();
			_package = null;
		}

		private void SaveManifest()
		{
			using Stream stream = _manifestPackagePart.GetStream(FileMode.Open, FileAccess.Write);
			stream.Position = 0L;
			stream.SetLength(0L);
			PackageManifest.Save(stream);
		}

		public void Dispose()
		{
			if (_package != null)
			{
				Close();
			}
			GC.SuppressFinalize(this);
		}
	}
}
