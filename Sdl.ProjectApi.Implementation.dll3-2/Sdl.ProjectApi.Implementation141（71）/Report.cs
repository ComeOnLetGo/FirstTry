using System;
using System.IO;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Reporting;

namespace Sdl.ProjectApi.Implementation
{
	public class Report : IReport
	{
		private readonly ITaskReportRenderingEngine _reportRenderingEngine;

		private readonly TaskReport _xmlTaskReport;

		private readonly IReportRepository _reportRepository;

		private readonly IProject _project;

		public Guid Guid => _xmlTaskReport.Guid;

		public string Name
		{
			get
			{
				return _xmlTaskReport.Name;
			}
			set
			{
				_xmlTaskReport.Name = value;
			}
		}

		public string Description
		{
			get
			{
				return _xmlTaskReport.Description;
			}
			set
			{
				_xmlTaskReport.Description = value;
			}
		}

		public ITaskTemplate TaskTemplate => ((IProjectConfiguration)_project).ProjectsProvider.Workflow.GetTaskTemplateById(_xmlTaskReport.TaskTemplateId);

		public ILanguageDirection LanguageDirection
		{
			get
			{
				if (!(_xmlTaskReport.LanguageDirectionGuid != Guid.Empty))
				{
					return null;
				}
				return ((IProjectConfiguration)_project).LanguageDirections.FirstOrDefault((ILanguageDirection ld) => ld.Guid == _xmlTaskReport.LanguageDirectionGuid);
			}
		}

		public ReportFormat[] AvailableFormats
		{
			get
			{
				ReportFormat[] array = _reportRenderingEngine.ReportFormats;
				if (TaskTemplate == null)
				{
					array = array.Where((ReportFormat x) => x.Name == "XML").ToArray();
				}
				return array;
			}
		}

		public string PhysicalPath => _xmlTaskReport.PhysicalPath;

		public bool IsCustomReport => _xmlTaskReport.IsCustomReport;

		public IScheduledTask Task { get; private set; }

		internal Report(ScheduledTask task, ITaskReportRenderingEngine reportRenderingEngine, TaskReport xmlTaskReport, IReportRepository reportRepository)
		{
			_xmlTaskReport = xmlTaskReport;
			_reportRepository = reportRepository;
			Task = (IScheduledTask)(object)task;
			_project = task.Project;
			_reportRenderingEngine = reportRenderingEngine;
		}

		public void SaveAs(string targetFilePath, ReportFormat format)
		{
			byte[] array = RenderReport(format);
			_reportRepository.SaveAs(targetFilePath, array);
		}

		public byte[] RenderReport(ReportFormat format)
		{
			return _reportRenderingEngine.RenderReport((ITaskReportInstance)(object)new TaskReportInstance(this), format);
		}

		public byte[] RenderReport(string customTemplatePath, ReportFormat format)
		{
			return _reportRenderingEngine.RenderReport((ITaskReportInstance)(object)new TaskReportInstance(this), _reportRepository.GetCustomTemplate(customTemplatePath), format);
		}

		public void Delete()
		{
			(Task as ScheduledTask).RemoveReport(this);
			File.Delete(GetAbsoluteReportFileName());
		}

		private string GetAbsoluteReportFileName()
		{
			return Path.Combine(_project.LocalDataFolder, _xmlTaskReport.PhysicalPath);
		}

		public void SetReportXml(string reportXml)
		{
			_reportRepository.SetReportXml(reportXml);
		}

		internal string GetReportXml()
		{
			return _reportRepository.GetReportXml();
		}
	}
}
