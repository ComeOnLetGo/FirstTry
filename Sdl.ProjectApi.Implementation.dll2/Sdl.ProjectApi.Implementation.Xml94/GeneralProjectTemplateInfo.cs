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
	public class GeneralProjectTemplateInfo
	{
		private string descriptionField;

		private DateTime createdAtField;

		private string createdByField;

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

		[XmlAttribute]
		public DateTime CreatedAt
		{
			get
			{
				return createdAtField;
			}
			set
			{
				createdAtField = value;
			}
		}

		[XmlAttribute]
		public string CreatedBy
		{
			get
			{
				return createdByField;
			}
			set
			{
				createdByField = value;
			}
		}
	}
}
