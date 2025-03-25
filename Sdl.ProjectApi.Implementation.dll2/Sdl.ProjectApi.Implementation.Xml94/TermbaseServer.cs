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
	public class TermbaseServer
	{
		private string serverConnectionUriField;

		[XmlElement(Order = 0)]
		public string ServerConnectionUri
		{
			get
			{
				return serverConnectionUriField;
			}
			set
			{
				serverConnectionUriField = value;
			}
		}
	}
}
