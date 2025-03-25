using System;
using System.Collections.Generic;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class ExecutionResult : IExecutionResult
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.ExecutionResult _xmlExecutionResult;

		private readonly IObjectWithExecutionResult _owner;

		private readonly List<IExecutionMessage> _messagesList;

		private int _errorCount;

		private int _warningCount;

		private int _informationCount;

		public IExecutionMessage[] Messages => _messagesList.ToArray();

		public bool HasErrors => ErrorCount > 0;

		public bool HasWarnings => WarningCount > 0;

		public bool HasInformation => InformationCount > 0;

		public int ErrorCount => _errorCount;

		public int WarningCount => _warningCount;

		public int InformationCount => _informationCount;

		public event EventHandler<ExecutionMessageEventArgs> MessageReported;

		public ExecutionResult(Sdl.ProjectApi.Implementation.Xml.ExecutionResult xmlExecutionResult, IObjectWithExecutionResult owner)
		{
			if (xmlExecutionResult == null)
			{
				throw new ArgumentNullException("xmlExecutionResult");
			}
			_xmlExecutionResult = xmlExecutionResult;
			_owner = owner;
			_messagesList = new List<IExecutionMessage>();
			foreach (Sdl.ProjectApi.Implementation.Xml.ExecutionMessage message in _xmlExecutionResult.Messages)
			{
				_messagesList.Add((IExecutionMessage)(object)new ExecutionMessage(message));
				UpdateCounts(message);
			}
		}

		protected virtual void OnMessageReported(IExecutionMessage message)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if (this.MessageReported != null)
			{
				this.MessageReported(this, new ExecutionMessageEventArgs(message));
			}
		}

		private IExecutionMessage AddMessage(Sdl.ProjectApi.Implementation.Xml.ExecutionMessage xmlMessage)
		{
			_xmlExecutionResult.Messages.Add(xmlMessage);
			UpdateCounts(xmlMessage);
			IExecutionMessage val = (IExecutionMessage)(object)new ExecutionMessage(xmlMessage);
			val.Message.XmlSanitize();
			_messagesList.Add(val);
			OnMessageReported(val);
			if (_owner != null)
			{
				_owner.RaiseMessageReported(val);
			}
			return val;
		}

		private void UpdateCounts(Sdl.ProjectApi.Implementation.Xml.ExecutionMessage xmlMessage)
		{
			if (_errorCount != -1)
			{
				switch (xmlMessage.Level)
				{
				case MessageLevel.Error:
					_errorCount++;
					break;
				case MessageLevel.Warning:
					_warningCount++;
					break;
				case MessageLevel.Information:
					_informationCount++;
					break;
				default:
					throw new ArgumentException("Unexpected message level: " + xmlMessage.Level);
				}
			}
		}

		internal void ReportMessage(string source, string message, MessageLevel level, Exception exception)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ExecutionMessage executionMessage = new Sdl.ProjectApi.Implementation.Xml.ExecutionMessage();
			executionMessage.Source = source;
			executionMessage.Message = message.XmlSanitize();
			executionMessage.Exception = exception;
			executionMessage.Level = EnumConvert.ConvertMessageLevel(level);
			AddMessage(executionMessage);
		}

		internal void ReportMessage(string source, string message, MessageLevel level, string exceptionData)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ExecutionMessage executionMessage = new Sdl.ProjectApi.Implementation.Xml.ExecutionMessage();
			executionMessage.Source = source;
			executionMessage.Message = message.XmlSanitize();
			executionMessage.ExceptionData = exceptionData;
			executionMessage.Level = EnumConvert.ConvertMessageLevel(level);
			AddMessage(executionMessage);
		}

		internal void ReportMessage(string source, string message, MessageLevel level)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ExecutionMessage executionMessage = new Sdl.ProjectApi.Implementation.Xml.ExecutionMessage();
			executionMessage.Source = source;
			executionMessage.Message = message.XmlSanitize();
			executionMessage.Level = EnumConvert.ConvertMessageLevel(level);
			AddMessage(executionMessage);
		}

		internal void ReportMessage(string source, string message, MessageLevel level, IMessageLocation fromLocation, IMessageLocation uptoLocation)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.ExecutionMessage executionMessage = new Sdl.ProjectApi.Implementation.Xml.ExecutionMessage();
			executionMessage.Source = source;
			executionMessage.Message = message.XmlSanitize();
			executionMessage.Level = EnumConvert.ConvertMessageLevel(level);
			AddMessage(executionMessage);
		}

		internal void Clear()
		{
			_xmlExecutionResult.Messages.Clear();
			if (_messagesList != null)
			{
				_messagesList.Clear();
			}
			_errorCount = 0;
			_warningCount = 0;
			_informationCount = 0;
		}
	}
}
