using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sdl.ApiClientSdk.StudioBFF;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.BestMatchService.Common;
using Sdl.BestMatchService.Common.Identity;
using Sdl.BestMatchServiceStudioIntegration.Common;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class AccountServicesCache : IAccountServicesCache
	{
		private readonly ILanguageCloudService _languageCloudService;

		private readonly ILogger _logger;

		public AccountServicesCache(ILanguageCloudService languageCloudService, ILogger logger)
		{
			_languageCloudService = languageCloudService;
			_logger = logger;
		}

		public async Task<DetailedProject> GetProject(string accountId, string projectId)
		{
			return await CreateService(accountId).GetProjectAsync(projectId).ConfigureAwait(continueOnCapturedContext: false);
		}

		public List<Project> GetProjectsInParallel(List<IAccount> accounts)
		{
			List<Project> projectList = new List<Project>();
			Parallel.ForEach(accounts, delegate(IAccount account)
			{
				try
				{
					projectList.AddRange(CreateService(account.Id).ProjectsAsync().ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
						.GetResult()
						.Items);
					}
					catch (Exception exception)
					{
						throw new CloudProjectsLoadException(account.Name, exception);
					}
				});
				return projectList;
			}

			public List<Project> GetProjects(Dictionary<string, string[]> accountsAndProjectIds)
			{
				List<Task> list = new List<Task>();
				List<Project> projectList = new List<Project>();
				try
				{
					foreach (string accountId in accountsAndProjectIds.Keys)
					{
						list.Add(Task.Run(delegate
						{
							if (accountsAndProjectIds[accountId].Length != 0)
							{
								projectList.AddRange(CreateService(accountId).GetProjects(accountsAndProjectIds[accountId]).Result.Items);
							}
						}));
					}
					Task.WaitAll(list.ToArray());
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(_logger, ex, "Failed to retrive completed projects in Cloud", Array.Empty<object>());
				}
				return projectList;
			}

			protected virtual IStudioBFFService CreateService(string accountId)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				return (IStudioBFFService)(object)_languageCloudService.Build<StudioBFFService>((AuthorizationHandler)2, new MultiTenantInfo(_languageCloudService.ApiContext, accountId), (Func<string>)null);
			}
		}
	}
