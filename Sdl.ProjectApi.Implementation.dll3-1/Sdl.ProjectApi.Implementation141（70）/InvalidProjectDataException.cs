using System;

namespace Sdl.ProjectApi.Implementation
{
	[Serializable]
	public class InvalidProjectDataException : ProjectApiException
	{
		public InvalidProjectDataException()
		{
		}

		public InvalidProjectDataException(string message)
			: base(message)
		{
		}

		public InvalidProjectDataException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}
