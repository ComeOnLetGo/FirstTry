using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class TaskFileFactory
	{
		public ITaskFile CreateTaskFile(ScheduledTask task, Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile)
		{
			if (task is ManualTask task2)
			{
				return (ITaskFile)(object)new ManualTaskFile(task2, xmlTaskFile);
			}
			return (ITaskFile)(object)new TaskFile(task, xmlTaskFile);
		}
	}
}
