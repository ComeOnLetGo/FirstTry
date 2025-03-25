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
	public class TQAData
	{
		private bool tQAResultField;

		private string documentCategoryField;

		private string evaluationCommentField;

		[XmlAttribute]
		public bool TQAResult
		{
			get
			{
				return tQAResultField;
			}
			set
			{
				tQAResultField = value;
			}
		}

		[XmlAttribute]
		public string DocumentCategory
		{
			get
			{
				return documentCategoryField;
			}
			set
			{
				documentCategoryField = value;
			}
		}

		[XmlAttribute]
		public string EvaluationComment
		{
			get
			{
				return evaluationCommentField;
			}
			set
			{
				evaluationCommentField = value;
			}
		}
	}
}
