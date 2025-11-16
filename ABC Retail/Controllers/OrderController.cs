using ABC_Retail.Data;
using ABC_Retail.Models;
using ABC_Retail.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail.Controllers
{
    public class OrderController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public OrderController(QueueStorageService queueService, AppDbContext context, UserManager<Users> userManager)
        {
            _queueService = queueService;
            _context = context;
            _userManager = userManager;
        }

        // Default index action - redirects based on role
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Manage");
            }
            else
            {
                return RedirectToAction("MyOrders");
            }
        }

        // gets the logged in customers own orders
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Order
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // order details of a specific order
        [HttpGet]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Order
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            //restricts access to the order to the owner, or admin
            if (order == null || (order.UserId != userId && !User.IsInRole("Admin")))
                return Unauthorized();

            return View(order);
        }

        // the logged in admin can view and manage all orders 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var orders = await _context.Order
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // updating the order status 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Process(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "PROCESSED";
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }


        // get method for themessages accessible by the admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Messages()
        {
            var messages = await _queueService.GetMessagesAsync(); 
            var messageList = new List<string>();

            foreach (var msg in messages)
            {
                messageList.Add(msg.MessageText); // gets the content of the message
            }

            return View(messageList); // return to a view to display the messages
        }
    }
}
