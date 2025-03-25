using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectPackageCreation : AbstractProjectPackageCreation, IProjectPackageCreation, IPackageCreation, IPackageOperation
	{
		internal ProjectPackageCreation(IProject project, Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreation, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, projectPackageCreation, packageInitializer, projectPathUtil)
		{
		}

		internal ProjectPackageCreation(IProject project, IManualTask[] tasks, string packageName, string comment, ProjectPackageCreationOptions options, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlProjectPackageCreation(project, tasks, packageName, comment), options, packageInitializer, projectPathUtil)
		{
		}

		protected override void OnCreationCompleted()
		{
			base.Project.AddProjectPackageCreationOperation((IProjectPackageCreation)(object)this);
		}

		protected override IEnumerable<ILanguageDirection> GetLanguageDirectionsForPackage()
		{
			return from ld in (from langFile in Tasks.SelectMany((IManualTask task) => task.Files, delegate(IManualTask task, IManualTaskFile file)
					{
						IProjectFile projectFile = ((ITaskFile)file).ProjectFile;
						return (ILanguageFile)(object)((projectFile is ILanguageFile) ? projectFile : null);
					})
					select langFile.LanguageDirection).Distinct()
				where ld != null
				select ld;
		}

		protected override void AddFilesToPackage(IPackageProject pkgProject)
		{
			IManualTask[] tasks = Tasks;
			foreach (IManualTask val in tasks)
			{
				((IProject)pkgProject).AddManualTask(val, (IPackageOperationMessageReporter)(object)new MyMessageReporter(this));
			}
		}
	}
}
