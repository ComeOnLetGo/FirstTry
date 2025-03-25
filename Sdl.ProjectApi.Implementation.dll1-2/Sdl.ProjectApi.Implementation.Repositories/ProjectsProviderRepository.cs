using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Platform.Services;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Interfaces;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ProjectsProviderRepository : IProjectsProviderRepository
	{
		private readonly IProjectPathUtil _projectPathUtil;

		private readonly IApplication _application;

		private readonly IProjectRepositoryFactory _projectRepositoryFactory;

		private readonly IMainRepository _mainRepository;

		private readonly IProjectFilePathValidator _projFilePaths;

		private readonly IEventAggregator _eventAggregator;

		private readonly ILogger _logger;

		public ProjectsProviderRepository(IProjectPathUtil projectPathUtil, IApplication application, IProjectRepositoryFactory projectRepositoryFactory, IMainRepository mainRepository, IProjectFilePathValidator projFilePaths, IEventAggregator eventAggregator, ILogger<ProjectsProviderRepository> logger)
		{
			_application = application;
			_projectPathUtil = projectPathUtil;
			_mainRepository = mainRepository;
			_projectRepositoryFactory = projectRepositoryFactory;
			_projFilePaths = projFilePaths;
			_eventAggregator = eventAggregator;
			_logger = (ILogger)(object)logger;
		}

		public List<IProject> GetProjects(IProjectsProvider projectsProvider, string localDataFolder, IProjectOperation projectOperation)
		{
			List<IProject> list = new List<IProject>();
			RemoveMissingFromDiskProjects();
			List<ProjectListItem> list2 = new List<ProjectListItem>();
			foreach (ProjectListItem project2 in _mainRepository.XmlProjectServer.Projects)
			{
				IProjectRepository repository = _projectRepositoryFactory.Create(_application, _projectPathUtil, project2);
				Project project = new Project(projectsProvider, project2, repository, projectOperation, _eventAggregator);
				try
				{
					project.Check();
					list.Add((IProject)(object)project);
				}
				catch (Exception)
				{
					list2.Add(project2);
				}
			}
			RemoveInvalidProjects(list2, "Project {0} from path {1} removed from projects.xml because deserialization failed.");
			return list;
		}

		public IProject LoadNewProject(string projectFilePath, IProjectsProvider projectsProvider, IProjectOperation projectOperation)
		{
			IProjectRepository repository = _projectRepositoryFactory.Create(_application, _projectPathUtil, projectFilePath);
			return (IProject)(object)new Project(projectsProvider, projectFilePath, licenseOverrideRequired: false, repository, projectOperation, _eventAggregator);
		}

		public void Save(List<IProject> projects, string localDataFolder)
		{
			_mainRepository.Save(projects, localDataFolder);
		}

		private void RemoveMissingFromDiskProjects()
		{
			List<ProjectListItem> unvalidatedProjects = _mainRepository.XmlProjectServer.Projects.Where((ProjectListItem p) => !_projFilePaths.Validate(p.ProjectFilePath)).ToList();
			RemoveInvalidProjects(unvalidatedProjects, "Project {0} removed from projects.xml because it is no longer existing on this path {1}");
		}

		private void RemoveInvalidProjects(List<ProjectListItem> unvalidatedProjects, string reason)
		{
			if (unvalidatedProjects.Any())
			{
				unvalidatedProjects.ForEach(delegate(ProjectListItem project)
				{
					_mainRepository.XmlProjectServer.Projects.Remove(project);
					LoggerExtensions.LogInformation(_logger, string.Format(reason, project.ProjectInfo.Name, project.ProjectFilePath), Array.Empty<object>());
				});
			}
		}
	}
}
