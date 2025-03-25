using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class CustomerProviderRepository : ICustomerProviderRepository
	{
		private readonly IMainRepository _mainRepository;

		public CustomerProviderRepository(IMainRepository mainRepository)
		{
			_mainRepository = mainRepository;
		}

		public List<ICustomer> GetCustomers()
		{
			List<ICustomer> list = new List<ICustomer>();
			foreach (Sdl.ProjectApi.Implementation.Xml.Customer customer in _mainRepository.XmlProjectServer.Customers)
			{
				list.Add((ICustomer)(object)new Customer(customer));
			}
			return list;
		}

		public ICustomer AddCustomer(Guid guid, string name, string email)
		{
			Sdl.ProjectApi.Implementation.Xml.Customer customer = new Sdl.ProjectApi.Implementation.Xml.Customer
			{
				Guid = guid,
				Name = name,
				Email = email
			};
			_mainRepository.XmlProjectServer.Customers.Add(customer);
			return (ICustomer)(object)new Customer(customer);
		}

		public void RemoveCustomer(Guid guid)
		{
			foreach (Sdl.ProjectApi.Implementation.Xml.Customer customer in _mainRepository.XmlProjectServer.Customers)
			{
				if (customer.Guid.Equals(guid))
				{
					_mainRepository.XmlProjectServer.Customers.Remove(customer);
					break;
				}
			}
		}
	}
}
