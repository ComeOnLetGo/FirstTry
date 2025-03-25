using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.Versioning;

namespace Sdl.ProjectApi.Implementation
{
	public class ApplicationRepository : IApplicationRepository
	{
		private const string ApplicationFileName = "Sdl.ProjectApi.xml";

		private const string DefaultApplicationResourceName = "Sdl.ProjectApi.Implementation.Xml.Application.xml";

		private readonly ILogger _log;

		private readonly string _applicationFilePath;

		private Sdl.ProjectApi.Implementation.Xml.Application _xmlApplication;

		private readonly object _lockObject = new object();

		public ApplicationRepository(ILogger log, string applicationFilePath)
		{
			_log = log;
			_applicationFilePath = applicationFilePath;
			Load();
		}

		public ApplicationRepository(ILogger log)
			: this(log, GetDefaultApplicationFilePath())
		{
		}

		private void Load()
		{
			//IL_004c: Expected O, but got Unknown
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			using Stream stream = (File.Exists(_applicationFilePath) ? File.OpenRead(_applicationFilePath) : Util.GetEmbeddedResourceStream("Sdl.ProjectApi.Implementation.Xml.Application.xml"));
			try
			{
				if (File.Exists(_applicationFilePath))
				{
					VersionUtil.CheckApplicationVersion(_applicationFilePath);
				}
				_xmlApplication = Sdl.ProjectApi.Implementation.Xml.Application.Deserialize(stream);
			}
			catch (InvalidVersionException val)
			{
				InvalidVersionException val2 = val;
				LoggerExtensions.LogError(_log, (Exception)(object)val2, "Failed to load application file. Falling back to default.", Array.Empty<object>());
				using Stream stream2 = Util.GetEmbeddedResourceStream("Sdl.ProjectApi.Implementation.Xml.Application.xml");
				_xmlApplication = Sdl.ProjectApi.Implementation.Xml.Application.Deserialize(stream2);
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Application_ErrorDeserializing, _applicationFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
		}

		public bool Exists(string localDataFolder)
		{
			return _xmlApplication.LocalProjectServers.Any((LocalProjectServerInfo p) => p.LocalDataFolder.Equals(localDataFolder, StringComparison.OrdinalIgnoreCase));
		}

		public void AddProjectsProvider(ProjectsProviderInfo providerInfo)
		{
			LocalProjectServerInfo item = new LocalProjectServerInfo
			{
				LocalDataFolder = providerInfo.LocalDataFolder,
				UserId = providerInfo.UserId
			};
			_xmlApplication.LocalProjectServers.Add(item);
		}

		public void RemoveProjectsProvider(string localDataFolder)
		{
			LocalProjectServerInfo localProjectServerInfo = _xmlApplication.LocalProjectServers.Find((LocalProjectServerInfo s) => s.LocalDataFolder == localDataFolder);
			if (localProjectServerInfo != null)
			{
				_xmlApplication.LocalProjectServers.Remove(localProjectServerInfo);
			}
		}

		public void UpdateLocalProjectServerDataFolder(string oldDataFolder, string newDataFolder)
		{
			LocalProjectServerInfo localProjectServerInfo = _xmlApplication.LocalProjectServers.Find((LocalProjectServerInfo s) => s.LocalDataFolder == oldDataFolder);
			if (localProjectServerInfo != null)
			{
				localProjectServerInfo.LocalDataFolder = newDataFolder;
			}
		}

		public List<ProjectsProviderInfo> GetAllProviders()
		{
			return ((IEnumerable<LocalProjectServerInfo>)_xmlApplication.LocalProjectServers).Select((Func<LocalProjectServerInfo, ProjectsProviderInfo>)((LocalProjectServerInfo s) => new ProjectsProviderInfo
			{
				LocalDataFolder = s.LocalDataFolder,
				UserId = s.UserId
			})).ToList();
		}

		public void Save()
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			lock (_lockObject)
			{
				try
				{
					Util.EnsureFilePathDirectoryExists(_applicationFilePath);
					_xmlApplication.Serialize(_applicationFilePath);
				}
				catch (Exception ex)
				{
					throw new ProjectApiException(string.Format(ErrorMessages.Application_ErrorSerializing, _applicationFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
				}
			}
		}

		private static string GetDefaultApplicationFilePath()
		{
			string text = Path.Combine(VersionedPaths.UserAppDataPath, "Sdl.ProjectApi.xml");
			string text2 = Path.Combine(VersionedPaths.LocalUserAppDataPath, "Sdl.ProjectApi.xml");
			if (!File.Exists(text) && File.Exists(text2))
			{
				Util.CopyFile(text2, text);
			}
			return text;
		}
	}
}
