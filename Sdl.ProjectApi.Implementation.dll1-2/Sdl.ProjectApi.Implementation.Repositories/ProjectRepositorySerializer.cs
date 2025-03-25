using System;
using System.IO;
using System.Xml;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectRepositorySerializer : IProjectRepositorySerializer
	{
		public Sdl.ProjectApi.Implementation.Xml.Project XmlProject { get; set; }

		public ProjectRepositorySerializer()
		{
			XmlProject = new Sdl.ProjectApi.Implementation.Xml.Project();
		}

		public ProjectRepositorySerializer(IPackageProject packageProject)
		{
			XmlProject = new Sdl.ProjectApi.Implementation.Xml.Project
			{
				Version = "4.0.0.0",
				Guid = ((IProject)packageProject).Guid,
				GeneralProjectInfo = new GeneralProjectInfo
				{
					Name = ((IProject)packageProject).Name,
					IsImported = true,
					CreatedAt = DateTime.UtcNow,
					CreatedBy = ((IProject)packageProject).CreatedBy.UserId
				}
			};
			XmlProject.GeneralProjectInfo.Status = ProjectStatus.Started;
			XmlProject.GeneralProjectInfo.StartedAtSpecified = true;
			XmlProject.GeneralProjectInfo.StartedAt = packageProject.StartedAt;
			XmlProject.GeneralProjectInfo.Description = ((IProject)packageProject).Description;
			XmlProject.GeneralProjectInfo.DueDateSpecified = true;
			XmlProject.GeneralProjectInfo.DueDate = ((IProject)packageProject).DueDate.ToUniversalTime();
			XmlProject.SourceLanguageCode = ((LanguageBase)((IProject)packageProject).SourceLanguage).IsoAbbreviation;
			if (((IProject)packageProject).IsSecure)
			{
				XmlProject.GeneralProjectInfo.IsSecureSpecified = true;
				XmlProject.GeneralProjectInfo.IsSecure = ((IProject)packageProject).IsSecure;
			}
			XmlProject.SettingsBundleGuid = Guid.NewGuid();
		}

		public void DeserializeProject(XmlDocument xmlDocument)
		{
			XmlProject = Sdl.ProjectApi.Implementation.Xml.Project.Deserialize(xmlDocument);
		}

		public void DeserializeProject(StringReader reader)
		{
			XmlProject = Sdl.ProjectApi.Implementation.Xml.Project.Deserialize(reader);
		}

		public Sdl.ProjectApi.Implementation.Xml.Project Deserialize(string projectFilePath)
		{
			return Sdl.ProjectApi.Implementation.Xml.Project.Deserialize(projectFilePath);
		}

		public virtual string Serialize()
		{
			return XmlProject.Serialize();
		}

		public virtual XmlDocument SerializeToXmlDocument()
		{
			return XmlProject.SerializeToXmlDocument();
		}

		public virtual void Serialize(string projectFilePath)
		{
			XmlProject.Serialize(projectFilePath);
		}

		public virtual void Serialize(StreamWriter stream)
		{
			XmlProject.Serialize(stream);
		}

		public Sdl.ProjectApi.Implementation.Xml.Project Deserialize(Stream projectManifestStream)
		{
			return Sdl.ProjectApi.Implementation.Xml.Project.Deserialize(projectManifestStream);
		}
	}
}
