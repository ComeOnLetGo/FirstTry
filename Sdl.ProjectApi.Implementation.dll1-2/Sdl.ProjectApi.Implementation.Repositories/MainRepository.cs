using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class MainRepository : IMainRepository
	{
		private const string ProjectServerFileName = "projects.xml";

		private const string DefaultServerResourceName = "Sdl.ProjectApi.Implementation.Xml.ProjectServer.xml";

		private readonly ILogger<MainRepository> _log;

		public ProjectServer XmlProjectServer { get; set; }

		public MainRepository(ILogger<MainRepository> log, string localDataFolder)
		{
			_log = log;
			Load(localDataFolder);
		}

		private void Load(string localdataFolder)
		{
			//IL_0026: Expected O, but got Unknown
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			string projectServerFilePath = GetProjectServerFilePath(localdataFolder);
			try
			{
				if (File.Exists(projectServerFilePath))
				{
					try
					{
						VersionUtil.CheckServerVersion(projectServerFilePath);
						XmlProjectServer = GetProjectServer(projectServerFilePath);
						return;
					}
					catch (InvalidVersionException val)
					{
						InvalidVersionException val2 = val;
						LoggerExtensions.LogError((ILogger)(object)_log, (Exception)(object)val2, "Failed to load server file. Falling back to default.", Array.Empty<object>());
						XmlProjectServer = GetDefaultProjectServer();
						return;
					}
				}
				XmlProjectServer = GetDefaultProjectServer();
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Server_ErrorLoadingXmlFile, projectServerFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		private ProjectServer GetProjectServer(string projectServerFilePath)
		{
			using Stream stream = File.OpenRead(projectServerFilePath);
			return ProjectServer.Deserialize(stream);
		}

		private ProjectServer GetDefaultProjectServer()
		{
			using Stream stream = Util.GetEmbeddedResourceStream("Sdl.ProjectApi.Implementation.Xml.ProjectServer.xml");
			return ProjectServer.Deserialize(stream);
		}

		private string GetProjectServerFilePath(string localDataFolder)
		{
			return Path.Combine(localDataFolder, "projects.xml");
		}

		public void Save(List<IProject> projects, string localDataFolder)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			string projectServerFilePath = GetProjectServerFilePath(localDataFolder);
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(projectServerFilePath);
				if (directoryInfo.Parent != null && !directoryInfo.Parent.Exists)
				{
					directoryInfo.Parent.Create();
				}
				XmlProjectServer.Projects.Clear();
				foreach (Project project in projects)
				{
					XmlProjectServer.Projects.Add(project.CreateProjectListItem());
				}
				XmlProjectServer.Serialize(projectServerFilePath);
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Server_ErrorSavingXmlFile, projectServerFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}
	}
}
