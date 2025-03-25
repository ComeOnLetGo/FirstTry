namespace Sdl.ProjectApi.Implementation
{
	internal class IncrementalStatus
	{
		private TaskStatus _status = (TaskStatus)9;

		private int _totalPercentComplete;

		private int _taskCount;

		public TaskStatus Status => _status;

		public byte PercentComplete
		{
			get
			{
				if (_taskCount == 0)
				{
					return 100;
				}
				return (byte)(_totalPercentComplete / _taskCount);
			}
		}

		public TaskStatus TaskStatus => _status;

		public IncrementalStatus()
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			_status = GetStatus(0);
		}

		public IncrementalStatus(TaskStatus status)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			_status = status;
		}

		public void Increment(TaskStatus status, int percentComplete)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (GetSeverity(status) > GetSeverity(_status))
			{
				_status = status;
			}
			_taskCount++;
			_totalPercentComplete += percentComplete;
		}

		private static int GetSeverity(TaskStatus status)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected I4, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			return (status - 1) switch
			{
				8 => 0, 
				1 => 1, 
				0 => 2, 
				2 => 3, 
				3 => 5, 
				7 => 6, 
				4 => 7, 
				5 => 8, 
				6 => 10, 
				_ => throw new ProjectApiException("Invalid status value: " + ((object)(TaskStatus)(ref status)).ToString()), 
			};
		}

		private static TaskStatus GetStatus(int severity)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			return (TaskStatus)(severity switch
			{
				0 => 9, 
				1 => 2, 
				2 => 1, 
				3 => 3, 
				5 => 4, 
				6 => 8, 
				7 => 5, 
				8 => 6, 
				10 => 7, 
				_ => throw new ProjectApiException("Invalid severity value: " + severity), 
			});
		}
	}
}
