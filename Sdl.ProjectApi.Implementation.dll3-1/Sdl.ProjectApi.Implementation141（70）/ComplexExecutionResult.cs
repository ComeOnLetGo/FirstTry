using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation
{
	internal class ComplexExecutionResult : IExecutionResult
	{
		private readonly IExecutionResult[] _results;

		public IExecutionMessage[] Messages
		{
			get
			{
				List<IExecutionMessage> list = new List<IExecutionMessage>();
				IExecutionResult[] results = _results;
				foreach (IExecutionResult val in results)
				{
					list.AddRange(val.Messages);
				}
				return list.ToArray();
			}
		}

		public bool HasErrors => Array.Exists(_results, (IExecutionResult result) => result.HasErrors);

		public bool HasWarnings => Array.Exists(_results, (IExecutionResult result) => result.HasWarnings);

		public bool HasInformation => Array.Exists(_results, (IExecutionResult result) => result.HasInformation);

		public int ErrorCount
		{
			get
			{
				int num = 0;
				IExecutionResult[] results = _results;
				foreach (IExecutionResult val in results)
				{
					num += val.ErrorCount;
				}
				return num;
			}
		}

		public int WarningCount
		{
			get
			{
				int num = 0;
				IExecutionResult[] results = _results;
				foreach (IExecutionResult val in results)
				{
					num += val.WarningCount;
				}
				return num;
			}
		}

		public int InformationCount
		{
			get
			{
				int num = 0;
				IExecutionResult[] results = _results;
				foreach (IExecutionResult val in results)
				{
					num += val.InformationCount;
				}
				return num;
			}
		}

		public event EventHandler<ExecutionMessageEventArgs> MessageReported;

		public ComplexExecutionResult(IExecutionResult[] results)
		{
			if (results == null || results.Length == 0)
			{
				throw new ArgumentNullException("results");
			}
			_results = results;
			IExecutionResult[] results2 = _results;
			foreach (IExecutionResult val in results2)
			{
				val.MessageReported += result_MessageReported;
			}
		}

		private void result_MessageReported(object sender, ExecutionMessageEventArgs e)
		{
			if (this.MessageReported != null)
			{
				this.MessageReported(this, e);
			}
		}
	}
}
