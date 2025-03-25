using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ManualTaskFile : TaskFile, IManualTaskFile, ITaskFile, ITaskEntity
	{
		private readonly ManualTask _manualTask;

		public ManualTaskFile(ManualTask task, Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile)
			: base(task, xmlTaskFile)
		{
			_manualTask = task;
		}

		public void Complete()
		{
			SetCompleted();
			if (!_manualTask.XmlTask.Files.Exists((Sdl.ProjectApi.Implementation.Xml.TaskFile xmlTaskFile) => xmlTaskFile.Purpose == TaskFilePurpose.WorkFile && !xmlTaskFile.Completed))
			{
				_manualTask.SetCompleted();
			}
		}
	}
}
