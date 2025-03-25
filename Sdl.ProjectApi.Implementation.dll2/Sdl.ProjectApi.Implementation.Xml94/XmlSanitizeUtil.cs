using System;
using System.Text;

namespace Sdl.ProjectApi.Implementation.Xml
{
	public static class XmlSanitizeUtil
	{
		public static bool IsLegalXmlChar(int character, string xmlVersion)
		{
			if (!(xmlVersion == "1.1"))
			{
				if (xmlVersion == "1.0")
				{
					if (character != 9 && character != 10 && character != 13 && (character < 32 || character > 55295) && (character < 57344 || character > 65533))
					{
						if (character >= 65536)
						{
							return character <= 1114111;
						}
						return false;
					}
					return true;
				}
				throw new ArgumentOutOfRangeException("xmlVersion", "It is not a valid XML version.");
			}
			if (character > 8)
			{
				switch (character)
				{
				default:
					if ((character < 127 || character > 132) && (character < 134 || character > 159))
					{
						return character <= 1114111;
					}
					break;
				case 11:
				case 12:
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
					break;
				}
			}
			return false;
		}

		public static bool IsLegalXmlChar(int character)
		{
			return IsLegalXmlChar(character, "1.0");
		}

		public static string SanitizeXmlString(string xml)
		{
			if (xml == null)
			{
				throw new ArgumentNullException("xml");
			}
			StringBuilder stringBuilder = new StringBuilder(xml.Length);
			foreach (char c in xml)
			{
				if (IsLegalXmlChar(c))
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		public static string SanitizeXmlString(string xml, string xmlVersion)
		{
			if (xml == null)
			{
				throw new ArgumentNullException("xml");
			}
			StringBuilder stringBuilder = new StringBuilder(xml.Length);
			foreach (char c in xml)
			{
				if (IsLegalXmlChar(c, xmlVersion))
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
