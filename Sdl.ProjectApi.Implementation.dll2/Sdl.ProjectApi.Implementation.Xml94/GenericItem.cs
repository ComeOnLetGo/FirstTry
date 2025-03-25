using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(Workflow))]
	[XmlInclude(typeof(FilterDefinition))]
	[XmlInclude(typeof(TaskReport))]
	[XmlInclude(typeof(TaskFile))]
	[XmlInclude(typeof(Task))]
	[XmlInclude(typeof(AutomaticTask))]
	[XmlInclude(typeof(ManualTask))]
	[XmlInclude(typeof(FileVersion))]
	[XmlInclude(typeof(GenericItemWithSettings))]
	[XmlInclude(typeof(ProjectConfiguration))]
	[XmlInclude(typeof(Project))]
	[XmlInclude(typeof(LanguageFile))]
	[XmlInclude(typeof(LanguageDirection))]
	[XmlInclude(typeof(ProjectFile))]
	[XmlInclude(typeof(ProjectTemplateListItem))]
	[XmlInclude(typeof(SettingsBundle))]
	[XmlInclude(typeof(Customer))]
	[XmlInclude(typeof(ProjectListItem))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class GenericItem
	{
		private Guid guidField = Guid.Empty;

		private string externalIdField;

		[XmlAttribute]
		public Guid Guid
		{
			get
			{
				return guidField;
			}
			set
			{
				guidField = value;
			}
		}

		[XmlAttribute]
		public string ExternalId
		{
			get
			{
				return externalIdField;
			}
			set
			{
				externalIdField = value;
			}
		}

		public void AssignNewGuid()
		{
			Guid = Guid.NewGuid();
		}

		public bool HasGuid()
		{
			return Guid != Guid.Empty;
		}
	}
}
