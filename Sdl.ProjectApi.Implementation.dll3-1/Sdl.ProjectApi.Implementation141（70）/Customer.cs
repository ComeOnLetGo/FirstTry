using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class Customer : ICustomer
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.Customer _xmlCustomer;

		public Guid Guid => _xmlCustomer.Guid;

		public string Name
		{
			get
			{
				return _xmlCustomer.Name;
			}
			set
			{
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (!(value == _xmlCustomer.Name))
				{
					if (value != null && value.Length == 0)
					{
						throw new ProjectApiException(ErrorMessages.Server_CustomerNameEmpty);
					}
					_xmlCustomer.Name = value;
				}
			}
		}

		public string Email
		{
			get
			{
				return _xmlCustomer.Email;
			}
			set
			{
				_xmlCustomer.Email = value;
			}
		}

		internal Customer(Sdl.ProjectApi.Implementation.Xml.Customer xmlCustomer)
		{
			_xmlCustomer = xmlCustomer;
		}

		internal Customer(string name, string email)
		{
			_xmlCustomer = new Sdl.ProjectApi.Implementation.Xml.Customer
			{
				Guid = Guid.NewGuid(),
				Name = name,
				Email = email
			};
		}

		public override bool Equals(object obj)
		{
			ICustomer val = (ICustomer)((obj is ICustomer) ? obj : null);
			if (val != null)
			{
				return val.Guid.Equals(Guid);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}
	}
}
