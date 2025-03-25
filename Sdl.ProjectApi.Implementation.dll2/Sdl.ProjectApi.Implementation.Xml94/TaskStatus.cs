using System;
using System.CodeDom.Compiler;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	public enum TaskStatus
	{
		Invalid,
		Created,
		Assigned,
		Started,
		Rejected,
		Completed,
		PartiallyCompleted,
		Failed,
		Cancelling,
		Cancelled,
		Skipped
	}
}
