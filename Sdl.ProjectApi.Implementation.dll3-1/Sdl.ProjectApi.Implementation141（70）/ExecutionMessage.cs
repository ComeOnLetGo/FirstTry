using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class ExecutionMessage : IExecutionMessage
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.ExecutionMessage _xmlMessage;

		public string Source => _xmlMessage.Source;

		public Exception Exception
		{
			get
			{
				return _xmlMessage.Exception;
			}
			set
			{
				_xmlMessage.Exception = value;
			}
		}

		public string Message => _xmlMessage.Message;

		public MessageLevel Level => EnumConvert.ConvertMessageLevel(_xmlMessage.Level);

		public string ExceptionData => _xmlMessage.ExceptionData;

		public ExecutionMessage(Sdl.ProjectApi.Implementation.Xml.ExecutionMessage xmlMessage)
		{
			_xmlMessage = xmlMessage;
		}
	}
}
