using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdl.ProjectApi.Implementation
{
	public class CustomerProvider : ICustomerProvider
	{
		private readonly ICustomerProviderRepository _customerProviderRepository;

		private List<ICustomer> _lazyCustomerList;

		private readonly object _syncRoot = new object();

		internal List<ICustomer> CustomerList => _lazyCustomerList ?? (_lazyCustomerList = _customerProviderRepository.GetCustomers());

		public ICustomer[] Customers => CustomerList.ToArray();

		public CustomerProvider(ICustomerProviderRepository customerProviderRepository)
		{
			_customerProviderRepository = customerProviderRepository;
		}

		public ICustomer AddCustomer(string name, string email)
		{
			return AddCustomer(Guid.NewGuid(), name, email);
		}

		public ICustomer AddCustomer(Guid guid, string name, string email)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			lock (_syncRoot)
			{
				if (GetCustomerByName(name) != null)
				{
					throw new ProjectApiException(string.Format(ErrorMessages.Server_CustomerAlreadyExists, name));
				}
				ICustomer val = _customerProviderRepository.AddCustomer(guid, name, email);
				CustomerList.Add(val);
				return val;
			}
		}

		public ICustomer GetCustomerByName(string name)
		{
			lock (_syncRoot)
			{
				return CustomerList.FirstOrDefault((ICustomer customer) => customer.Name == name);
			}
		}

		public void RemoveCustomer(ICustomer customer)
		{
			if (customer == null)
			{
				throw new ArgumentNullException("customer");
			}
			lock (_syncRoot)
			{
				_customerProviderRepository.RemoveCustomer(customer.Guid);
				CustomerList.Remove((ICustomer)(object)(Customer)(object)customer);
			}
		}

		public ICustomer GetCustomer(Guid customerGuid)
		{
			lock (_syncRoot)
			{
				return CustomerList.FirstOrDefault((ICustomer c) => c.Guid == customerGuid);
			}
		}
	}
}
