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
	public class CountData
	{
		private int wordsField;

		private int charactersField;

		private int segmentsField;

		private int placeablesField;

		private int tagsField;

		[XmlAttribute]
		public int Words
		{
			get
			{
				return wordsField;
			}
			set
			{
				wordsField = value;
			}
		}

		[XmlAttribute]
		public int Characters
		{
			get
			{
				return charactersField;
			}
			set
			{
				charactersField = value;
			}
		}

		[XmlAttribute]
		public int Segments
		{
			get
			{
				return segmentsField;
			}
			set
			{
				segmentsField = value;
			}
		}

		[XmlAttribute]
		public int Placeables
		{
			get
			{
				return placeablesField;
			}
			set
			{
				placeablesField = value;
			}
		}

		[XmlAttribute]
		public int Tags
		{
			get
			{
				return tagsField;
			}
			set
			{
				tagsField = value;
			}
		}

		public void Reset()
		{
			segmentsField = 0;
			wordsField = 0;
			charactersField = 0;
			placeablesField = 0;
			tagsField = 0;
		}
	}
}
