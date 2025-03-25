namespace Sdl.ProjectApi.Implementation.Xml
{
	public static class StringSanitizeXmlExtensions
	{
		public static string XmlSanitize(this string xml)
		{
			return XmlSanitizeUtil.SanitizeXmlString(xml);
		}

		public static string XmlSanitize(this string xml, string xmlVersion)
		{
			return XmlSanitizeUtil.SanitizeXmlString(xml, xmlVersion);
		}
	}
}
