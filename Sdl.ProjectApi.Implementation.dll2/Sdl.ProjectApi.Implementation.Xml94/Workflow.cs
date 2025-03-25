using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class Workflow : GenericItem
	{
		private List<ComplexTaskTemplate> complexTaskTemplatesField = new List<ComplexTaskTemplate>();

		private List<ManualTaskTemplate> manualTaskTemplatesField = new List<ManualTaskTemplate>();

		private List<AutomaticTaskTemplate> automaticTaskTemplatesField = new List<AutomaticTaskTemplate>();

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<ComplexTaskTemplate> ComplexTaskTemplates
		{
			get
			{
				return complexTaskTemplatesField;
			}
			set
			{
				complexTaskTemplatesField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<ManualTaskTemplate> ManualTaskTemplates
		{
			get
			{
				return manualTaskTemplatesField;
			}
			set
			{
				manualTaskTemplatesField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<AutomaticTaskTemplate> AutomaticTaskTemplates
		{
			get
			{
				return automaticTaskTemplatesField;
			}
			set
			{
				automaticTaskTemplatesField = value;
			}
		}

		public Workflow Copy()
		{
			Workflow workflow = new Workflow();
			workflow.Guid = base.Guid;
			workflow.ExternalId = base.ExternalId;
			foreach (ComplexTaskTemplate complexTaskTemplate in ComplexTaskTemplates)
			{
				workflow.ComplexTaskTemplates.Add(complexTaskTemplate.Copy());
			}
			foreach (ManualTaskTemplate manualTaskTemplate in ManualTaskTemplates)
			{
				workflow.ManualTaskTemplates.Add(manualTaskTemplate.Copy());
			}
			return workflow;
		}
	}
}
