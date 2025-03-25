using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Sdl.ProjectApi.Implementation
{
	public class UserRegistryStorage : IUserStorage
	{
		private enum EXTENDED_NAME_FORMAT
		{
			NameUnknown = 0,
			NameFullyQualifiedDN = 1,
			NameSamCompatible = 2,
			NameDisplay = 3,
			NameUniqueId = 4,
			NameCanonical = 7,
			NameUserPrincipal = 8,
			NameCanonicalEx = 9,
			NameServicePrincipal = 10,
			NameDnsDomain = 12
		}

		private readonly ILogger<UserRegistryStorage> _log;

		[DllImport("Secur32.dll", CharSet = CharSet.Auto)]
		private static extern int GetUserNameEx(EXTENDED_NAME_FORMAT format, StringBuilder username, ref int size);

		public UserRegistryStorage(ILogger<UserRegistryStorage> log)
		{
			_log = log;
		}

		public IUser Read(string userId)
		{
			User user = new User(userId);
			user.FullName = string.Empty;
			string text = GetCurrentUserFullName() ?? userId;
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Constants.Registry.KeyCurrentUser);
			if (registryKey != null)
			{
				string text2 = registryKey.GetValue("CurrentUserId") as string;
				if (text2 == user.UserId)
				{
					user.Email = registryKey.GetValue("CurrentUserEmail") as string;
					user.EmailType = (EmailType)1;
					user.FullName = registryKey.GetValue("CurrentUserFullName", text) as string;
					user.PhoneNumber = registryKey.GetValue("CurrentUserPhoneNumber", string.Empty) as string;
					user.Description = registryKey.GetValue("CurrentUserDescription", string.Empty) as string;
				}
				registryKey.Close();
			}
			else
			{
				LoggerExtensions.LogWarning((ILogger)(object)_log, "UserRegistryStorage.Read: Failed to read registry data", Array.Empty<object>());
			}
			if (string.IsNullOrEmpty(user.FullName))
			{
				user.FullName = text;
			}
			if (user.Description == null)
			{
				user.Description = string.Empty;
			}
			return (IUser)(object)user;
		}

		public void Save(IUser user)
		{
			if (string.IsNullOrWhiteSpace(user.FullName))
			{
				user.FullName = GetCurrentUserFullName() ?? user.UserId;
			}
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(Constants.Registry.KeyCurrentUser);
			if (registryKey != null)
			{
				registryKey.SetValue("CurrentUserId", user.UserId);
				registryKey.SetValue("CurrentUserEmail", (user.Email != null) ? user.Email.Trim() : string.Empty);
				registryKey.SetValue("CurrentUserFullName", user.FullName.Trim());
				registryKey.SetValue("CurrentUserPhoneNumber", (user.PhoneNumber != null) ? user.PhoneNumber.Trim() : string.Empty);
				registryKey.SetValue("CurrentUserDescription", (user.Description != null) ? user.Description.Trim() : string.Empty);
				registryKey.Close();
			}
			else
			{
				LoggerExtensions.LogWarning((ILogger)(object)_log, "UserRegistryStorage.Save: Failed to access registry data", Array.Empty<object>());
			}
		}

		private string GetCurrentUserFullName()
		{
			string result = null;
			try
			{
				int size = 1024;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Length = size;
				if (GetUserNameEx(EXTENDED_NAME_FORMAT.NameDisplay, stringBuilder, ref size) != 0 && stringBuilder.Length > 0)
				{
					result = stringBuilder.ToString();
				}
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError((ILogger)(object)_log, ex, "Failed to get user name", Array.Empty<object>());
			}
			return result;
		}
	}
}
