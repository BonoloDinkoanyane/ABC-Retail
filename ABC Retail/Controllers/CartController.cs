using System.Collections.Concurrent;
using ABC_Retail.Data;
using ABC_Retail.Models;
using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly TableStorageService<Product> _tableService;

        public CartController(AppDbContext context, UserManager<Users> userManager, TableStorageService<Product> tableService)
        {
            _context = context;
            _userManager = userManager;
            _tableService = tableService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var items = await _context.Cart
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(items);
        }

        // adding the items to the cart
        public async Task<IActionResult> Add(string rowKey, string partitionKey, string name, double price)
        {
            var userId = _userManager.GetUserId(User);

            // fetch product from Azure Table Storage
            var product = await _tableService.GetAsync(partitionKey, rowKey);

            if (product == null)
            {
                TempData["Error"] = "Product not found!";
                return RedirectToAction("Index", "Product");
            }

            var existing = await _context.Cart
                .FirstOrDefaultAsync(c =>
                    c.ProductRowKey == rowKey &&
                    c.ProductPartitionKey == partitionKey &&
                    c.UserId == userId);

            if (existing == null)
            {
                var item = new Cart
                {
                    UserId = userId,
                    ProductRowKey = rowKey,
                    ProductPartitionKey = partitionKey,
                    ProductName = name,
                    ProductPrice = price,
                    Quantity = 1
                };

                _context.Cart.Add(item);
            }
            else
            {
                existing.Quantity++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // updating the cart quantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.Cart.FindAsync(cartItemId);
            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        //
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = await _context.Cart
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Your cart is currently empty.";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = (decimal)cartItems.Sum(i => (i.ProductPrice ?? 0) * i.Quantity),
                Items = cartItems.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    UnitPrice = (decimal)(i.ProductPrice ?? 0),
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Order.Add(order);

            //clears the cart after an order is placed
            _context.Cart.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Order", new { id = order.OrderId });
        }

        // deelting an item from the cart
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.Cart.FindAsync(id);
            if (cartItem != null)
            {
                _context.Cart.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
