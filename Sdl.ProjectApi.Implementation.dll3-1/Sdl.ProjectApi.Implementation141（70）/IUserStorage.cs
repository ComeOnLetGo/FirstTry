namespace Sdl.ProjectApi.Implementation
{
	public interface IUserStorage
	{
		IUser Read(string userId);

		void Save(IUser user);
	}
}
