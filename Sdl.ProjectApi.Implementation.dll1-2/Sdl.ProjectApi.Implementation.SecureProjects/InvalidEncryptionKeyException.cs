namespace Sdl.ProjectApi.Implementation.SecureProjects
{
	public class InvalidEncryptionKeyException : ProjectApiException
	{
		public InvalidEncryptionKeyException(string message)
			: base(message)
		{
		}
	}
}
