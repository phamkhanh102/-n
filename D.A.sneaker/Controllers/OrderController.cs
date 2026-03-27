using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // UPDATE ADDRESS
        [HttpPut("{id}/address")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Không tìm thấy đơn");

            // ⭐ NGHIỆP VỤ QUAN TRỌNG
            if (order.Status != "Pending")
                return BadRequest("Đơn hàng đã được xử lý, không thể đổi địa chỉ");

            order.Address = dto.Address;
            order.Ward = dto.Ward;
            order.District = dto.District;
            order.Province = dto.Province;

            await _context.SaveChangesAsync();

            return Ok("Đã cập nhật địa chỉ");
        }
        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _context.CartItems
     .Include(x => x.Variant).ThenInclude(v => v.Product)
     .Include(x => x.Variant).ThenInclude(v => v.Color)
     .Include(x => x.Variant).ThenInclude(v => v.Size)
     .Where(x => x.UserId == userId)
     .ToListAsync();

                if (!cart.Any())
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Cart rỗng");
                }

                var customer = await _context.Customers
    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (customer == null)
                    return BadRequest("Không tìm thấy customer");

                var order = new Order
                {
                    Address = customer.Address,
                    Ward = customer.Ward,
                    District = customer.District,
                    Province = customer.City,
                };

                decimal total = 0;

                foreach (var item in cart)
                {
                    if (item.Variant.Stock < item.Quantity)
                        return BadRequest("Hết hàng");

                    item.Variant.Stock -= item.Quantity;

                    var orderItem = new OrderItem
                    {
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Price = item.Variant.Product.Price,
                        ProductName = item.Variant.Product.Name,
                        ColorName = item.Variant.Color.Name,
                        SizeNumber = item.Variant.Size.Number
                    };

                    total += orderItem.Price * orderItem.Quantity;
                    order.Items.Add(orderItem);
                }

                order.TotalAmount = total;

                _context.Orders.Add(order);

                // clear cart
                _context.CartItems.RemoveRange(cart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500);
            }
        }
        //ORDER STATUS FLOW//
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            var valid = new[]
            {
        "Pending",
        "Confirmed",
        "Shipping",
        "Completed",
        "Cancelled"
    };

            if (!valid.Contains(status))
                return BadRequest("Status không hợp lệ");

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(order);
        }
        [Authorize]
        [HttpPost("{orderId}/pay")]
        public async Task<IActionResult> Pay(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Method = "COD",
                Status = "Paid",
                PaidAt = DateTime.Now
            };

            order.Status = "Paid";

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return Ok(payment);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDTO dto)
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                CreatedAt = DateTime.Now,
                Status = "Pending",
                Address = dto.Address,
                Ward = dto.Ward,
                District = dto.District,
                Province = dto.Province,
                Items = new List<OrderItem>()
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .Include(v => v.Size)
                    .Include(v => v.Color)
                    .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                if (variant == null)
                    return BadRequest("Variant không tồn tại");

                // ⭐ CHECK STOCK
                if (variant.Stock < item.Quantity)
                    return BadRequest("Không đủ hàng");

                // ⭐ TRỪ STOCK
                variant.Stock -= item.Quantity;

                var orderItem = new OrderItem
                {
                    VariantId = variant.Id,
                    Quantity = item.Quantity,
                    Price = variant.Product.Price,

                    // snapshot
                    ProductName = variant.Product.Name,
                    ColorName = variant.Color.Name,
                    SizeNumber = variant.Size.Number
                };

                total += orderItem.Price * orderItem.Quantity;

                order.Items.Add(orderItem);
            }

            order.TotalAmount = total;

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return Ok(order);
        }

    }
}