using System.IO;
using System.Xml;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	public interface IProjectRepositorySerializer
	{
		Sdl.ProjectApi.Implementation.Xml.Project XmlProject { get; set; }

		Sdl.ProjectApi.Implementation.Xml.Project Deserialize(Stream projectManifestStream);

		Sdl.ProjectApi.Implementation.Xml.Project Deserialize(string projectFilePath);

		void DeserializeProject(StringReader reader);

		void DeserializeProject(XmlDocument xmlDocument);

		string Serialize();

		void Serialize(StreamWriter stream);

		void Serialize(string projectFilePath);

		XmlDocument SerializeToXmlDocument();
	}
}
