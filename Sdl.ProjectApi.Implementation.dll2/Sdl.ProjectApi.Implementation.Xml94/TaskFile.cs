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
	public class TaskFile : GenericItem
	{
		private ExecutionResult resultField;

		private Guid languageFileGuidField = Guid.Empty;

		private TaskFilePurpose purposeField;

		private bool completedField;

		private Guid parentTaskFileGuidField = Guid.Empty;

		[XmlElement(Order = 0)]
		public ExecutionResult Result
		{
			get
			{
				return resultField;
			}
			set
			{
				resultField = value;
			}
		}

		[XmlAttribute]
		public Guid LanguageFileGuid
		{
			get
			{
				return languageFileGuidField;
			}
			set
			{
				languageFileGuidField = value;
			}
		}

		[XmlAttribute]
		public TaskFilePurpose Purpose
		{
			get
			{
				return purposeField;
			}
			set
			{
				purposeField = value;
			}
		}

		[XmlAttribute]
		public bool Completed
		{
			get
			{
				return completedField;
			}
			set
			{
				completedField = value;
			}
		}

		[XmlAttribute]
		public Guid ParentTaskFileGuid
		{
			get
			{
				return parentTaskFileGuidField;
			}
			set
			{
				parentTaskFileGuidField = value;
			}
		}
	}
}
