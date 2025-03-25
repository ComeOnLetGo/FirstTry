using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class ProjectRepositoryFactoryStrategy : IProjectRepositoryFactory
	{
		private readonly List<IProjectRepositoryFactory> _factories;

		public ProjectRepositoryFactoryStrategy(List<IProjectRepositoryFactory> factories)
		{
			_factories = factories ?? throw new ArgumentNullException("factories");
		}

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, ProjectListItem projectListItem)
		{
			foreach (IProjectRepositoryFactory factory in _factories)
			{
				IProjectRepository projectRepository = factory.Create(application, projectPathUtil, projectListItem);
				if (projectRepository != null)
				{
					return projectRepository;
				}
			}
			return null;
		}

		public IProjectRepository Create(IApplication application, IProjectPathUtil projectPathUtil, string projectFilePath)
		{
			foreach (IProjectRepositoryFactory factory in _factories)
			{
				IProjectRepository projectRepository = factory.Create(application, projectPathUtil, projectFilePath);
				if (projectRepository != null)
				{
					return projectRepository;
				}
			}
			return null;
		}
	}
}
