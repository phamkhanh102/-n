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
    .Include(c => c.User)
    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (customer == null)
                    return BadRequest("Không tìm thấy customer");

                var order = new Order
                {
                    CustomerId   = customer.Id,
                    CustomerName = customer.User?.Name ?? "Khách",
                    CreatedAt    = DateTime.Now,
                    Status       = "Pending",
                    Address  = customer.Address,
                    Ward     = customer.Ward,
                    District = customer.District,
                    Province = customer.City,
                    Items    = new List<OrderItem>()
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

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Tìm Customer của user này
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return Ok(new List<object>()); // chưa có đơn hàng nào

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new {
                    o.Id,
                    o.Status,
                    o.TotalAmount,
                    o.CreatedAt,
                    o.Address,
                    o.District,
                    o.Province,
                    o.Phone,
                    o.CustomerName,
                    itemCount = o.Items.Count,
                    Items = o.Items.Select(i => new {
                        i.ProductName,
                        i.ColorName,
                        i.SizeNumber,
                        i.Quantity,
                        i.Price
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }


        // ─── CANCEL ORDER (USER) ──────────────────────────
        [Authorize]
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return NotFound(new { error = "Không tìm thấy tài khoản khách hàng." });

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customer.Id);

            if (order == null)
                return NotFound(new { error = "Không tìm thấy đơn hàng." });

            if (order.Status != "Pending")
                return BadRequest(new { error = "Chỉ có thể huỷ đơn hàng đang ở trạng thái 'Chờ xử lý'." });

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã huỷ đơn hàng thành công." });
        }


        // ─── GET ALL ORDERS (ADMIN) ──────────────────────
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new {
                    o.Id,
                    o.CustomerName,
                    o.Status,
                    o.TotalAmount,
                    o.CreatedAt,
                    o.Address,
                    o.District,
                    o.Province,
                    ItemCount = o.Items.Count
                })
                .ToListAsync();

            return Ok(orders);
        }


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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Lấy CustomerId từ JWT token nếu có (ưu tiên hơn từ body) ──
                int resolvedCustomerId = dto.CustomerId;
                string resolvedCustomerName = dto.CustomerName ?? "Khách";

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _context.Customers
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (customer == null)
                    {
                        // Tài khoản cũ chưa có Customer → tự động tạo
                        var userInfo = await _context.Users.FindAsync(userId);
                        if (userInfo != null)
                        {
                            customer = new Customer
                            {
                                UserId    = userId,
                                Address   = "",
                                City      = "",
                                District  = "",
                                Ward      = "",
                                CreatedAt = DateTime.Now
                            };
                            _context.Customers.Add(customer);
                            await _context.SaveChangesAsync();
                            // reload để có Id
                            customer = await _context.Customers
                                .Include(c => c.User)
                                .FirstOrDefaultAsync(c => c.UserId == userId);
                        }
                    }

                    if (customer != null)
                    {
                        resolvedCustomerId = customer.Id;
                        if (string.IsNullOrEmpty(dto.CustomerName))
                            resolvedCustomerName = customer.User?.Name ?? dto.CustomerName ?? "Khách";
                    }
                }
                else if (resolvedCustomerId > 0)
                {
                    // Guest checkout: dùng customerId từ body
                    var c = await _context.Customers.Include(x => x.User)
                        .FirstOrDefaultAsync(x => x.Id == resolvedCustomerId);
                    if (c != null && string.IsNullOrEmpty(dto.CustomerName))
                        resolvedCustomerName = c.User?.Name ?? "Khách";
                }

                if (resolvedCustomerId == 0)
                    return BadRequest(new { error = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập." });


                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest(new { error = "Đơn hàng phải có ít nhất 1 sản phẩm." });

                var order = new Order
                {
                    CustomerId   = resolvedCustomerId,
                    CustomerName = resolvedCustomerName,
                    CreatedAt    = DateTime.Now,
                    Status       = "Pending",
                    Address      = dto.Address  ?? "",
                    Ward         = dto.Ward     ?? "",
                    District     = dto.District ?? "",
                    Province     = dto.Province ?? "",
                    Phone        = dto.Phone    ?? "",
                    Items        = new List<OrderItem>()
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
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"Sản phẩm (variantId={item.VariantId}) không tồn tại." });
                    }

                    if (variant.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"Sản phẩm \"{variant.Product.Name}\" không đủ hàng." });
                    }

                    variant.Stock -= item.Quantity;

                    var orderItem = new OrderItem
                    {
                        VariantId   = variant.Id,
                        Quantity    = item.Quantity,
                        Price       = variant.Product.Price,
                        ProductName = variant.Product.Name,
                        ColorName   = variant.Color.Name,
                        SizeNumber  = variant.Size.Number
                    };

                    total += orderItem.Price * orderItem.Quantity;
                    order.Items.Add(orderItem);
                }

                // Áp dụng discount từ coupon (giới hạn không vượt quá subtotal)
                var discount = Math.Min(dto.DiscountAmount, total);
                if (discount < 0) discount = 0;

                order.TotalAmount = total - discount;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { id = order.Id, totalAmount = order.TotalAmount, discount, couponCode = dto.CouponCode, status = order.Status });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Lỗi server: " + ex.Message });
            }
        }

    }
}