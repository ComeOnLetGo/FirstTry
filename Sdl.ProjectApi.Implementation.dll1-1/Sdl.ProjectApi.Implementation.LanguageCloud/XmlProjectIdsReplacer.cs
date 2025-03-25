using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Platform.Extensions;
using Sdl.ProjectApi.Settings.SettingTypes;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class XmlProjectIdsReplacer : IXmlProjectIdsReplacer
	{
		private readonly XmlDocument _xmlDocument;

		private readonly ILogger _logger;

		public XmlProjectIdsReplacer(XmlDocument xmlDocument, ILogger logger)
		{
			_xmlDocument = xmlDocument;
			_logger = logger;
		}

		public void ReplaceProjectIds(string projectFilePath, string newProjectId, Dictionary<string, string> fileIdAssociations)
		{
			_xmlDocument.Load(projectFilePath);
			string stringGuid = GetStringGuid(newProjectId);
			_xmlDocument.DocumentElement.SetAttribute("Guid", stringGuid);
			foreach (string key in fileIdAssociations.Keys)
			{
				string stringGuid2 = GetStringGuid(key);
				XmlNodeList xmlNodeList = _xmlDocument.SelectNodes($"//*/@*[.='{stringGuid2}']");
				foreach (XmlNode item in xmlNodeList)
				{
					fileIdAssociations.TryGetValue(key, out var value);
					item.Value = GetStringGuid(value);
				}
			}
			_xmlDocument.Save(projectFilePath);
		}

		public void ReplaceProjectIdForReturnPackage(string projectFilePath, MigrationData migrationData)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			if (migrationData == null)
			{
				return;
			}
			_xmlDocument.Load(projectFilePath);
			XmlElement documentElement = _xmlDocument.DocumentElement;
			IdAssociation projectIdAssoication = migrationData.ProjectIdAssoication;
			documentElement.SetAttribute("Guid", (projectIdAssoication != null) ? projectIdAssoication.OldId.ToString() : null);
			foreach (IdAssociation fileIdAssociation in migrationData.FileIdAssociations)
			{
				if ((int)fileIdAssociation.AssociationType == 0)
				{
					XmlNode xmlNode = _xmlDocument.SelectSingleNode("//PackageProject/ProjectFiles/ProjectFile/LanguageFiles/LanguageFile[@Guid = '" + fileIdAssociation.NewId.ToString() + "']");
					if (xmlNode != null)
					{
						xmlNode.Attributes.GetNamedItem("Guid").Value = fileIdAssociation.OldId.ToString();
					}
					XmlNode xmlNode2 = _xmlDocument.SelectSingleNode("//PackageProject/Tasks/ManualTask/Files/TaskFile[@LanguageFileGuid = '" + fileIdAssociation.NewId.ToString() + "']");
					if (xmlNode2 != null)
					{
						xmlNode2.Attributes.GetNamedItem("LanguageFileGuid").Value = fileIdAssociation.OldId.ToString();
					}
				}
			}
			ReplaceCustomerNode(migrationData);
			_xmlDocument.Save(projectFilePath);
		}

		private void ReplaceCustomerNode(MigrationData migrationData)
		{
			XmlNode xmlNode = _xmlDocument.SelectSingleNode("//PackageProject/GeneralProjectInfo/Customer");
			if (xmlNode != null)
			{
				if (migrationData.PackageProjectCustomer != null)
				{
					xmlNode.Attributes.GetNamedItem("Guid").Value = migrationData.PackageProjectCustomer.Guid.ToString();
					xmlNode.Attributes.GetNamedItem("Name").Value = migrationData.PackageProjectCustomer.Name;
					xmlNode.Attributes.GetNamedItem("Email").Value = migrationData.PackageProjectCustomer.Email;
				}
				else
				{
					xmlNode.ParentNode.RemoveChild(xmlNode);
				}
			}
		}

		private string GetStringGuid(string id)
		{
			if (!Guid.TryParse(id, out var result))
			{
				return GUIDExtensions.ToGuid(id).ToString();
			}
			return result.ToString();
		}
	}
}
