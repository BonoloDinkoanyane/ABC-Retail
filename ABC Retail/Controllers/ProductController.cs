using ABC_Retail.Models;
using ABC_Retail.Services;
using ABC_Retail.Services.Functions.ProductFunctions;
using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ProductFunctionService _productFunctionService;
        private readonly CustomerService _customerService;
        private readonly OrderService _orderService;
        private readonly QueueStorageService _queueService;
        private readonly FileShareStorageService _fileShareService;
        private readonly BlobStorageService _blobService;

        // Constructor injects all services
        public ProductController(
            ProductService productService,
            CustomerService customerService,
            OrderService orderService,
            QueueStorageService queueService,
            FileShareStorageService fileShareService,
            BlobStorageService blobService,
            ProductFunctionService productFunctionService)
        {
            _productFunctionService = productFunctionService;
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _queueService = queueService;
            _fileShareService = fileShareService;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productFunctionService.GetAllProductsAsync();
            return View(products);
        }

        //manage product controlleer is onnly for the admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var products = await _productFunctionService.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var product = await _productFunctionService.GetProductAsync(partitionKey, rowKey);
            return product != null ? View(product) : NotFound();
        }

        // creating a product
        public IActionResult Create()
        {
            return View();
        }


        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                await _productFunctionService.AddProductAsync(product, photo);
                return RedirectToAction(nameof(Index));
            }

            // If model validation failed, redisplay form with errors
            return View(product);
        }

        // fetching the edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _productFunctionService.GetProductAsync(partitionKey, rowKey);
            return product != null ? View(product) : NotFound();
        }

        // posting the edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Product product, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                await _productFunctionService.UpdateProductAsync(product, photo);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _productFunctionService.GetProductAsync(partitionKey, rowKey);
            return product != null ? View(product) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _productFunctionService.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
