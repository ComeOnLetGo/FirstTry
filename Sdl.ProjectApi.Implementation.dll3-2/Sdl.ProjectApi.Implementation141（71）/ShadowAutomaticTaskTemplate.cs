using System;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TaskImplementation;

namespace Sdl.ProjectApi.Implementation
{
	public class ShadowAutomaticTaskTemplate : IAutomaticTaskTemplate, ITaskTemplate
	{
		public string Id => XmlAutomaticTaskTemplate.Id;

		public string Name
		{
			get
			{
				return XmlAutomaticTaskTemplate.Name;
			}
			set
			{
				XmlAutomaticTaskTemplate.Name = value;
			}
		}

		public string Description
		{
			get
			{
				return XmlAutomaticTaskTemplate.Description;
			}
			set
			{
				XmlAutomaticTaskTemplate.Description = value;
			}
		}

		public AutomaticTaskType TaskType { get; set; }

		public bool AllowMultiThreading => false;

		public TaskFileType[] SupportedFileTypes => (TaskFileType[])(object)new TaskFileType[0];

		public bool AllowMultiple => false;

		public bool RestrictedForTaskSequence => true;

		public TaskFileType GeneratedFileType => (TaskFileType)0;

		public string[] RequiredSettingsPageIds => new string[0];

		public Type[] RequiresBatchTaskSettingsPageTypes => new Type[0];

		internal Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate XmlAutomaticTaskTemplate { get; }

		public bool IsLocallyExecutable => false;

		internal ShadowAutomaticTaskTemplate(Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate xmlAutomaticTaskTemplate)
		{
			XmlAutomaticTaskTemplate = xmlAutomaticTaskTemplate;
		}

		public IAbstractTaskImplementation CreateImplementation()
		{
			throw new NotImplementedException();
		}

		public bool SupportsFileType(TaskFileType type)
		{
			return false;
		}

		public bool SupportsFileTypes(TaskFileType[] types)
		{
			return false;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
