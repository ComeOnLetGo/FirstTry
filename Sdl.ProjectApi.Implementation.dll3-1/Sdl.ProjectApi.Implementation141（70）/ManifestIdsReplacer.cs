using System;
using System.Collections.Generic;
using System.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ManifestIdsReplacer
	{
		private Guid _newProjectId;

		public ManifestIdsReplacer(Guid newProjectId)
		{
			_newProjectId = newProjectId;
		}

		public void ReplaceIds(string fullPathToProject)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fullPathToProject);
			ReplaceIds(xmlDocument, fullPathToProject);
		}

		public void ReplaceIds(XmlDocument xmlDoc, string fullPathToProject)
		{
			xmlDoc.DocumentElement.SetAttribute("Guid", _newProjectId.ToString());
			string[] guidAttributeXPaths = new string[7] { "LanguageDirections/LanguageDirection/@Guid", "LanguageDirections/LanguageDirection/@SettingsBundleGuid", "ProjectFiles/ProjectFile/@Guid", "ProjectFiles/ProjectFile/LanguageFiles/LanguageFile/@Guid", "ProjectFiles/ProjectFile/LanguageFiles/LanguageFile/@SettingsBundleGuid", "ProjectFiles/ProjectFile/LanguageFiles/LanguageFile/ChildFiles/LanguageFileRef/@LanguageFileGuid", "ProjectFiles/ProjectFile/LanguageFiles/LanguageFile/FileVersions/FileVersion/@Guid" };
			Dictionary<string, List<XmlNode>> dictionary = GroupById(xmlDoc, guidAttributeXPaths);
			foreach (KeyValuePair<string, List<XmlNode>> item in dictionary)
			{
				string value = Guid.NewGuid().ToString();
				foreach (XmlNode item2 in item.Value)
				{
					item2.Value = value;
				}
			}
			XmlNode xmlNode = xmlDoc.SelectSingleNode("//Project/SettingsBundles/SettingsBundle/SettingsBundle/SettingsGroup[@Id = 'PublishProjectOperationSettings']");
			xmlNode?.ParentNode.RemoveChild(xmlNode);
			RemoveLanguageFilesSettingsGuid(xmlDoc);
			xmlDoc.Save(fullPathToProject);
		}

		private Dictionary<string, List<XmlNode>> GroupById(XmlDocument xmlDoc, string[] guidAttributeXPaths)
		{
			XmlNodeList xmlNodeList = xmlDoc.DocumentElement.SelectNodes(CombineXPathExpressions(guidAttributeXPaths));
			Dictionary<string, List<XmlNode>> dictionary = new Dictionary<string, List<XmlNode>>();
			if (xmlNodeList != null)
			{
				foreach (XmlNode item in xmlNodeList)
				{
					if (!dictionary.TryGetValue(item.Value, out var value))
					{
						value = new List<XmlNode>(1);
						dictionary[item.Value] = value;
					}
					value.Add(item);
				}
			}
			return dictionary;
		}

		private string CombineXPathExpressions(IList<string> expressions)
		{
			return string.Join(" | ", expressions);
		}

		private void RemoveLanguageFilesSettingsGuid(XmlDocument xmlDoc)
		{
			XmlNodeList xmlNodeList = xmlDoc.DocumentElement.SelectNodes("//Project/SettingsBundles/SettingsBundle/SettingsBundle/SettingsGroup[@Id = 'LanguageFileServerStateSettings']");
			foreach (XmlNode item in xmlNodeList)
			{
				XmlNode xmlNode2 = item.ParentNode?.ParentNode;
				if (xmlNode2 != null && xmlNode2.ParentNode != null)
				{
					xmlNode2.ParentNode.RemoveChild(xmlNode2);
				}
			}
		}
	}
}
