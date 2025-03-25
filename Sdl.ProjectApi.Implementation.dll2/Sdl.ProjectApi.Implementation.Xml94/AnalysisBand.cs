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
	public class AnalysisBand
	{
		private int minimumMatchValueField;

		[XmlAttribute]
		public int MinimumMatchValue
		{
			get
			{
				return minimumMatchValueField;
			}
			set
			{
				minimumMatchValueField = value;
			}
		}

		public AnalysisBand()
		{
		}

		public AnalysisBand(int minimumMatchValue)
		{
			minimumMatchValueField = minimumMatchValue;
		}

		public AnalysisBand Copy()
		{
			return (AnalysisBand)MemberwiseClone();
		}
	}
}
