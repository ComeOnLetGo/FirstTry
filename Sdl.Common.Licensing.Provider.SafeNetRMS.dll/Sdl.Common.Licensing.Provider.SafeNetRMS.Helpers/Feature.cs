using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	[Serializable]
	[GeneratedCode("System.Xml", "4.0.30319.34234")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class Feature
	{
		private string nameField;

		private bool isDefaultField;

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

		public bool IsDefault
		{
			get
			{
				return isDefaultField;
			}
			set
			{
				isDefaultField = value;
			}
		}
	}
}
