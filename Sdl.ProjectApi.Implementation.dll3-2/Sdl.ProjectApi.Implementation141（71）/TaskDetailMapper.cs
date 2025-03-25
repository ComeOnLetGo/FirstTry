using System.Collections.Generic;
using System.Text;

namespace Sdl.ProjectApi.Implementation
{
	public static class TaskDetailMapper
	{
		public static string GetTaskTemplateDescriptions(ITaskTemplate[] templates)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < templates.Length; i++)
			{
				if (templates[i] != null)
				{
					stringBuilder.Append(templates[i].Description);
					if (i < templates.Length - 1)
					{
						stringBuilder.Append('+');
					}
				}
			}
			return stringBuilder.ToString();
		}

		public static string GetTaskTemplateNames(ITaskTemplate[] templates, List<string> templateIds)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < templates.Length; i++)
			{
				if (templates[i] == null)
				{
					if (templateIds != null && templateIds.Count > i)
					{
						stringBuilder.Append(templateIds[i]);
					}
					else
					{
						stringBuilder.Append("");
					}
				}
				else
				{
					stringBuilder.Append(templates[i].Name);
				}
				if (i < templates.Length - 1)
				{
					stringBuilder.Append('+');
				}
			}
			return stringBuilder.ToString();
		}
	}
}
