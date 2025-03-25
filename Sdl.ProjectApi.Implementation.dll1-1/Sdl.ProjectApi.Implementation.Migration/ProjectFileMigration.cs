using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal class ProjectFileMigration : AbstractFileMigration
	{
		public ProjectFileMigration()
		{
		}

		public ProjectFileMigration(IServerEvents serverEvents)
			: base(serverEvents)
		{
			_serverEvents = serverEvents;
		}

		protected override bool CanMigrate(string projectFilePath, Version projectFileVersion, Version currentFileVersion)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			string projectFileType = GetProjectFileType(projectFilePath);
			if (projectFileType == "Project" && projectFileVersion < new Version("4.0.0.0"))
			{
				string text = string.Format(ErrorMessages.VersionUtil_InvalidOlderProjectFileVersion, projectFileVersion, GetProjectName(projectFilePath));
				throw new InvalidVersionException(projectFileVersion.ToString(), text);
			}
			return projectFileVersion < currentFileVersion;
		}

		public override IEnumerable<IMigration> GetDocumentMigrations(Version fileVersion)
		{
			return new List<IMigration>();
		}

		public override Version GetCurrentFileVersion()
		{
			return new Version("4.0.0.0");
		}

		public override string GetMigrationMessage(string filePath)
		{
			return null;
		}

		public override string GetInvalidVersionExceptionMessage(Version fileVersion, string filePath)
		{
			return string.Format(ErrorMessages.VersionUtil_InvalidProjectFileVersion, fileVersion, GetProjectName(filePath));
		}

		private string GetProjectName(string projectFilePath)
		{
			string text = Path.GetFileNameWithoutExtension(projectFilePath);
			Regex regex = new Regex("-\\d+-\\d+h\\d+m\\d+s$");
			Match match = regex.Match(text);
			if (match.Success)
			{
				int index = match.Index;
				if (index > -1)
				{
					text = text.Substring(0, index);
				}
			}
			return text;
		}

		private static string GetProjectFileType(string filePath)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(filePath);
				xmlTextReader.MoveToContent();
				return xmlTextReader.Name;
			}
			catch (Exception)
			{
				throw new ProjectApiException(ErrorMessages.ProjectFileMigration_UnableToDetermineProjectFileType);
			}
		}
	}
}
