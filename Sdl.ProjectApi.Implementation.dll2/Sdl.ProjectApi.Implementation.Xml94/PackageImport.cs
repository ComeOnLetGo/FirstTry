using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[XmlInclude(typeof(ReturnPackageImport))]
	[XmlInclude(typeof(ProjectPackageImport))]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class PackageImport : PackageOperation
	{
		private string pathField;

		[XmlAttribute]
		public string Path
		{
			get
			{
				return pathField;
			}
			set
			{
				pathField = value;
			}
		}
	}
}
