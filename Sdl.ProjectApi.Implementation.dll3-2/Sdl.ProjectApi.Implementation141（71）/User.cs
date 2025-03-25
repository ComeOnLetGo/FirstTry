using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class User : IUser
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.User _xmlUser;

		public string UserId => _xmlUser.UserId;

		public string Email
		{
			get
			{
				return _xmlUser.Email;
			}
			set
			{
				_xmlUser.Email = value;
			}
		}

		public EmailType EmailType
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return EnumConvert.ConvertEmailType(_xmlUser.EmailType);
			}
			set
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				_xmlUser.EmailType = EnumConvert.ConvertEmailType(value);
			}
		}

		public string FullName
		{
			get
			{
				return _xmlUser.FullName;
			}
			set
			{
				_xmlUser.FullName = value;
			}
		}

		public string PhoneNumber
		{
			get
			{
				return _xmlUser.PhoneNumber;
			}
			set
			{
				_xmlUser.PhoneNumber = value;
			}
		}

		public string Description
		{
			get
			{
				return _xmlUser.Description;
			}
			set
			{
				_xmlUser.Description = value;
			}
		}

		internal User(Sdl.ProjectApi.Implementation.Xml.User xmlUser)
		{
			_xmlUser = xmlUser;
		}

		public User(string userId)
		{
			_xmlUser = new Sdl.ProjectApi.Implementation.Xml.User();
			_xmlUser.UserId = userId;
			_xmlUser.FullName = userId;
		}

		public override bool Equals(object obj)
		{
			IUser val = (IUser)((obj is IUser) ? obj : null);
			if (val == null)
			{
				return false;
			}
			if (string.Equals(UserId, val.UserId, StringComparison.OrdinalIgnoreCase) && string.Equals(FullName, val.FullName, StringComparison.OrdinalIgnoreCase) && string.Equals(Email, val.Email, StringComparison.OrdinalIgnoreCase) && string.Equals(Description, val.Description, StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(PhoneNumber, val.PhoneNumber, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return UserId.GetHashCode();
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
