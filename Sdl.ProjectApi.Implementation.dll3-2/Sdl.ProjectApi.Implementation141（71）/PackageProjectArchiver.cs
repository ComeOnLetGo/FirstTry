using System;
using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	public class PackageProjectArchiver : IPackageProjectArchiver
	{
		private readonly IZipCompress _zipCompress;

		public PackageProjectArchiver(IZipCompress zipCompress)
		{
			_zipCompress = zipCompress;
		}

		public string ExtractPackage(string packageFilePath)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			try
			{
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				UnZip(packageFilePath, text);
			}
			catch (Exception ex)
			{
				throw new ProjectApiException("An error occurred extracting the package. " + ex.Message, ex);
			}
			string[] files = Directory.GetFiles(text, "*" + FileTypes.ProjectFileExtension, SearchOption.TopDirectoryOnly);
			if (files.Length != 1)
			{
				throw new ProjectApiException(ErrorMessages.PackageProject_ProjectFileNotFound);
			}
			return files[0];
		}

		public void ZipDirectory(string packageFilepath, string localDataFolder)
		{
			_zipCompress.ZipDirectory(packageFilepath, localDataFolder);
		}

		public void UnZip(string zipfile, string targetDir)
		{
			_zipCompress.UnZip(zipfile, targetDir);
		}
	}
}
