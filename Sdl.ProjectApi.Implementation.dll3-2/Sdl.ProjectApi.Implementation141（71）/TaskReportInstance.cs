using System;
using Sdl.ProjectApi.Reporting;

namespace Sdl.ProjectApi.Implementation
{
	internal class TaskReportInstance : ITaskReportInstance
	{
		private readonly Report _report;

		public Guid TaskId
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				TaskId id = ((ITaskBase)_report.Task).Id;
				return ((TaskId)(ref id)).ToGuidArray()[0];
			}
		}

		public ITaskTemplate TaskTemplate => _report.TaskTemplate;

		public string Xml => _report.GetReportXml();

		internal TaskReportInstance(Report report)
		{
			_report = report;
		}
	}
}
