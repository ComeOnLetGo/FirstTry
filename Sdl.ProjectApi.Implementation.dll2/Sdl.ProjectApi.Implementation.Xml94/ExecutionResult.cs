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
	public class ExecutionResult
	{
		private List<ExecutionMessage> messagesField = new List<ExecutionMessage>();

		private Guid guidField = Guid.Empty;

		[XmlElement("Messages", Order = 0)]
		public List<ExecutionMessage> Messages
		{
			get
			{
				return messagesField;
			}
			set
			{
				messagesField = value;
			}
		}

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
	}
}
