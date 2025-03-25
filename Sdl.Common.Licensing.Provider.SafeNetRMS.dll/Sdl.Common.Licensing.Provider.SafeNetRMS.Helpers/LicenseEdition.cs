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
	public class LicenseEdition
	{
		private string nameField;

		private bool isCurrentField;

		private Feature[] featuresField;

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

		public bool IsCurrent
		{
			get
			{
				return isCurrentField;
			}
			set
			{
				isCurrentField = value;
			}
		}

		[XmlArrayItem("Feature", IsNullable = false)]
		public Feature[] Features
		{
			get
			{
				return featuresField;
			}
			set
			{
				featuresField = value;
			}
		}
	}
}
