using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Sdl.ProjectApi.Implementation.SecureProjects
{
	public static class SecureProjectUtil
	{
		private const string ElementEncryptedData = "EncryptedData";

		private const string ElementGeneralProjectInfo = "GeneralProjectInfo";

		private const string AttributeIsSecure = "IsSecure";

		public static bool IsSecureProject(string projectFilePath)
		{
			if (string.IsNullOrEmpty(projectFilePath) || !File.Exists(projectFilePath))
			{
				return false;
			}
			try
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(projectFilePath);
				xmlTextReader.MoveToContent();
				if (xmlTextReader.ReadToFollowing("GeneralProjectInfo") && xmlTextReader.MoveToAttribute("IsSecure"))
				{
					return Convert.ToBoolean(xmlTextReader.Value);
				}
			}
			catch
			{
				return false;
			}
			return false;
		}

		public static void Remove(XmlDocument xmlDocument, string elementName)
		{
			XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);
			while (elementsByTagName.Count > 0)
			{
				elementsByTagName[0]?.ParentNode?.RemoveChild(elementsByTagName[0]);
			}
		}

		public static byte[] GetDecodedKey(string base64EncodedKey)
		{
			return Convert.FromBase64String(base64EncodedKey);
		}

		public static void Encrypt(XmlDocument xmlDocument, string elementName, SymmetricAlgorithm key)
		{
			XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);
			List<KeyValuePair<XmlElement, EncryptedData>> list = new List<KeyValuePair<XmlElement, EncryptedData>>();
			foreach (object item in elementsByTagName)
			{
				if (item is XmlElement xmlElement)
				{
					EncryptedXml encryptedXml = new EncryptedXml();
					byte[] cipherValue = encryptedXml.EncryptData(xmlElement, key, content: false);
					EncryptedData encryptedData = new EncryptedData();
					encryptedData.Type = "http://www.w3.org/2001/04/xmlenc#Element";
					encryptedData.EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#aes256-cbc");
					encryptedData.CipherData.CipherValue = cipherValue;
					list.Add(new KeyValuePair<XmlElement, EncryptedData>(xmlElement, encryptedData));
				}
			}
			foreach (KeyValuePair<XmlElement, EncryptedData> item2 in list)
			{
				EncryptedXml.ReplaceElement(item2.Key, item2.Value, content: false);
			}
		}

		public static void Decrypt(XmlDocument xmlDocument, SymmetricAlgorithm key)
		{
			XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("EncryptedData");
			List<KeyValuePair<EncryptedData, XmlElement>> list = new List<KeyValuePair<EncryptedData, XmlElement>>();
			foreach (object item in elementsByTagName)
			{
				if (item is XmlElement value)
				{
					EncryptedData encryptedData = new EncryptedData();
					encryptedData.LoadXml(value);
					list.Add(new KeyValuePair<EncryptedData, XmlElement>(encryptedData, value));
				}
			}
			foreach (KeyValuePair<EncryptedData, XmlElement> item2 in list)
			{
				EncryptedXml encryptedXml = new EncryptedXml();
				byte[] decryptedData = encryptedXml.DecryptData(item2.Key, key);
				encryptedXml.ReplaceData(item2.Value, decryptedData);
			}
		}
	}
}
