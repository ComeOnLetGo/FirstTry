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
	public class ExecutionMessage
	{
		private string exceptionDataField;

		private string sourceField;

		private string messageField;

		private MessageLevel levelField;

		[XmlElement(Order = 0)]
		public string ExceptionData
		{
			get
			{
				return exceptionDataField;
			}
			set
			{
				exceptionDataField = value;
			}
		}

		[XmlAttribute]
		public string Source
		{
			get
			{
				return sourceField;
			}
			set
			{
				sourceField = value;
			}
		}

		[XmlAttribute]
		public string Message
		{
			get
			{
				return messageField;
			}
			set
			{
				messageField = value;
			}
		}

		[XmlAttribute]
		public MessageLevel Level
		{
			get
			{
				return levelField;
			}
			set
			{
				levelField = value;
			}
		}

		[XmlIgnore]
		public Exception Exception
		{
			get
			{
				if (ExceptionData == null)
				{
					return null;
				}
				return Util.DeserializeException(ExceptionData);
			}
			set
			{
				if (value != null)
				{
					try
					{
						ExceptionData = Util.SerializeException(value);
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
				ExceptionData = null;
			}
		}
	}
}
