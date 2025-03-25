using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class FilterDefinition : GenericItem
	{
		private string contentField;

		private string filterDefinitionFileNameField;

		[XmlElement(Order = 0)]
		public string Content
		{
			get
			{
				return contentField;
			}
			set
			{
				contentField = value;
			}
		}

		[XmlAttribute]
		public string FilterDefinitionFileName
		{
			get
			{
				return filterDefinitionFileNameField;
			}
			set
			{
				filterDefinitionFileNameField = value;
			}
		}

		public FilterDefinition Copy()
		{
			return (FilterDefinition)MemberwiseClone();
		}
	}
}
