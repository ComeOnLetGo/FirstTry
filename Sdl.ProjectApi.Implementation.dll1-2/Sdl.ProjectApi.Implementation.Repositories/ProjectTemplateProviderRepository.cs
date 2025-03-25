using System.Collections.Generic;
using Sdl.Desktop.Platform.Implementation.SystemIO;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectTemplateProviderRepository : IProjectTemplatesProviderRepository
	{
		private readonly IMainRepository _mainRepository;

		private readonly IProjectPathUtil _projectPathUtil;

		private readonly IApplication _application;

		private readonly IProjectTemplateRepositoryFactory _projectTemplateRepositoryFactory;

		private readonly IFile _fileUtil;

		public ProjectTemplateProviderRepository(IMainRepository mainRepository, IProjectPathUtil projectPathUtil, IApplication application, IProjectTemplateRepositoryFactory projectTemplateRepositoryFactory, IFile fileUtil)
		{
			_mainRepository = mainRepository;
			_projectPathUtil = projectPathUtil;
			_application = application;
			_projectTemplateRepositoryFactory = projectTemplateRepositoryFactory;
			_fileUtil = fileUtil;
			RemoveMissingTemplates();
		}

		public List<IProjectTemplate> GetProjectTemplates(IProjectsProvider projectsProvider)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			List<IProjectTemplate> list = new List<IProjectTemplate>();
			foreach (ProjectTemplateListItem projectTemplate2 in _mainRepository.XmlProjectServer.ProjectTemplates)
			{
				IProjectTemplateRepository repository = _projectTemplateRepositoryFactory.Create(_application, _projectPathUtil, projectTemplate2);
				ProjectTemplate projectTemplate = new ProjectTemplate(projectsProvider, _projectPathUtil, repository, (IDirectory)new DirectoryWrapper());
				if (!projectTemplate.IsCloudTemplate())
				{
					list.Add((IProjectTemplate)(object)projectTemplate);
				}
			}
			return list;
		}

		public void Save(IEnumerable<IProjectTemplate> projectTemplates)
		{
			_mainRepository.XmlProjectServer.ProjectTemplates.Clear();
			foreach (IProjectTemplate projectTemplate in projectTemplates)
			{
				if (((IProjectConfiguration)projectTemplate).Repository is ProjectTemplateRepository projectTemplateRepository)
				{
					_mainRepository.XmlProjectServer.ProjectTemplates.Add(projectTemplateRepository.ProjectTemplateListItem);
				}
			}
		}

		private void RemoveMissingTemplates()
		{
			_mainRepository.XmlProjectServer.ProjectTemplates.RemoveAll((ProjectTemplateListItem pt) => !_fileUtil.Exists(pt.ProjectTemplateFilePath) && _projectPathUtil.IsPathRooted(pt.ProjectTemplateFilePath));
		}
	}
}
