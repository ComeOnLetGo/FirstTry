using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal abstract class AbstractFileMigration
	{
		protected IServerEvents _serverEvents;

		protected string ProjectFilePath { get; private set; }

		protected AbstractFileMigration()
			: this(null)
		{
		}

		protected AbstractFileMigration(IServerEvents serverEvents)
		{
			_serverEvents = serverEvents;
		}

		public bool Migrate(string filePath)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			ProjectFilePath = filePath;
			Version fileVersion = GetFileVersion(filePath);
			Version currentFileVersion = GetCurrentFileVersion();
			if (fileVersion == currentFileVersion)
			{
				return true;
			}
			if (CanMigrate(filePath, fileVersion, currentFileVersion))
			{
				Migrate(filePath, fileVersion);
				if (_serverEvents != null)
				{
					string migrationMessage = GetMigrationMessage(filePath);
					if (migrationMessage != null)
					{
						_serverEvents.ShowMigrationMessage(migrationMessage);
					}
				}
			}
			if (fileVersion > currentFileVersion)
			{
				throw new InvalidVersionException(fileVersion.ToString(), GetInvalidVersionExceptionMessage(fileVersion, filePath));
			}
			return true;
		}

		protected virtual bool CanMigrate(string filePath, Version projectFileVersion, Version currentFileVersion)
		{
			return projectFileVersion < currentFileVersion;
		}

		private void Migrate(string filePath, Version fileVersion)
		{
			string backupFilePath = GetBackupFilePath(filePath);
			File.Copy(filePath, backupFilePath, overwrite: true);
			XDocument xDocument = XDocument.Load(filePath);
			foreach (IMigration documentMigration in GetDocumentMigrations(fileVersion))
			{
				documentMigration.Migrate(xDocument, fileVersion);
			}
			VersionMigration versionMigration = new VersionMigration(GetCurrentFileVersion());
			versionMigration.Migrate(xDocument, fileVersion);
			xDocument.Save(filePath);
		}

		private string GetBackupFilePath(string filePath)
		{
			Regex regex = new Regex(Regex.Escape(Path.GetFileName(filePath)) + "\\.(\\d+)\\.bak");
			int num = -1;
			string[] files = Directory.GetFiles(Path.GetDirectoryName(filePath));
			foreach (string path in files)
			{
				string fileName = Path.GetFileName(path);
				Match match = regex.Match(fileName);
				if (match.Success && match.Length == fileName.Length)
				{
					string value = regex.Match(fileName).Groups[1].Value;
					if (int.TryParse(value, out var result) && result > num)
					{
						num = result;
					}
				}
			}
			return filePath + "." + (num + 1) + ".bak";
		}

		public abstract IEnumerable<IMigration> GetDocumentMigrations(Version fileVersion);

		public abstract Version GetCurrentFileVersion();

		public abstract string GetMigrationMessage(string filePath);

		public abstract string GetInvalidVersionExceptionMessage(Version fileVersion, string filePath);

		private static Version GetFileVersion(string filePath)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(filePath);
				xmlTextReader.MoveToContent();
				if (xmlTextReader.MoveToAttribute("Version"))
				{
					string value = xmlTextReader.Value;
					return new Version(value);
				}
			}
			catch (DirectoryNotFoundException)
			{
				throw;
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch (Exception ex3)
			{
				throw new InvalidVersionException((string)null, ErrorMessages.VersionUtil_GetFileVersionError, ex3);
			}
			throw new InvalidVersionException((string)null, ErrorMessages.VersionUtil_FileVersionNotFound);
		}
	}
}
