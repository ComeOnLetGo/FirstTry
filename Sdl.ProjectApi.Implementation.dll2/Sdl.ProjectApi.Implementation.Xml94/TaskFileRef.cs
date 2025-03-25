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
	public class TaskFileRef
	{
		private Guid taskFileGuidField = Guid.Empty;

		[XmlAttribute]
		public Guid TaskFileGuid
		{
			get
			{
				return taskFileGuidField;
			}
			set
			{
				taskFileGuidField = value;
			}
		}
	}
}
