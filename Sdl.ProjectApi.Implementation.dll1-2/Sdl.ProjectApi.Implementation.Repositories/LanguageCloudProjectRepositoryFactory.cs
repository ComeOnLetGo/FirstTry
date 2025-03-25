using System;
using System.IO;
using System.Xml;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.ProjectSettings;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class LanguageCloudProjectRepositoryFactory : IProjectRepositoryFactory
	{
		private const string SettingsBundles = "SettingsBundles";

		private const string ProjectOriginSetting = "ProjectOrigin";

		private const string LCProjectType = "LC project";

		private const string NodeId = "Id";

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectListItem projectListItem)
		{
			SettingsBundle settingsBundle = projectListItem?.SettingsBundle;
			if (settingsBundle != null)
			{
				ISettingsBundle val = settingsBundle.LoadSettingsBundle(null);
				string projectOrigin = val.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>().ProjectOrigin;
				if (projectOrigin == "LC project")
				{
					return new LanguageCloudProjectRepository(application, projectPathUtil);
				}
			}
			return null;
		}

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, string projectFilePath)
		{
			if (string.IsNullOrEmpty(projectFilePath) || !File.Exists(projectFilePath))
			{
				return null;
			}
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(projectFilePath);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("//*[@Id='ProjectOrigin']");
				if (xmlNode != null && xmlNode.InnerText.Equals("LC project", StringComparison.OrdinalIgnoreCase))
				{
					return new LanguageCloudProjectRepository(application, projectPathUtil);
				}
			}
			catch
			{
				return null;
			}
			return null;
		}
	}
}
