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
	public class ComplexTaskTemplate
	{
		private List<SubTaskTemplate> subTaskTemplatesField = new List<SubTaskTemplate>();

		private string idField;

		private string nameField;

		private string descriptionField;

		[XmlArray(Order = 0)]
		[XmlArrayItem(IsNullable = false)]
		public List<SubTaskTemplate> SubTaskTemplates
		{
			get
			{
				return subTaskTemplatesField;
			}
			set
			{
				subTaskTemplatesField = value;
			}
		}

		[XmlAttribute]
		public string Id
		{
			get
			{
				return idField;
			}
			set
			{
				idField = value;
			}
		}

		[XmlAttribute]
		public string Name
		{
			get
			{
				return nameField;
			}
			set
			{
				nameField = value;
			}
		}

		[XmlAttribute]
		public string Description
		{
			get
			{
				return descriptionField;
			}
			set
			{
				descriptionField = value;
			}
		}

		public ComplexTaskTemplate Copy()
		{
			ComplexTaskTemplate complexTaskTemplate = new ComplexTaskTemplate();
			complexTaskTemplate.Id = Id;
			complexTaskTemplate.Name = Name;
			complexTaskTemplate.Description = Description;
			foreach (SubTaskTemplate subTaskTemplate in SubTaskTemplates)
			{
				complexTaskTemplate.SubTaskTemplates.Add(subTaskTemplate.Copy());
			}
			return complexTaskTemplate;
		}
	}
}
