using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	public interface IMainRepository
	{
		ProjectServer XmlProjectServer { get; set; }

		void Save(List<IProject> projects, string localDataFolder);
	}
}
