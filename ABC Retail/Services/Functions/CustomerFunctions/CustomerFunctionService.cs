using ABC_Retail.Models;
using ABC_Retail.Controllers;
using ABC_Retail.Services.Storage;

namespace ABC_Retail.Services.Functions.CustomerFunctions
{
    public class CustomerFunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public CustomerFunctionService(HttpClient httpClient, IConfiguration configuration)
        { 
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionsBaseUrlProd"] ?? throw new InvalidOperationException("Azure Functions Base Url is missing.");
        }

        //get all customers
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<Customer>>($"{_functionBaseUrl}/api/customers");
            return response ?? new List<Customer>();
        }

        //get customer details
        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var response = await _httpClient.GetFromJsonAsync<Customer>($"{_functionBaseUrl}/api/customers/{partitionKey}/{rowKey}");
            return response;
        }

        //update customer

        //create customer
        public async Task<Customer?> AddCustomerAsync(Customer customer)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_functionBaseUrl}/api/customers", customer);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<Customer>();
            return null;
        }
        //delete customer
    }
}
