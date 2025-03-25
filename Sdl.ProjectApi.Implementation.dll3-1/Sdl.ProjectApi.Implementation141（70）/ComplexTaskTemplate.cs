using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ComplexTaskTemplate : IComplexTaskTemplate, ITaskTemplate
	{
		private readonly IWorkflow _workflow;

		private ITaskTemplate[] _lazySubTaskTemplates;

		public string Id => XmlComplexTaskTemplate.Id;

		public string Name
		{
			get
			{
				string text = "";
				if (!string.IsNullOrEmpty(XmlComplexTaskTemplate.Name) && XmlComplexTaskTemplate.Name.StartsWith("ComplexTask_"))
				{
					try
					{
						text = StringResources.ResourceManager.GetString(XmlComplexTaskTemplate.Name);
					}
					catch
					{
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return XmlComplexTaskTemplate.Name;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("Name");
				}
				XmlComplexTaskTemplate.Name = value;
			}
		}

		public string Description
		{
			get
			{
				string text = "";
				if (!string.IsNullOrEmpty(XmlComplexTaskTemplate.Description) && XmlComplexTaskTemplate.Description.StartsWith("ComplexTask_"))
				{
					try
					{
						text = StringResources.ResourceManager.GetString(XmlComplexTaskTemplate.Description);
					}
					catch
					{
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return XmlComplexTaskTemplate.Description;
			}
			set
			{
				XmlComplexTaskTemplate.Description = value;
			}
		}

		public TaskFileType[] SupportedFileTypes
		{
			get
			{
				ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById(XmlComplexTaskTemplate.SubTaskTemplates[0].TaskTemplateId);
				return taskTemplateById.SupportedFileTypes;
			}
		}

		public string[] RequiredSettingsPageIds
		{
			get
			{
				HashSet<string> hashSet = new HashSet<string>();
				ITaskTemplate[] subTaskTemplates = SubTaskTemplates;
				foreach (ITaskTemplate val in subTaskTemplates)
				{
					string[] requiredSettingsPageIds = val.RequiredSettingsPageIds;
					foreach (string item in requiredSettingsPageIds)
					{
						hashSet.Add(item);
					}
				}
				string[] array = new string[hashSet.Count];
				hashSet.CopyTo(array);
				return array;
			}
		}

		public Type[] RequiresBatchTaskSettingsPageTypes => (from type in SubTaskTemplates.SelectMany((ITaskTemplate template) => template.RequiresBatchTaskSettingsPageTypes)
			where type != null
			select type).ToArray();

		public TaskFileType GeneratedFileType
		{
			get
			{
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				for (int num = XmlComplexTaskTemplate.SubTaskTemplates.Count - 1; num >= 0; num--)
				{
					ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById(XmlComplexTaskTemplate.SubTaskTemplates[num].TaskTemplateId);
					if ((int)taskTemplateById.GeneratedFileType != 0)
					{
						return taskTemplateById.GeneratedFileType;
					}
				}
				return (TaskFileType)0;
			}
		}

		public bool AllowMultiple
		{
			get
			{
				ITaskTemplate[] subTaskTemplates = SubTaskTemplates;
				foreach (ITaskTemplate val in subTaskTemplates)
				{
					if (!val.AllowMultiple)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool RestrictedForTaskSequence
		{
			get
			{
				ITaskTemplate[] subTaskTemplates = SubTaskTemplates;
				foreach (ITaskTemplate val in subTaskTemplates)
				{
					if (val.RestrictedForTaskSequence)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ITaskTemplate[] SubTaskTemplates
		{
			get
			{
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				if (_lazySubTaskTemplates == null)
				{
					_lazySubTaskTemplates = (ITaskTemplate[])(object)new ITaskTemplate[XmlComplexTaskTemplate.SubTaskTemplates.Count];
					for (int i = 0; i < _lazySubTaskTemplates.Length; i++)
					{
						string taskTemplateId = XmlComplexTaskTemplate.SubTaskTemplates[i].TaskTemplateId;
						ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById(taskTemplateId);
						_lazySubTaskTemplates[i] = taskTemplateById ?? throw new ProjectApiException($"Complex task refers to sub task '{taskTemplateId}' that cannot be found.");
					}
				}
				return _lazySubTaskTemplates;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					throw new ArgumentNullException("SubTaskTemplates");
				}
				_lazySubTaskTemplates = null;
				XmlComplexTaskTemplate.SubTaskTemplates.Clear();
				foreach (ITaskTemplate val in value)
				{
					if (val == null)
					{
						throw new ArgumentNullException("SubTaskTemplates", "One of the sub-tasktemplates is null.");
					}
					SubTaskTemplate item = new SubTaskTemplate
					{
						TaskTemplateId = val.Id
					};
					XmlComplexTaskTemplate.SubTaskTemplates.Add(item);
				}
			}
		}

		internal Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate XmlComplexTaskTemplate { get; }

		public bool IsLocallyExecutable => true;

		internal ComplexTaskTemplate(IWorkflow workflow, Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate xmlComplexTaskTemplate)
		{
			_workflow = workflow;
			XmlComplexTaskTemplate = xmlComplexTaskTemplate;
		}

		public bool SupportsFileType(TaskFileType taskFileType)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			ITaskTemplate taskTemplateById = _workflow.GetTaskTemplateById(XmlComplexTaskTemplate.SubTaskTemplates[0].TaskTemplateId);
			if (taskTemplateById != null)
			{
				return taskTemplateById.SupportsFileType(taskFileType);
			}
			return false;
		}

		public virtual bool SupportsFileTypes(TaskFileType[] types)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			foreach (TaskFileType taskFileType in types)
			{
				if (!SupportsFileType(taskFileType))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
