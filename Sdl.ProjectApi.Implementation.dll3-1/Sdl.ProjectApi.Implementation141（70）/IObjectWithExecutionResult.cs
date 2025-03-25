namespace Sdl.ProjectApi.Implementation
{
	internal interface IObjectWithExecutionResult
	{
		void RaiseMessageReported(IExecutionMessage message);
	}
}
