using System;
using System.Collections.Generic;
using SDL.ApiClientSDK.GS.Models;
using Sdl.Desktop.Platform.ServerConnectionPlugin.Client.IdentityModel;

namespace Sdl.ProjectApi.Implementation.Server
{
	public static class ServerUserManager
	{
		public static UserDetails GetServerUser(Uri serverUri, string serverUserName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			UserManagerClient val = new UserManagerClient(serverUri.AbsoluteUri);
			try
			{
				return val.GetUserByUserName(serverUserName);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		public static IUser ToProjectApiUser(this UserDetails serverUser)
		{
			return (IUser)(object)new User(serverUser.Name)
			{
				FullName = serverUser.DisplayName,
				Description = serverUser.Description,
				Email = serverUser.EmailAddress,
				EmailType = (EmailType)1,
				PhoneNumber = serverUser.PhoneNumber
			};
		}

		public static IEnumerable<IUser> GetAllUsers(Uri serverUri)
		{
			UserManagerClient umc = new UserManagerClient(serverUri.AbsoluteUri);
			try
			{
				UserDetails[] allUsers = umc.GetAllUsers();
				UserDetails[] array = allUsers;
				foreach (UserDetails serverUser in array)
				{
					yield return serverUser.ToProjectApiUser();
				}
			}
			finally
			{
				((IDisposable)umc)?.Dispose();
			}
		}
	}
}
