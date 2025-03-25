using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sdl.ProjectApi.Reporting;

namespace Sdl.ProjectApi.Implementation
{
	internal class TaskReportTypeManager : ITaskReportTypeManager
	{
		private static readonly Dictionary<string, ReportDefinition> _reportTypeDefinitionCache = new Dictionary<string, ReportDefinition>();

		public Assembly GetReportAssembly(ITaskTemplate taskTemplate)
		{
			AutomaticTaskTemplate automaticTaskTemplate = taskTemplate as AutomaticTaskTemplate;
			Type extensionType = automaticTaskTemplate.Extension.ExtensionType;
			return extensionType.Assembly;
		}

		public ReportDefinition GetReportTypeDefinition(ITaskTemplate taskTemplate)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Expected O, but got Unknown
			if (taskTemplate == null)
			{
				throw new ProjectApiException(ErrorMessages.TaskReportTypeManager_TaskTempateDoesNotExist);
			}
			AutomaticTaskTemplate automaticTaskTemplate = taskTemplate as AutomaticTaskTemplate;
			if (automaticTaskTemplate == null)
			{
				throw new ProjectApiException(ErrorMessages.TaskReportTypeManager_TaskTemplateIsNotAutomatic);
			}
			string id = automaticTaskTemplate.Id;
			if (_reportTypeDefinitionCache.TryGetValue(id, out var value))
			{
				return value;
			}
			Type extensionType = automaticTaskTemplate.Extension.ExtensionType;
			Assembly assembly = extensionType.Assembly;
			string[] manifestResourceNames = assembly.GetManifestResourceNames();
			string text = ((!string.IsNullOrWhiteSpace(automaticTaskTemplate.ExtensionAttribute.ReportRendererTemplateName)) ? manifestResourceNames.FirstOrDefault((string resourceName) => resourceName.Contains("." + automaticTaskTemplate.ExtensionAttribute.ReportRendererTemplateName)) : manifestResourceNames.FirstOrDefault((string resourceName) => resourceName.EndsWith(".xsl")));
			if (text != null)
			{
				using (Stream stream = assembly.GetManifestResourceStream(text))
				{
					value = new ReportDefinition
					{
						Uri = assembly.GetName().FullName
					};
					int num = (int)stream.Length;
					byte[] array = new byte[num];
					stream.Read(array, 0, num);
					value.Data = array;
					return value;
				}
			}
			throw new ProjectApiException(string.Format(ErrorMessages.TaskReportTypeManager_NoReportDefinitionForTaskTemplate, automaticTaskTemplate.Name));
		}
	}
}
