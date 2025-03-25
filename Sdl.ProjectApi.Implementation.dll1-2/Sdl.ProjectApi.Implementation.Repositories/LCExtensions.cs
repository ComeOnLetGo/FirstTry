using System;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public static class LCExtensions
	{
		public static TaskStatus GetTaskStatus(this Task task)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			FileTaskStatus status = task.Status;
			if (Enum.TryParse<TaskStatus>(((object)(FileTaskStatus)(ref status)).ToString(), out var result))
			{
				return result;
			}
			return TaskStatus.Created;
		}

		public static CountData GetCountData(this AnalysisStatistic statistic)
		{
			return new CountData
			{
				Characters = statistic.Characters,
				Placeables = statistic.Placeables,
				Segments = statistic.Segments,
				Tags = statistic.Tags,
				Words = statistic.Words
			};
		}

		public static FileRole MapFileRole(this LightFile file)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			FileRole role = file.Role;
			if (Enum.TryParse<FileRole>(((object)(FileRole)(ref role)).ToString(), out var result))
			{
				return result;
			}
			return FileRole.Unknown;
		}

		public static ProjectStatus MapProjectStatus(this DetailedProject detailedProject)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected I4, but got Unknown
			ProjectStatus result = ProjectStatus.Pending;
			ProjectStatus status = ((Project)detailedProject).Status;
			switch ((int)status)
			{
			case 0:
				result = ProjectStatus.Started;
				break;
			case 2:
				result = ProjectStatus.Completed;
				break;
			case 1:
				result = ProjectStatus.Archived;
				break;
			}
			return result;
		}
	}
}
