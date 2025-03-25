using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class UserProviderRepository : IUserProviderRepository
	{
		private readonly IMainRepository _mainRepository;

		public UserProviderRepository(IMainRepository mainRepository)
		{
			_mainRepository = mainRepository;
		}

		public void RemoveUser(string userId)
		{
			for (int i = 0; i < _mainRepository.XmlProjectServer.Users.Count; i++)
			{
				if (string.Compare(_mainRepository.XmlProjectServer.Users[i].UserId, userId, ignoreCase: true) == 0)
				{
					_mainRepository.XmlProjectServer.Users.RemoveAt(i);
					break;
				}
			}
		}

		public List<IUser> GetUsers()
		{
			return _mainRepository.XmlProjectServer.Users.ConvertAll((Sdl.ProjectApi.Implementation.Xml.User xmlUser) => CreateOrUpdateUser(xmlUser));
		}

		public IUser GetUser(string userId)
		{
			Sdl.ProjectApi.Implementation.Xml.User user2 = _mainRepository.XmlProjectServer.Users.FirstOrDefault((Sdl.ProjectApi.Implementation.Xml.User user) => string.Compare(user.UserId, userId, ignoreCase: true) == 0);
			if (user2 == null)
			{
				return null;
			}
			return (IUser)(object)new User(user2);
		}

		public IUser CreateOrUpdateUser(string userId, string fullName)
		{
			Sdl.ProjectApi.Implementation.Xml.User user = new Sdl.ProjectApi.Implementation.Xml.User();
			user.UserId = userId;
			user.FullName = fullName;
			return CreateOrUpdateUser(user);
		}

		public IUser CreateOrUpdateUser(IUser user)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.User newXmlUser = new Sdl.ProjectApi.Implementation.Xml.User
			{
				UserId = user.UserId,
				FullName = user.FullName,
				Description = user.Description,
				Email = user.Email,
				EmailType = EnumConvert.ConvertEmailType(user.EmailType),
				PhoneNumber = user.PhoneNumber
			};
			return CreateOrUpdateUser(newXmlUser);
		}

		public IUser CreateOrUpdateUser(Sdl.ProjectApi.Implementation.Xml.User newXmlUser)
		{
			for (int i = 0; i < _mainRepository.XmlProjectServer.Users.Count; i++)
			{
				if (string.Compare(_mainRepository.XmlProjectServer.Users[i].UserId, newXmlUser.UserId, ignoreCase: true) == 0)
				{
					_mainRepository.XmlProjectServer.Users[i] = newXmlUser;
					return (IUser)(object)new User(newXmlUser);
				}
			}
			_mainRepository.XmlProjectServer.Users.Add(newXmlUser);
			return (IUser)(object)new User(newXmlUser);
		}
	}
}
