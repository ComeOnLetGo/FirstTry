namespace Sdl.ProjectApi.Implementation.SecureProjects
{
	public class InvalidEncryptedProjectFileException : ProjectApiException
	{
		public InvalidEncryptedProjectFileException(string message)
			: base(message)
		{
		}
	}
}
