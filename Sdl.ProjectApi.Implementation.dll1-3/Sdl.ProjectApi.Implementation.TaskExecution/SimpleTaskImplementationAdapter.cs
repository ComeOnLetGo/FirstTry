using System;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal class SimpleTaskImplementationAdapter : ISimpleTaskImplementation, IAbstractTaskImplementation
	{
		private readonly AbstractFileLevelAutomaticTask _implementation;

		public event EventHandler<TaskProgressEventArgs> Progress;

		public SimpleTaskImplementationAdapter(AbstractFileLevelAutomaticTask implementation)
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

		public void Execute()
		{
			_implementation.Execute();
		}

		public void Dispose()
		{
		}
	}
}
