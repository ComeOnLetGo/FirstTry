using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.LanguageCloud.Builders
{
	public class TaskBuilderBase
	{
		protected void AddOrUpdateTaskFile(Task xmlTask, Task task)
		{
			if (xmlTask.Files == null)
			{
				xmlTask.Files = new List<Sdl.ProjectApi.Implementation.Xml.TaskFile>();
			}
			if (task.Files.Any())
			{
				Sdl.ProjectApi.Implementation.Xml.TaskFile taskFile = xmlTask.Files.Where((Sdl.ProjectApi.Implementation.Xml.TaskFile f) => f.LanguageFileGuid == Guid.Parse(task.Files.FirstOrDefault().Id)).FirstOrDefault();
				if (taskFile == null)
				{
					taskFile = new Sdl.ProjectApi.Implementation.Xml.TaskFile
					{
						Guid = Guid.NewGuid(),
						LanguageFileGuid = Guid.Parse(task.Files.FirstOrDefault().Id),
						Completed = (xmlTask.Status == TaskStatus.Completed),
						Purpose = TaskFilePurpose.WorkFile
					};
					xmlTask.Files.Add(taskFile);
				}
				else
				{
					taskFile.Completed = xmlTask.Status == TaskStatus.Completed;
				}
			}
		}
	}
}
