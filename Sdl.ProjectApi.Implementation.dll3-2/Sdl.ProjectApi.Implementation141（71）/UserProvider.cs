using System;

namespace Sdl.ProjectApi.Implementation
{
	public class UserProvider : IUserProvider
	{
		private readonly IUserProviderRepository _userProviderRepository;

		private readonly IUserStorage _userStorage;

		private readonly object _syncRoot = new object();

		public IUser[] Users
		{
			get
			{
				lock (_syncRoot)
				{
					return _userProviderRepository.GetUsers().ToArray();
				}
			}
		}

		public IUser CurrentUser { get; set; }

		public UserProvider(IUserProviderRepository userProviderRepository, IUserStorage userStorage)
		{
			_userProviderRepository = userProviderRepository;
			_userStorage = userStorage;
		}

		public IUser GetUserById(string userId)
		{
			lock (_syncRoot)
			{
				return _userProviderRepository.GetUser(userId);
			}
		}

		public IUser CreateUser(string userId, string fullName)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			lock (_syncRoot)
			{
				IUser user = _userProviderRepository.GetUser(userId);
				if (user != null)
				{
					throw new ProjectApiException(ErrorMessages.ProjectsProvider_UserIdInUse);
				}
				return _userProviderRepository.CreateOrUpdateUser(userId, fullName);
			}
		}

		public void DeleteUser(IUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			lock (_syncRoot)
			{
				if (((object)user).Equals((object)CurrentUser))
				{
					throw new ArgumentException("Cannot delete the current user.", "user");
				}
				_userProviderRepository.RemoveUser(user.UserId);
			}
		}

		public void SaveCurrentWindowsUser()
		{
			lock (_syncRoot)
			{
				string windowsUserId = UserHelper.WindowsUserId;
				IUser user = _userProviderRepository.GetUser(windowsUserId);
				if (user != null)
				{
					CurrentUser = user;
					_userStorage.Save(user);
				}
			}
		}

		public IUser CreateCurrentWindowsUser()
		{
			lock (_syncRoot)
			{
				string windowsUserId = UserHelper.WindowsUserId;
				IUser user = _userProviderRepository.GetUser(windowsUserId);
				if (user != null && !string.IsNullOrEmpty(user.Email))
				{
					CurrentUser = user;
					return user;
				}
				IUser val = _userStorage.Read(windowsUserId);
				return CurrentUser = _userProviderRepository.CreateOrUpdateUser(val);
			}
		}

		public IUser CreateOrUpdateUser(IUser user)
		{
			lock (_syncRoot)
			{
				return CurrentUser = _userProviderRepository.CreateOrUpdateUser(user);
			}
		}
	}
}
