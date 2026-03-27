using D.A.sneaker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();

            var revenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            var users = await _context.Users.CountAsync();

            var products = await _context.Products.CountAsync();

            var topProducts = await _context.OrderItems
                .GroupBy(x => x.ProductName)
                .Select(g => new {
                    name = g.Key,
                    sold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.sold)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                totalOrders,
                revenue,
                users,
                products,
                topProducts
            });
        }

        // UPDATE ORDER STATUS
        [HttpPut("orders/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
