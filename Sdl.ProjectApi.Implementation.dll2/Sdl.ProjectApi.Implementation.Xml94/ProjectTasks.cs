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
	[XmlType(AnonymousType = true)]
	public class ProjectTasks
	{
		private List<Task> itemsField = new List<Task>();

		[XmlElement("AutomaticTask", typeof(AutomaticTask), Order = 0)]
		[XmlElement("ManualTask", typeof(ManualTask), Order = 0)]
		public List<Task> Items
		{
			get
			{
				return itemsField;
			}
			set
			{
				itemsField = value;
			}
		}
	}
}
