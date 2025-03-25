using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sdl.ProjectApi.Implementation.Migration.ServerFile
{
	internal class AddTranslateAndAnalyzeTaskSequenceMigration : IMigration
	{
		private const string DefaultServerResourceName = "Sdl.ProjectApi.Implementation.Xml.ProjectServer.xml";

		private const string TranslateAndAnalyzeTaskSequenceId = "Sdl.ProjectApi.AutomaticTasks.TranslateAndAnalyze";

		private const string NewVersion = "3.2.0.0";

		private const string ElementProjectServer = "ProjectServer";

		private const string ElementComplexTaskTemplates = "ComplexTaskTemplates";

		private const string ElementComplexTaskTemplate = "ComplexTaskTemplate";

		private const string AttributeId = "Id";

		public void Migrate(XDocument document, Version documentVersion)
		{
			XElement root = document.Root;
			if (root != null && root.Name.LocalName == "ProjectServer" && documentVersion < new Version("3.2.0.0"))
			{
				XElement xElement = root.Descendants("ComplexTaskTemplates").FirstOrDefault();
				if (xElement?.Descendants("ComplexTaskTemplate").FirstOrDefault((XElement s) => (string)s.Attribute("Id") == "Sdl.ProjectApi.AutomaticTasks.TranslateAndAnalyze") == null)
				{
					xElement?.Add(GetTaskSequenceFromEmbeddedResources());
				}
			}
		}

		private XElement GetTaskSequenceFromEmbeddedResources()
		{
			using Stream stream = Util.GetEmbeddedResourceStream("Sdl.ProjectApi.Implementation.Xml.ProjectServer.xml");
			XDocument xDocument = XDocument.Load(stream);
			return xDocument.Descendants("ComplexTaskTemplate").FirstOrDefault((XElement s) => (string)s.Attribute("Id") == "Sdl.ProjectApi.AutomaticTasks.TranslateAndAnalyze");
		}
	}
}
