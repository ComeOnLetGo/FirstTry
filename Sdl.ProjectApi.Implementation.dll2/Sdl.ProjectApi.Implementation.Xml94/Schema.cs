using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal static class Schema
	{
		private static readonly object _syncObject = new object();

		private static XmlSchemaSet _schemaSet;

		public static XmlSchemaSet GetSchemaSet()
		{
			lock (_syncObject)
			{
				if (_schemaSet == null)
				{
					_schemaSet = new XmlSchemaSet();
					using (XmlReader schemaDocument = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("Sdl.ProjectApi.Implementation.Xml.ProjectApiObjects.xsd")))
					{
						_schemaSet.Add("", schemaDocument);
					}
					_schemaSet.Compile();
				}
				return _schemaSet;
			}
		}
	}
}
