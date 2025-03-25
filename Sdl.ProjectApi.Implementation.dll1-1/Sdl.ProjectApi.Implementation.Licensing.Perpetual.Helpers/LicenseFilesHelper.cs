using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers
{
	[ExcludeFromCodeCoverage]
	internal class LicenseFilesHelper : ILicenseFilesHelper
	{
		private const string CommuterLicenseResourceName = "Sdl.ProjectApi.Implementation.Licensing.Perpetual.StudioCommuter.lic";

		private const string TrialLicenseResourceName = "Sdl.ProjectApi.Implementation.Licensing.Perpetual.Studio30DayTrial.lic";

		private static readonly object LicenseFileLock = new object();

		public string GetCommuterLicenseDefinition()
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sdl.ProjectApi.Implementation.Licensing.Perpetual.StudioCommuter.lic");
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}

		public void AddTrialLicense(string licenseFilePath)
		{
			if (File.Exists(licenseFilePath))
			{
				return;
			}
			lock (LicenseFileLock)
			{
				string directoryName = Path.GetDirectoryName(licenseFilePath);
				if (string.IsNullOrEmpty(directoryName))
				{
					return;
				}
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sdl.ProjectApi.Implementation.Licensing.Perpetual.Studio30DayTrial.lic");
				int num = (int)stream.Length;
				byte[] buffer = new byte[num];
				stream.Read(buffer, 0, num);
				using Stream stream2 = File.Create(licenseFilePath);
				stream2.Write(buffer, 0, num);
			}
		}

		public string GetTrialActivationId()
		{
			string text = ReadTrialLicense();
			string[] array = text.Split(new string[1] { "AID=" }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length > 1)
			{
				return array[1];
			}
			throw new TrialLicenseException("Failed to retrieve trial license information.");
		}

		private static string ReadTrialLicense()
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sdl.ProjectApi.Implementation.Licensing.Perpetual.Studio30DayTrial.lic");
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadLine();
		}
	}
}
