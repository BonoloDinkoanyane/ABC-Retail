using ABC_Retail.Models;
using ABC_Retail.Services;
using ABC_Retail.Services.Functions.CustomerFunctions;
using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerFunctionService _functionService;
        private readonly CustomerService _customerService;
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;
        private readonly FileShareStorageService _fileShareStorageService;

        public CustomerController(CustomerFunctionService functionService, CustomerService customerService, BlobStorageService blobStorageService, QueueStorageService queueStorageService, FileShareStorageService fileShareStorageService)
        {
            _functionService = functionService;
            _customerService = customerService;
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
            _fileShareStorageService = fileShareStorageService;
        }


        // GET: CustomerController/index 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _functionService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: CustomerController/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // GET: CustomerController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Ensure PartitionKey and RowKey are set
                if (string.IsNullOrEmpty(customer.PartitionKey))
                {
                    customer.PartitionKey = "Customer"; // this is the default partion should none be entered
                }

                if (string.IsNullOrEmpty(customer.RowKey))
                {
                    customer.RowKey = Guid.NewGuid().ToString();
                }

                // Adds Customer to the Table Storage
                await _customerService.AddCustomerAsync(customer);

                // Sends a ceration message to the queue
                var message = new
                {
                    Action = "New Customer created",
                    Timestamp = DateTime.UtcNow,
                    Details = new
                    {
                        customer.PartitionKey,
                        customer.RowKey,
                        customer.Name,
                        customer.Email,
                        customer.PhoneNumber,
                        customer.Address
                    }
                };
                await _queueStorageService.SendMessageAsync(message);

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: CustomerController/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        } 

        // POST: CustomerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.UpdateCustomerAsync(customer);

                var message = new
                {
                    Action = "Customer updated",
                    Timestamp = DateTime.UtcNow,
                    Details = new
                    {
                        customer.PartitionKey,
                        customer.RowKey,
                        customer.Name,
                        customer.Email,
                        customer.PhoneNumber,
                        customer.Address
                    }
                };
                await _queueStorageService.SendMessageAsync(message);

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: CustomerController/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            var customer = await _customerService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // GET: CustomerController/Logs
        [HttpGet]
        public async Task<IActionResult> Log()
        {
            try
            {
                // Get all messages from the queue
                var messages = await _queueStorageService.GetMessagesAsync();

                // Pass to the view
                return View(messages);
            }
            catch (Exception ex)
            {
                // Optional: log the error
                Console.WriteLine($"Error fetching queue messages: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch logs.");
            }
        }

        // POST: CustomerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return BadRequest();

            await _customerService.DeleteCustomerAsync(partitionKey, rowKey);

            var message = new
            {
                Action = "Customer deleted",
                Timestamp = DateTime.UtcNow,
                Details = new { partitionKey, rowKey }
            };
            await _queueStorageService.SendMessageAsync(message);

            return RedirectToAction(nameof(Index));
        }
    }
}
