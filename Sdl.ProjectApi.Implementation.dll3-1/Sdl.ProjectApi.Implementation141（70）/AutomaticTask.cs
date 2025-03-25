using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class AutomaticTask : ScheduledTask, IAutomaticCollaborativeTask, IAutomaticTask, IScheduledTask, ITaskBase, ICollaborativeTask<ITaskFile>
	{
		private AutomaticTaskExecuter _currentAutomaticTaskExecuter;

		public string CurrentOperationDescription { get; private set; }

		internal AutomaticTask(IProject project, Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlTask, TaskFileFactory taskFileBuilder, IProjectPathUtil projectPathUtil)
			: base(project, xmlTask, taskFileBuilder, projectPathUtil)
		{
		}

		public override void Cancel()
		{
			if (_currentAutomaticTaskExecuter != null)
			{
				_currentAutomaticTaskExecuter.Cancel();
				_currentAutomaticTaskExecuter = null;
			}
		}

		public void Start()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if (!string.IsNullOrEmpty(base.XmlTask.ComplexTaskTemplateId))
			{
				throw new ProjectApiException(ErrorMessages.AutomaticTask_CannotStartSubTask);
			}
			TaskId id = base.Id;
			Guid[] array = ((TaskId)(ref id)).ToGuidArray();
			AutomaticTask automaticTask = base.Project.GetTask(new TaskId(array[0])) as AutomaticTask;
			using LanguageObjectsCache objectsCache = new LanguageObjectsCache();
			ExecutingAutomaticTask executingAutomaticTask = new ExecutingAutomaticTask(base.Project, automaticTask, (ILanguageObjectsCache)(object)objectsCache);
			_currentAutomaticTaskExecuter = new AutomaticTaskExecuter(new ExecutingAutomaticTask[1] { executingAutomaticTask });
			_currentAutomaticTaskExecuter.Execute();
			_currentAutomaticTaskExecuter = null;
		}

		internal void SetPercentComplete(int percentComplete, string currentOperationDescription)
		{
			CurrentOperationDescription = currentOperationDescription;
			base.XmlTask.PercentComplete = percentComplete;
			OnStatusChanged();
		}

		protected override void OnStatusChanged()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)base.Status != 3)
			{
				CurrentOperationDescription = null;
			}
			base.OnStatusChanged();
		}
	}
}
