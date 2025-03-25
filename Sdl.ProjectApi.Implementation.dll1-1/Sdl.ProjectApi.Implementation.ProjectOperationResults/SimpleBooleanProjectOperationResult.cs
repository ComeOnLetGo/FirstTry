namespace Sdl.ProjectApi.Implementation.ProjectOperationResults
{
	public class SimpleBooleanProjectOperationResult : IProjectOperationResult
	{
		public bool IsSuccesful { get; set; }

		public object Result { get; set; }

		public SimpleBooleanProjectOperationResult()
		{
		}

		public SimpleBooleanProjectOperationResult(bool result)
		{
			IsSuccesful = result;
		}
	}
}
