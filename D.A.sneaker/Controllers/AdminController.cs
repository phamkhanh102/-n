using D.A.sneaker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) => _context = context;

        // ── HELPER: kiểm tra quyền theo role ─────────────────────
        private bool HasRole(params string[] roles)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            return roles.Contains(role);
        }

        // ── DASHBOARD ─────────────────────────────────────────────
        // Ai cũng xem được dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var totalOrders   = await _context.Orders.CountAsync();
            var totalUsers    = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var revenue = await _context.Orders
                .Where(o => o.Status == "Completed" || o.Status == "Paid")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Dùng CustomerName field trực tiếp trong Order (không join)
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt).Take(8)
                .Select(o => new {
                    o.Id, o.Status, o.TotalAmount, o.CreatedAt,
                    customerName = o.CustomerName ?? "Khách"
                }).ToListAsync();

            var topProducts = await _context.OrderItems
                .GroupBy(x => x.ProductName)
                .Select(g => new { name = g.Key, sold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.sold).Take(5)
                .ToListAsync();

            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync();

            return Ok(new { totalOrders, totalUsers, totalProducts, revenue, recentOrders, topProducts, ordersByStatus });
        }

        // ── PRODUCTS ─────────────────────────────────────────────
        // Admin, Staff xem sản phẩm
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? search, [FromQuery] string? brand,
            [FromQuery] bool? active, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();

            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand == brand);
            if (active.HasValue)
                query = query.Where(p => p.IsActive == active.Value);

            var total = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * size).Take(size)
                .Select(p => new {
                    p.Id, p.Name, p.Brand, p.Price,
                    MainImage = string.IsNullOrEmpty(p.MainImage) ? ""
                        : (p.MainImage.StartsWith("http") || p.MainImage.StartsWith("/"))
                            ? p.MainImage
                            : "/images/" + p.MainImage,
                    p.IsActive, p.SoldCount, p.Rating, p.CategoryId,
                    p.Description,
                    stock = _context.ProductVariants
                        .Where(v => v.ProductId == p.Id).Sum(v => v.Stock)
                }).ToListAsync();

            return Ok(new { total, page, size, products });
        }

        // Chỉ Admin thêm sản phẩm
        [HttpPost("products")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] ProductUpsertDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Brand))
                return BadRequest(new { error = "Tên và hãng là bắt buộc" });

            var product = new Product
            {
                Name = dto.Name, Brand = dto.Brand, Price = dto.Price,
                Description = dto.Description ?? "",
                MainImage = dto.MainImage ?? "",
                CategoryId = dto.CategoryId > 0 ? dto.CategoryId : 1,
                IsActive = dto.IsActive ?? true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm sản phẩm", id = product.Id });
        }

        // Chỉ Admin sửa sản phẩm
        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpsertDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });

            if (!string.IsNullOrWhiteSpace(dto.Name))  product.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Brand)) product.Brand = dto.Brand;
            if (dto.Price > 0)        product.Price = dto.Price;
            if (dto.CategoryId > 0)   product.CategoryId = dto.CategoryId;
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.MainImage != null)   product.MainImage = dto.MainImage;
            if (dto.IsActive.HasValue)   product.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật sản phẩm" });
        }

        // Chỉ Admin toggle ẩn/hiện (KHÔNG xóa thật)
        [HttpPut("products/{id}/toggle")]
        public async Task<IActionResult> ToggleProduct(int id)
        {
            if (!HasRole("Admin")) return Forbid();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });
            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = product.IsActive ? "Đã hiện sản phẩm" : "Đã ẩn sản phẩm", isActive = product.IsActive });
        }

        // ── ORDERS ───────────────────────────────────────────────
        // Admin, Staff, Cashier xem đơn hàng
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin", "Staff", "Cashier")) return Forbid();

            var query = _context.Orders.Include(o => o.Items).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .Select(o => new {
                    o.Id, o.Status, o.TotalAmount, o.CreatedAt,
                    o.Address, o.Province, o.Phone,
                    itemCount = o.Items.Count,
                    // Ưu tiên CustomerName có sẵn trong Order
                    customerName = o.CustomerName ?? "Khách"
                }).ToListAsync();

            return Ok(new { total, page, size, orders });
        }

        // Admin, Staff cập nhật trạng thái đơn
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { error = "Không tìm thấy đơn hàng" });
            var valid = new[] { "Pending", "Confirmed", "Shipping", "Completed", "Cancelled" };
            if (!valid.Contains(dto.Status))
                return BadRequest(new { error = "Trạng thái không hợp lệ" });
            order.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật", order.Status });
        }

        // ── USERS ────────────────────────────────────────────────
        // Chỉ Admin quản lý users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search, [FromQuery] string? role,
            [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin")) return Forbid();

            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .Select(u => new { u.Id, u.Name, u.Email, u.Role, u.Status, u.CreatedAt })
                .ToListAsync();

            return Ok(new { total, page, size, users });
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { error = "Không tìm thấy người dùng" });
            user.Status = dto.Active;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật", user.Status });
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { error = "Không tìm thấy người dùng" });
            var validRoles = new[] { "Admin", "Staff", "Cashier", "Customer" };
            if (!validRoles.Contains(dto.Role)) return BadRequest(new { error = "Role không hợp lệ" });
            user.Role = dto.Role;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật role", user.Role });
        }

        // ── REVIEWS ──────────────────────────────────────────────
        // Admin, Staff xem và xóa review
        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews(
            [FromQuery] int? productId, [FromQuery] int? rating,
            [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();

            var query = _context.Reviews
                .Include(r => r.Product).Include(r => r.User).AsQueryable();
            if (productId.HasValue) query = query.Where(r => r.ProductId == productId.Value);
            if (rating.HasValue)    query = query.Where(r => r.Rating == rating.Value);

            var total = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .Select(r => new {
                    r.Id, r.Rating, r.Comment, r.CreatedAt,
                    userName    = r.User != null ? r.User.Name : "Ẩn danh",
                    productName = r.Product != null ? r.Product.Name : "—",
                    r.ProductId
                }).ToListAsync();

            return Ok(new { total, page, size, reviews });
        }

        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound(new { error = "Không tìm thấy đánh giá" });
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa đánh giá" });
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────
    public class UpdateStatusDto     { public string Status { get; set; } = ""; }
    public class UpdateUserStatusDto { public bool   Active { get; set; } }
    public class UpdateRoleDto       { public string Role   { get; set; } = ""; }
    public class ProductUpsertDto
    {
        public string  Name        { get; set; } = "";
        public string  Brand       { get; set; } = "";
        public decimal Price       { get; set; }
        public int     CategoryId  { get; set; }
        public string? Description { get; set; }
        public string? MainImage   { get; set; }
        public bool?   IsActive    { get; set; }
    }
}
