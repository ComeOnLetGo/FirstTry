using System;
using System.Collections.Generic;
using System.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public static class XliffUtil
	{
		public const string SdlXliffFileExtension = ".sdlxliff";

		public static int GetFileCount(string xliffPath)
		{
			NameTable nameTable = new NameTable();
			object obj = nameTable.Add("file");
			XmlReaderSettings settings = new XmlReaderSettings
			{
				ValidationType = ValidationType.None,
				IgnoreWhitespace = true,
				IgnoreComments = true,
				CheckCharacters = false,
				NameTable = nameTable
			};
			HashSet<string> hashSet = new HashSet<string>();
			using (XmlReader xmlReader = XmlReader.Create(xliffPath, settings))
			{
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Element && obj.Equals(xmlReader.LocalName))
					{
						string attribute = xmlReader.GetAttribute("original");
						if (attribute != null)
						{
							hashSet.Add(attribute);
						}
					}
				}
			}
			return hashSet.Count;
		}

		public static bool IsSdlXliffFile(string filePath)
		{
			return filePath.EndsWith(".sdlxliff", StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
