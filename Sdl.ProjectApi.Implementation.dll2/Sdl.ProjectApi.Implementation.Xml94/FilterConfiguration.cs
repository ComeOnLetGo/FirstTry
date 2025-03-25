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
	public class FilterConfiguration
	{
		private string filterManagerConfigFileContentField;

		private List<FilterDefinition> filterDefinitionsField = new List<FilterDefinition>();

		[XmlElement(Order = 0)]
		public string FilterManagerConfigFileContent
		{
			get
			{
				return filterManagerConfigFileContentField;
			}
			set
			{
				filterManagerConfigFileContentField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<FilterDefinition> FilterDefinitions
		{
			get
			{
				return filterDefinitionsField;
			}
			set
			{
				filterDefinitionsField = value;
			}
		}
	}
}
