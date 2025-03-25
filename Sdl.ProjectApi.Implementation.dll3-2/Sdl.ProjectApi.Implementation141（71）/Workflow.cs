using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Sdl.Core.PluginFramework;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation
{
	public class Workflow : IWorkflow
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.Workflow _xmlWorkflow;

		private readonly IPluginRegistry _pluginRegistry;

		private IAutomaticTaskTemplate[] _lazyAutomaticTaskTemplates;

		private IComplexTaskTemplate[] _lazyComplexTaskTemplates;

		private IManualTaskTemplate[] _lazyManualTaskTemplates;

		public IAutomaticTaskTemplate[] AutomaticTaskTemplates
		{
			get
			{
				if (_lazyAutomaticTaskTemplates == null)
				{
					List<IAutomaticTaskTemplate> list = new List<IAutomaticTaskTemplate>();
					IExtensionPoint extensionPoint = _pluginRegistry.GetExtensionPoint<AutomaticTaskAttribute>();
					foreach (IExtension item in (ReadOnlyCollection<IExtension>)(object)extensionPoint.Extensions)
					{
						list.Add((IAutomaticTaskTemplate)(object)new AutomaticTaskTemplate(item));
					}
					List<Assembly> list2 = new List<Assembly>();
					IExtensionPoint extensionPoint2 = _pluginRegistry.GetExtensionPoint<AutomaticTaskAttribute>();
					foreach (IExtension item2 in (ReadOnlyCollection<IExtension>)(object)extensionPoint2.Extensions)
					{
						list.Add((IAutomaticTaskTemplate)(object)new AutomaticTaskTemplate(item2));
						list2.Add(item2.ExtensionType.Assembly);
					}
					ProjectAutomationTaskAssemblies.Assemblies = list2.ToArray();
					if (_xmlWorkflow.AutomaticTaskTemplates != null)
					{
						foreach (Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate automaticTaskTemplate in _xmlWorkflow.AutomaticTaskTemplates)
						{
							list.Add((IAutomaticTaskTemplate)(object)new ShadowAutomaticTaskTemplate(automaticTaskTemplate));
						}
					}
					_lazyAutomaticTaskTemplates = list.ToArray();
				}
				return _lazyAutomaticTaskTemplates;
			}
		}

		public IComplexTaskTemplate[] ComplexTaskTemplates
		{
			get
			{
				if (_lazyComplexTaskTemplates == null)
				{
					_lazyComplexTaskTemplates = (IComplexTaskTemplate[])(object)new IComplexTaskTemplate[_xmlWorkflow.ComplexTaskTemplates.Count];
					for (int i = 0; i < _lazyComplexTaskTemplates.Length; i++)
					{
						_lazyComplexTaskTemplates[i] = (IComplexTaskTemplate)(object)new ComplexTaskTemplate((IWorkflow)(object)this, _xmlWorkflow.ComplexTaskTemplates[i]);
					}
				}
				return _lazyComplexTaskTemplates;
			}
		}

		public IManualTaskTemplate[] ManualTaskTemplates
		{
			get
			{
				if (_lazyManualTaskTemplates == null)
				{
					_lazyManualTaskTemplates = (IManualTaskTemplate[])(object)new IManualTaskTemplate[_xmlWorkflow.ManualTaskTemplates.Count];
					for (int i = 0; i < _lazyManualTaskTemplates.Length; i++)
					{
						_lazyManualTaskTemplates[i] = (IManualTaskTemplate)(object)new ManualTaskTemplate(_xmlWorkflow.ManualTaskTemplates[i]);
					}
				}
				return _lazyManualTaskTemplates;
			}
		}

		public Workflow(Sdl.ProjectApi.Implementation.Xml.Workflow xmlWorkflow, IPluginRegistry pluginRegistry)
		{
			_xmlWorkflow = xmlWorkflow;
			_pluginRegistry = pluginRegistry;
		}

		internal IAutomaticTaskTemplate AddAutomaticTaskTemplate(AutomaticTaskDescriptor automaticTaskDescriptor)
		{
			IAutomaticTaskTemplate[] collection = _lazyAutomaticTaskTemplates ?? AutomaticTaskTemplates;
			List<IAutomaticTaskTemplate> list = new List<IAutomaticTaskTemplate>(collection);
			AutomaticTaskTemplate automaticTaskTemplate = new AutomaticTaskTemplate(automaticTaskDescriptor);
			list.Add((IAutomaticTaskTemplate)(object)automaticTaskTemplate);
			_lazyAutomaticTaskTemplates = list.ToArray();
			return (IAutomaticTaskTemplate)(object)automaticTaskTemplate;
		}

		public List<ITaskTemplate> GetAllTaskTemplates()
		{
			List<ITaskTemplate> list = new List<ITaskTemplate>();
			list.AddRange((IEnumerable<ITaskTemplate>)(object)ComplexTaskTemplates);
			list.AddRange((IEnumerable<ITaskTemplate>)(object)AutomaticTaskTemplates);
			list.AddRange((IEnumerable<ITaskTemplate>)(object)ManualTaskTemplates);
			return list;
		}

		public ITaskTemplate GetTaskTemplateById(string id)
		{
			IComplexTaskTemplate[] complexTaskTemplates = ComplexTaskTemplates;
			for (int i = 0; i < complexTaskTemplates.Length; i++)
			{
				ComplexTaskTemplate complexTaskTemplate = (ComplexTaskTemplate)(object)complexTaskTemplates[i];
				if (complexTaskTemplate.Id == id)
				{
					return (ITaskTemplate)(object)complexTaskTemplate;
				}
			}
			IAutomaticTaskTemplate[] automaticTaskTemplates = AutomaticTaskTemplates;
			foreach (IAutomaticTaskTemplate val in automaticTaskTemplates)
			{
				if (((ITaskTemplate)val).Id == id)
				{
					return (ITaskTemplate)(object)val;
				}
			}
			IManualTaskTemplate[] manualTaskTemplates = ManualTaskTemplates;
			foreach (IManualTaskTemplate val2 in manualTaskTemplates)
			{
				if (((ITaskTemplate)val2).Id == id)
				{
					return (ITaskTemplate)(object)val2;
				}
			}
			return null;
		}

		public ITaskTemplate GetTaskTemplateByName(string name)
		{
			IComplexTaskTemplate[] complexTaskTemplates = ComplexTaskTemplates;
			for (int i = 0; i < complexTaskTemplates.Length; i++)
			{
				ComplexTaskTemplate complexTaskTemplate = (ComplexTaskTemplate)(object)complexTaskTemplates[i];
				if (complexTaskTemplate.Name == name)
				{
					return (ITaskTemplate)(object)complexTaskTemplate;
				}
			}
			IAutomaticTaskTemplate[] automaticTaskTemplates = AutomaticTaskTemplates;
			foreach (IAutomaticTaskTemplate val in automaticTaskTemplates)
			{
				if (((ITaskTemplate)val).Name == name)
				{
					return (ITaskTemplate)(object)val;
				}
			}
			IManualTaskTemplate[] manualTaskTemplates = ManualTaskTemplates;
			foreach (IManualTaskTemplate val2 in manualTaskTemplates)
			{
				if (((ITaskTemplate)val2).Name == name)
				{
					return (ITaskTemplate)(object)val2;
				}
			}
			return null;
		}

		public IComplexTaskTemplate CreateComplexTaskTemplate(string name, string description, ITaskTemplate[] templates)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (templates == null || templates.Length == 0)
			{
				throw new ArgumentNullException("templates");
			}
			Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate complexTaskTemplate = new Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate();
			complexTaskTemplate.Id = Guid.NewGuid().ToString();
			complexTaskTemplate.Name = name;
			complexTaskTemplate.Description = description;
			foreach (ITaskTemplate val in templates)
			{
				if (val == null)
				{
					throw new ArgumentNullException("templates", "The specified array of task templates contains null values.");
				}
				if (GetTaskTemplateById(val.Id) == null)
				{
					throw new ArgumentException("templates", "The specified array of task templates contains templates that are not defined in this workflow.");
				}
				complexTaskTemplate.SubTaskTemplates.Add(new SubTaskTemplate(val.Id));
			}
			return (IComplexTaskTemplate)(object)new ComplexTaskTemplate((IWorkflow)(object)this, complexTaskTemplate);
		}

		public void AddComplexTaskTemplate(IComplexTaskTemplate template)
		{
			if (!(template is ComplexTaskTemplate complexTaskTemplate))
			{
				throw new ArgumentException("You can only add templates created using this workflow instance.");
			}
			int num = _xmlWorkflow.ComplexTaskTemplates.FindIndex((Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate xmlTemplate) => xmlTemplate.Id == ((ITaskTemplate)template).Id);
			if (num != -1)
			{
				throw new ArgumentException("The specified complex task template is already part of this workflow.");
			}
			_xmlWorkflow.ComplexTaskTemplates.Add(complexTaskTemplate.XmlComplexTaskTemplate);
			_lazyComplexTaskTemplates = null;
		}

		public void RemoveComplexTaskTemplate(IComplexTaskTemplate template)
		{
			if (template == null)
			{
				throw new ArgumentNullException("template");
			}
			int num = _xmlWorkflow.ComplexTaskTemplates.FindIndex((Sdl.ProjectApi.Implementation.Xml.ComplexTaskTemplate xmlTemplate) => xmlTemplate.Id == ((ITaskTemplate)template).Id);
			if (num != -1)
			{
				_xmlWorkflow.ComplexTaskTemplates.RemoveAt(num);
				_lazyComplexTaskTemplates = null;
			}
		}

		public IManualTaskTemplate AddManualTaskTemplate(string id, string name, string description)
		{
			return AddManualTaskTemplate(id, name, description, (WorkflowStage)5, isImported: false, isLocallyExecutable: true);
		}

		public IManualTaskTemplate AddManualTaskTemplate(string id, string name, string description, bool isImported, bool isLocallyExecutable)
		{
			return AddManualTaskTemplate(id, name, description, (WorkflowStage)5, isImported, isLocallyExecutable);
		}

		public IManualTaskTemplate AddManualTaskTemplate(string id, string name, string description, WorkflowStage workflowStage, bool isImported, bool isLocallyExecutable)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate xmlManualTaskTemplate = new Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate
			{
				Id = id,
				Name = name,
				Description = description,
				WorkflowStageType = EnumConvert.ConvertWorkflowStageType(workflowStage),
				IsImported = isImported,
				IsLocallyExecutable = isLocallyExecutable
			};
			IManualTaskTemplate result = (IManualTaskTemplate)(object)new ManualTaskTemplate(xmlManualTaskTemplate);
			int num = _xmlWorkflow.ManualTaskTemplates.FindIndex((Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate template) => xmlManualTaskTemplate.Id == template.Id);
			if (num != -1)
			{
				throw new ArgumentException("The specified manual task template is already part of this workflow.");
			}
			_xmlWorkflow.ManualTaskTemplates.Add(xmlManualTaskTemplate);
			_lazyManualTaskTemplates = null;
			return result;
		}

		public void RemoveManualTaskTemplate(IManualTaskTemplate template)
		{
			_xmlWorkflow.ManualTaskTemplates.RemoveAll((Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate t) => t.Id == ((ITaskTemplate)template).Id);
			_lazyManualTaskTemplates = null;
		}

		public IAutomaticTaskTemplate AddAutomaticTaskTemplate(string id, string name, string description)
		{
			Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate xmlAutomaticTaskTemplate = new Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate();
			xmlAutomaticTaskTemplate.Id = id;
			xmlAutomaticTaskTemplate.Name = name;
			xmlAutomaticTaskTemplate.Description = description;
			IAutomaticTaskTemplate result = (IAutomaticTaskTemplate)(object)new ShadowAutomaticTaskTemplate(xmlAutomaticTaskTemplate);
			int num = _xmlWorkflow.AutomaticTaskTemplates.FindIndex((Sdl.ProjectApi.Implementation.Xml.AutomaticTaskTemplate t) => xmlAutomaticTaskTemplate.Id == t.Id);
			if (num == -1)
			{
				_xmlWorkflow.AutomaticTaskTemplates.Add(xmlAutomaticTaskTemplate);
				_lazyAutomaticTaskTemplates = null;
			}
			return result;
		}

		public List<IManualTaskTemplate> GetLocalManualTaskTemplates()
		{
			return ManualTaskTemplates.Where((IManualTaskTemplate t) => !t.IsImported).ToList();
		}
	}
}
