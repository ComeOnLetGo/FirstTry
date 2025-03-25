using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal static class BatchTaskAdapterFactory
	{
		public static ExecutingAutomaticTask ToExecutingBatchTask(IExecutingAutomaticTask task)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			return new ExecutingAutomaticTask(task);
		}

		public static ExecutingAutomaticTaskFile ToExecutingTaskFile(IExecutingTaskFile file)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			return new ExecutingAutomaticTaskFile(file);
		}
	}
}
