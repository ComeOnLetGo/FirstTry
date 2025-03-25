using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sdl.ApiClientSdk.StudioBFF;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.ApiClientSdk.StudioBFF.ResponseModels;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectMigrationChecker : IProjectMigrationChecker
	{
		public async Task<Dictionary<string, List<IProject>>> CheckForMigration(IStudioBFFService studioBFFService, List<IProject> projects)
		{
			Dictionary<string, List<IProject>> projectsToBeMigrated = new Dictionary<string, List<IProject>>();
			IEnumerable<string> source = from p in projects
				where p.IsLCProject && p.Exists && !HasFolderStructure(p)
				select p.Guid.ToString();
			if (source.Any())
			{
				MigrationTypeResponse migrations = await studioBFFService.CheckProjectsForMigrationAsync(source.ToArray()).ConfigureAwait(continueOnCapturedContext: false);
				if (migrations.ItemsCount > 0)
				{
					IEnumerable<string> enumerable = migrations.Items.Select((MigrationType i) => i.Type).Distinct();
					foreach (string migrationType in enumerable)
					{
						projectsToBeMigrated.Add(migrationType, projects.Where((IProject p) => migrations.Items.Any((MigrationType m) => m.Type == migrationType && m.ProjectId == p.Guid.ToString())).ToList());
					}
				}
			}
			return projectsToBeMigrated;
		}

		private bool HasFolderStructure(IProject languageCloudProject)
		{
			return languageCloudProject.GetAllProjectFiles().Any((IProjectFile f) => !string.IsNullOrEmpty(f.PathInProject));
		}
	}
}
