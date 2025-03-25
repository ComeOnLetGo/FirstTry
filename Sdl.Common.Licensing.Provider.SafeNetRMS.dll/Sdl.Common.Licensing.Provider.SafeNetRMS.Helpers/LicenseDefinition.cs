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
	[XmlRoot(Namespace = "", IsNullable = false)]
	public class LicenseDefinition
	{
		private LicenseEdition[] licenseEditionField;

		[XmlElement("LicenseEdition")]
		public LicenseEdition[] LicenseEdition
		{
			get
			{
				return licenseEditionField;
			}
			set
			{
				licenseEditionField = value;
			}
		}
	}
}
