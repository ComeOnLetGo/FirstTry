using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal class ContentProcessingTaskImplementationAdapter : IContentProcessingTaskImplementation, IAbstractTaskImplementation
	{
		private readonly AbstractFileContentProcessingAutomaticTask _implementation;

		public bool ShouldRunOnMultipleThreads => false;

		public ContentProcessingTaskImplementationAdapter(AbstractFileContentProcessingAutomaticTask implementation)
		{
			_implementation = implementation;
		}

		public void InitializeTask(IExecutingAutomaticTask task)
		{
			_implementation.InitializeTask(BatchTaskAdapterFactory.ToExecutingBatchTask(task));
		}

		public void TaskComplete()
		{
			_implementation.TaskComplete();
		}

		public void Cancel(bool doRollback)
		{
			_implementation.Cancel(doRollback);
		}

		public bool ShouldProcessFile(IExecutingTaskFile executingTaskFile)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			return _implementation.ShouldProcessFile(new ExecutingAutomaticTaskFile(executingTaskFile));
		}

		public void ConfigureConverter(IExecutingTaskFile executingTaskFile, IMultiFileConverter multiFileConverter)
		{
			_implementation.ConfigureConverter(BatchTaskAdapterFactory.ToExecutingTaskFile(executingTaskFile), multiFileConverter);
		}

		public FileContentProcessingResult FileComplete(IExecutingTaskFile executingTaskFile, IMultiFileConverter multiFileConverter)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			bool flag = _implementation.FileComplete(BatchTaskAdapterFactory.ToExecutingTaskFile(executingTaskFile), multiFileConverter);
			return new FileContentProcessingResult(flag);
		}
	}
}
