using System;
using Sdl.ProjectApi.TaskImplementation;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal static class TaskImplementationFactory
	{
		public static ISimpleTaskImplementation CreateTaskImplementation(IExecutingAutomaticTask task)
		{
			if (IsSimpleTask(task))
			{
				return CreateSimpleTaskImplementation(task);
			}
			if (IsContentProcessingTask(task))
			{
				return CreateContentProcessingTaskImplementation(task);
			}
			throw new ArgumentException("The specified task cannot be processed, since it is not a simple or a content processing task.");
		}

		private static bool IsSimpleTask(IExecutingAutomaticTask task)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			ITaskTemplate[] taskTemplates = ((ITaskBase)task).TaskTemplates;
			if (taskTemplates.Length != 1)
			{
				return false;
			}
			ITaskTemplate obj = taskTemplates[0];
			IAutomaticTaskTemplate val = (IAutomaticTaskTemplate)(object)((obj is IAutomaticTaskTemplate) ? obj : null);
			if (val != null)
			{
				return (int)val.TaskType == 0;
			}
			return false;
		}

		private static bool IsContentProcessingTask(IExecutingAutomaticTask task)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Invalid comparison between Unknown and I4
			ITaskTemplate[] taskTemplates = ((ITaskBase)task).TaskTemplates;
			ITaskTemplate[] array = taskTemplates;
			foreach (ITaskTemplate val in array)
			{
				IAutomaticTaskTemplate val2 = (IAutomaticTaskTemplate)(object)((val is IAutomaticTaskTemplate) ? val : null);
				if (val2 == null || (int)val2.TaskType != 1)
				{
					return false;
				}
			}
			return true;
		}

		private static ISimpleTaskImplementation CreateSimpleTaskImplementation(IExecutingAutomaticTask task)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			IAutomaticTaskTemplate val = (IAutomaticTaskTemplate)((ITaskBase)task).TaskTemplates[0];
			IAbstractTaskImplementation val2 = val.CreateImplementation();
			ISimpleTaskImplementation val3 = (ISimpleTaskImplementation)(object)((val2 is ISimpleTaskImplementation) ? val2 : null);
			if (val3 == null)
			{
				throw new ProjectApiException($"Task template '{((ITaskTemplate)val).Name}' does not implement ISimpleTaskImplementation.");
			}
			try
			{
				((IAbstractTaskImplementation)val3).InitializeTask(task);
				return val3;
			}
			catch (Exception ex)
			{
				throw new ProjectApiException("Unexpected exception when initioalizing task.", ex);
			}
		}

		private static ISimpleTaskImplementation CreateContentProcessingTaskImplementation(IExecutingAutomaticTask task)
		{
			ContentProcessingTaskImplementation contentProcessingTaskImplementation = new ContentProcessingTaskImplementation();
			contentProcessingTaskImplementation.InitializeTask(task);
			return (ISimpleTaskImplementation)(object)contentProcessingTaskImplementation;
		}
	}
}
