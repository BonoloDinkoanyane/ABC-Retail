using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;

namespace ABCRetailFunctions.Services
{
    public class CustomerService
    {
        private readonly TableStorageService<Customer> _tableService;

        public CustomerService(string connectionString, string tableName)
        {
            _tableService = new TableStorageService<Customer>(connectionString, tableName);
        }

        public Task<List<Customer>> GetAllCustomersAsync()
            => _tableService.GetAllAsync();

        public Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
            => _tableService.GetAsync(partitionKey, rowKey);

        public Task AddCustomerAsync(Customer customer)
            => _tableService.AddAsync(customer);

        public Task UpdateCustomerAsync(Customer customer)
            => _tableService.UpdateAsync(customer);

        public Task DeleteCustomerAsync(string partitionKey, string rowKey)
            => _tableService.DeleteAsync(partitionKey, rowKey);
    }
}
