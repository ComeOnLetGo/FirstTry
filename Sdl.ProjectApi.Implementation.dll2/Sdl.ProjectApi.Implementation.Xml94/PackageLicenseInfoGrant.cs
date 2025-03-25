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
	[XmlType(AnonymousType = true)]
	public class PackageLicenseInfoGrant
	{
		private string featureField;

		[XmlAttribute]
		public string Feature
		{
			get
			{
				return featureField;
			}
			set
			{
				featureField = value;
			}
		}
	}
}
