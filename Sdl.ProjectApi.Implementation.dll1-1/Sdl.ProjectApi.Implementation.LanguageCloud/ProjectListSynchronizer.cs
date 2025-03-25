using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class ProjectListSynchronizer : IProjectListSynchronizer
	{
		private readonly IProjectsProvider _projectsProvider;

		private readonly object _syncObject = new object();

		private readonly List<Guid> _syncProjectsList = new List<Guid>();

		private bool _isSyncInProgress;

		public ProjectListSynchronizer(IProjectsProvider projectsProvider)
		{
			_projectsProvider = projectsProvider;
		}

		public async Task<IProjectOperationResult> RefreshProjectsListAsync()
		{
			lock (_syncObject)
			{
				if (_isSyncInProgress)
				{
					return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true)
					{
						Result = new RefreshOperationsResult
						{
							WasPerformed = false
						}
					};
				}
				_isSyncInProgress = true;
			}
			try
			{
				if (_projectsProvider == null)
				{
					return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: false)
					{
						Result = new RefreshOperationsResult
						{
							WasPerformed = false
						}
					};
				}
				_projectsProvider.Reload(false);
				return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true)
				{
					Result = new RefreshOperationsResult
					{
						WasPerformed = true
					}
				};
			}
			finally
			{
				lock (_syncObject)
				{
					_isSyncInProgress = false;
				}
			}
		}

		public async Task<IProjectOperationResult> RefreshProjectAsync(IProject project)
		{
			SimpleBooleanProjectOperationResult simpleBooleanProjectOperationResult = new SimpleBooleanProjectOperationResult(result: true);
			try
			{
				if (project.AllowsOperation("RefreshProjectOperation"))
				{
					lock (_syncObject)
					{
						if (_syncProjectsList.Contains(project.Guid))
						{
							return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true)
							{
								Result = new RefreshOperationsResult
								{
									WasPerformed = false
								}
							};
						}
						_syncProjectsList.Add(project.Guid);
					}
					simpleBooleanProjectOperationResult = (await project.ExecuteOperationAsync("RefreshProjectOperation", new object[0]).ConfigureAwait(continueOnCapturedContext: false)) as SimpleBooleanProjectOperationResult;
				}
			}
			catch (Exception)
			{
				simpleBooleanProjectOperationResult = new SimpleBooleanProjectOperationResult(result: false);
			}
			finally
			{
				lock (_syncObject)
				{
					_syncProjectsList.Remove(project.Guid);
				}
			}
			simpleBooleanProjectOperationResult.Result = new RefreshOperationsResult
			{
				WasPerformed = true
			};
			return (IProjectOperationResult)(object)simpleBooleanProjectOperationResult;
		}

		public async Task<IProjectOperationResult> RefreshProjectsAsync(List<IProject> projects)
		{
			IEnumerable<Task<IProjectOperationResult>> tasks = projects.Select((IProject p) => RefreshProjectAsync(p));
			return (IProjectOperationResult)(object)((await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false)).All((IProjectOperationResult r) => (r as SimpleBooleanProjectOperationResult).IsSuccesful) ? new SimpleBooleanProjectOperationResult(result: true)
			{
				Result = new RefreshOperationsResult
				{
					WasPerformed = true
				}
			} : new SimpleBooleanProjectOperationResult(result: false)
			{
				Result = new RefreshOperationsResult
				{
					WasPerformed = true
				}
			});
		}
	}
}
