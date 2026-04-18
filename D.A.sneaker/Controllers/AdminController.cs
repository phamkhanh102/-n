using D.A.sneaker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

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

            // Tính lợi nhuận: doanh thu - giá nhập
            var completedItems = await _context.OrderItems
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Where(i => i.Order.Status == "Completed" || i.Order.Status == "Paid")
                .ToListAsync();
            var totalCost = completedItems.Sum(i => (i.Variant?.Product?.CostPrice ?? 0) * i.Quantity);
            var profit = revenue - totalCost;
            var profitMargin = revenue > 0 ? Math.Round((double)(profit / revenue) * 100, 1) : 0;

            // Đơn giao thành công
            var totalDelivery = await _context.Orders
                .CountAsync(o => o.Status == "Completed" || o.Status == "Paid" || o.Status == "Shipping");

            // Khách hàng mới (tháng này)
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var newCustomers = await _context.Users
                .CountAsync(u => u.CreatedAt >= startOfMonth);

            // Doanh thu 7 ngày gần nhất
            var last7Days = DateTime.Now.Date.AddDays(-6);
            var dailyRevenue = await _context.Orders
                .Where(o => (o.Status == "Completed" || o.Status == "Paid") && o.CreatedAt >= last7Days)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { date = g.Key, revenue = g.Sum(o => o.TotalAmount), orders = g.Count() })
                .OrderBy(x => x.date)
                .ToListAsync();

            // Fill đủ 7 ngày (kể cả ngày không có đơn)
            var dailyChart = new List<object>();
            for (int i = 0; i < 7; i++)
            {
                var d = last7Days.AddDays(i);
                var entry = dailyRevenue.FirstOrDefault(x => x.date == d);
                dailyChart.Add(new { date = d.ToString("dd/MM"), dayName = d.ToString("ddd"), revenue = entry?.revenue ?? 0, orders = entry?.orders ?? 0 });
            }

            // Doanh thu theo brand (top 5)
            var brandRevenue = await _context.OrderItems
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Where(i => i.Order.Status == "Completed" || i.Order.Status == "Paid")
                .GroupBy(i => i.Variant != null && i.Variant.Product != null ? i.Variant.Product.Brand : "Khác")
                .Select(g => new { brand = g.Key, revenue = g.Sum(i => i.Price * i.Quantity), sold = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.revenue).Take(5)
                .ToListAsync();

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt).Take(8)
                .Select(o => new {
                    o.Id, o.Status, o.TotalAmount, o.CreatedAt,
                    customerName = o.CustomerName ?? "Khách",
                    o.Phone
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

            return Ok(new { totalOrders, totalUsers, totalProducts, revenue, totalCost, profit, profitMargin, totalDelivery, newCustomers, dailyChart, brandRevenue, recentOrders, topProducts, ordersByStatus });
        }

        // ── UPLOAD IMAGE (single) ────────────────────────────────
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (!HasRole("Admin")) return Forbid();
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Vui lòng chọn file ảnh" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { error = "Chỉ hỗ trợ file ảnh: jpg, jpeg, png, webp, gif" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "File ảnh không được lớn hơn 5MB" });

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/images/products/{fileName}";
            return Ok(new { message = "Upload thành công", path = relativePath, fileName });
        }

        // ── UPLOAD MULTIPLE IMAGES ───────────────────────────────
        [HttpPost("upload-images")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            if (!HasRole("Admin")) return Forbid();
            if (files == null || !files.Any())
                return BadRequest(new { error = "Vui lòng chọn ít nhất 1 file" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var results = new List<object>();
            var errors  = new List<string>();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext)) { errors.Add($"{file.FileName}: định dạng không hợp lệ"); continue; }
                if (file.Length > 5 * 1024 * 1024)   { errors.Add($"{file.FileName}: vượt quá 5MB");               continue; }

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                results.Add(new { path = $"/images/products/{fileName}", fileName, originalName = file.FileName });
            }

            return Ok(new { uploaded = results, errors, total = results.Count });
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
                    p.Id, p.Name, p.Brand, p.Price, p.CostPrice,
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
                CostPrice = dto.CostPrice,
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
            if (dto.CostPrice >= 0)   product.CostPrice = dto.CostPrice;
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

        // ── PRODUCT DETAIL ──────────────────────────────────────
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants).ThenInclude(v => v.Color)
                .Include(p => p.Variants).ThenInclude(v => v.Size)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });

            return Ok(new
            {
                product.Id, product.Name, product.Brand, product.Price, product.CostPrice,
                product.CategoryId, product.Description, product.IsActive, product.SoldCount, product.Rating,
                mainImage = string.IsNullOrEmpty(product.MainImage) ? ""
                    : (product.MainImage.StartsWith("http") || product.MainImage.StartsWith("/"))
                        ? product.MainImage : "/images/" + product.MainImage,
                images = product.Images?.Select(i => new {
                    i.Id,
                    imageUrl = string.IsNullOrEmpty(i.ImageUrl) ? ""
                        : (i.ImageUrl.StartsWith("http") || i.ImageUrl.StartsWith("/"))
                            ? i.ImageUrl : "/images/" + i.ImageUrl
                }) ?? Enumerable.Empty<object>(),
                variants = product.Variants?.Select(v => new {
                    v.Id, v.SizeId, v.ColorId, v.Stock,
                    sizeName = v.Size != null ? v.Size.Number.ToString() : "?",
                    colorName = v.Color != null ? v.Color.Name : "?"
                }) ?? Enumerable.Empty<object>(),
                reviews = product.Reviews?.OrderByDescending(r => r.CreatedAt).Take(20).Select(r => new {
                    r.Id, r.Rating, r.Comment, r.CreatedAt,
                    userName = r.User != null ? r.User.Name : "Ẩn danh"
                }) ?? Enumerable.Empty<object>()
            });
        }

        [HttpPost("products/{id}/images")]
        public async Task<IActionResult> AddProductImage(int id, [FromBody] AddImageDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });

            var image = new D.A.sneaker.Models.ProductImage { ProductId = id, ImageUrl = dto.ImageUrl };
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm ảnh", image.Id, image.ImageUrl });
        }

        [HttpDelete("products/{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(int id, int imageId)
        {
            if (!HasRole("Admin")) return Forbid();
            var image = await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);
            if (image == null) return NotFound(new { error = "Không tìm thấy ảnh" });
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa ảnh" });
        }

        [HttpPost("products/{id}/variants")]
        public async Task<IActionResult> AddVariant(int id, [FromBody] VariantDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });

            var variant = new ProductVariant
            {
                ProductId = id, SizeId = dto.SizeId, ColorId = dto.ColorId, Stock = dto.Stock
            };
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm variant", variant.Id });
        }

        // ── BATCH ADD VARIANTS (nhiều size × màu cùng lúc) ──────
        [HttpPost("products/{id}/variants/batch")]
        public async Task<IActionResult> AddVariantBatch(int id, [FromBody] List<VariantDto> items)
        {
            if (!HasRole("Admin")) return Forbid();
            if (items == null || !items.Any())
                return BadRequest(new { error = "Danh sách rỗng" });

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { error = "Không tìm thấy sản phẩm" });

            // Lấy các variant đã tồn tại để tránh trùng
            var existing = await _context.ProductVariants
                .Where(v => v.ProductId == id)
                .Select(v => new { v.SizeId, v.ColorId })
                .ToListAsync();

            var added = 0; var skipped = 0;
            foreach (var item in items)
            {
                if (existing.Any(e => e.SizeId == item.SizeId && e.ColorId == item.ColorId))
                { skipped++; continue; }

                _context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = id, SizeId = item.SizeId, ColorId = item.ColorId, Stock = item.Stock
                });
                added++;
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã thêm {added} variant, bỏ qua {skipped} variant trùng", added, skipped });
        }

        [HttpPut("products/{id}/variants/{variantId}")]
        public async Task<IActionResult> UpdateVariant(int id, int variantId, [FromBody] VariantDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == id);
            if (variant == null) return NotFound(new { error = "Không tìm thấy variant" });
            variant.SizeId = dto.SizeId;
            variant.ColorId = dto.ColorId;
            variant.Stock = dto.Stock;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật variant" });
        }

        [HttpDelete("products/{id}/variants/{variantId}")]
        public async Task<IActionResult> DeleteVariant(int id, int variantId)
        {
            if (!HasRole("Admin")) return Forbid();
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == id);
            if (variant == null) return NotFound(new { error = "Không tìm thấy variant" });
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa variant" });
        }

        // Helper: Get sizes and colors for dropdown
        [HttpGet("sizes-colors")]
        public async Task<IActionResult> GetSizesColors()
        {
            var sizes = await _context.Sizes.OrderBy(s => s.Number).ToListAsync();
            var colors = await _context.Colors.OrderBy(c => c.Name).ToListAsync();
            return Ok(new { sizes, colors });
        }

        // ── ORDERS ───────────────────────────────────────────────
        // Admin, Staff, Cashier xem đơn hàng
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string? status,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            [FromQuery] string? search,
            [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin", "Staff", "Cashier")) return Forbid();

            var query = _context.Orders.Include(o => o.Items).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.CustomerName != null && o.CustomerName.Contains(search)
                    || o.Phone != null && o.Phone.Contains(search));
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var fd))
                query = query.Where(o => o.CreatedAt.Date >= fd.Date);
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var td))
                query = query.Where(o => o.CreatedAt.Date <= td.Date);

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * size).Take(size)
                .Select(o => new {
                    o.Id, o.Status, o.TotalAmount, o.CreatedAt,
                    o.Address, o.Province, o.Phone,
                    itemCount = o.Items.Count,
                    customerName = o.CustomerName ?? "Khách"
                }).ToListAsync();

            return Ok(new { total, page, size, orders });
        }

        // Admin, Staff cập nhật trạng thái đơn
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { error = "Không tìm thấy đơn hàng" });
            var valid = new[] { "Pending", "Confirmed", "Shipping", "Completed", "Cancelled" };
            if (!valid.Contains(dto.Status))
                return BadRequest(new { error = "Trạng thái không hợp lệ" });

            var oldStatus = order.Status;
            var wasCompleted = oldStatus == "Completed" || oldStatus == "Paid";
            var isNowCompleted = dto.Status == "Completed" || dto.Status == "Paid";

            order.Status = dto.Status;

            // Cập nhật SoldCount khi đơn hàng chuyển sang Completed
            if (!wasCompleted && isNowCompleted)
            {
                foreach (var item in order.Items)
                {
                    if (item.Variant?.Product != null)
                    {
                        item.Variant.Product.SoldCount += item.Quantity;
                    }
                }
            }
            // Giảm SoldCount nếu huỷ đơn đã hoàn thành
            else if (wasCompleted && !isNowCompleted)
            {
                foreach (var item in order.Items)
                {
                    if (item.Variant?.Product != null)
                    {
                        item.Variant.Product.SoldCount = Math.Max(0, item.Variant.Product.SoldCount - item.Quantity);
                    }
                }
            }

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

        // ── RESET PASSWORD ────────────────────────────────────────
        [HttpPut("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest(new { error = "Mật khẩu phải có ít nhất 6 ký tự" });

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { error = "Không tìm thấy người dùng" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã đặt lại mật khẩu cho tài khoản #{id}" });
        }

        // ── REVIEWS ──────────────────────────────────────────────
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

        // ── FINANCE ──────────────────────────────────────────────
        [HttpGet("finance")]
        public async Task<IActionResult> GetFinance([FromQuery] int? months)
        {
            if (!HasRole("Admin")) return Forbid();
            var period = months ?? 6;
            var since = DateTime.Now.AddMonths(-period);

            // Lợi nhuận theo sản phẩm
            var productProfit = await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Where(i => (i.Order.Status == "Completed" || i.Order.Status == "Paid") && i.Order.CreatedAt >= since)
                .GroupBy(i => i.ProductName)
                .Select(g => new {
                    name = g.Key,
                    sold = g.Sum(i => i.Quantity),
                    revenue = g.Sum(i => i.Price * i.Quantity),
                    cost = g.Sum(i => (i.Variant != null && i.Variant.Product != null ? i.Variant.Product.CostPrice : 0) * i.Quantity)
                }).OrderByDescending(x => x.revenue).ToListAsync();

            // Doanh thu theo tháng
            var monthlyData = await _context.Orders
                .Where(o => (o.Status == "Completed" || o.Status == "Paid") && o.CreatedAt >= since)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new {
                    year = g.Key.Year, month = g.Key.Month,
                    revenue = g.Sum(o => o.TotalAmount),
                    orders = g.Count()
                }).OrderBy(x => x.year).ThenBy(x => x.month).ToListAsync();

            // Chi phí theo tháng (từ OrderItems)
            var monthlyCost = await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Variant).ThenInclude(v => v.Product)
                .Where(i => (i.Order.Status == "Completed" || i.Order.Status == "Paid") && i.Order.CreatedAt >= since)
                .GroupBy(i => new { i.Order.CreatedAt.Year, i.Order.CreatedAt.Month })
                .Select(g => new {
                    year = g.Key.Year, month = g.Key.Month,
                    cost = g.Sum(i => (i.Variant != null && i.Variant.Product != null ? i.Variant.Product.CostPrice : 0) * i.Quantity)
                }).ToListAsync();

            var totalRevenue = productProfit.Sum(p => p.revenue);
            var totalCost = productProfit.Sum(p => p.cost);
            var totalProfit = totalRevenue - totalCost;
            var margin = totalRevenue > 0 ? Math.Round((double)(totalProfit / totalRevenue) * 100, 1) : 0;

            return Ok(new { totalRevenue, totalCost, totalProfit, margin, productProfit, monthlyData, monthlyCost });
        }

        // ── PROMOTIONS ──────────────────────────────────────────
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions(
            [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            if (!HasRole("Admin", "Staff")) return Forbid();
            var query = _context.Promotions.AsQueryable();
            var total = await query.CountAsync();
            var promotions = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * size).Take(size)
                .ToListAsync();
            return Ok(new { total, page, size, promotions });
        }

        [HttpPost("promotions")]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Tên khuyến mãi là bắt buộc" });

            var promo = new D.A.sneaker.Models.Promotion
            {
                Name = dto.Name,
                Description = dto.Description ?? "",
                DiscountPercent = dto.DiscountPercent,
                DiscountAmount = dto.DiscountAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                BannerImage = dto.BannerImage ?? "",
                ProductIds = dto.ProductIds,
                CreatedAt = DateTime.Now
            };
            _context.Promotions.Add(promo);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã tạo khuyến mãi", id = promo.Id });
        }

        [HttpPut("promotions/{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] PromotionDto dto)
        {
            if (!HasRole("Admin")) return Forbid();
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound(new { error = "Không tìm thấy khuyến mãi" });

            if (!string.IsNullOrWhiteSpace(dto.Name)) promo.Name = dto.Name;
            if (dto.Description != null) promo.Description = dto.Description;
            promo.DiscountPercent = dto.DiscountPercent;
            promo.DiscountAmount = dto.DiscountAmount;
            promo.StartDate = dto.StartDate;
            promo.EndDate = dto.EndDate;
            promo.IsActive = dto.IsActive;
            if (dto.BannerImage != null) promo.BannerImage = dto.BannerImage;
            promo.ProductIds = dto.ProductIds;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật khuyến mãi" });
        }

        [HttpPut("promotions/{id}/toggle")]
        public async Task<IActionResult> TogglePromotion(int id)
        {
            if (!HasRole("Admin")) return Forbid();
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound(new { error = "Không tìm thấy" });
            promo.IsActive = !promo.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { message = promo.IsActive ? "Đã kích hoạt" : "Đã tắt", isActive = promo.IsActive });
        }

        [HttpDelete("promotions/{id}")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            if (!HasRole("Admin")) return Forbid();
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound(new { error = "Không tìm thấy" });
            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xoá khuyến mãi" });
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────
    public class UpdateStatusDto     { public string Status { get; set; } = ""; }
    public class UpdateUserStatusDto { public bool   Active { get; set; } }
    public class UpdateRoleDto       { public string Role   { get; set; } = ""; }
    public class ResetPasswordDto    { public string NewPassword { get; set; } = ""; }
    public class ProductUpsertDto
    {
        public string  Name        { get; set; } = "";
        public string  Brand       { get; set; } = "";
        public decimal Price       { get; set; }
        public decimal CostPrice   { get; set; }
        public int     CategoryId  { get; set; }
        public string? Description { get; set; }
        public string? MainImage   { get; set; }
        public bool?   IsActive    { get; set; }
    }
    public class PromotionDto
    {
        public string  Name            { get; set; } = "";
        public string? Description     { get; set; }
        public int     DiscountPercent { get; set; }
        public decimal DiscountAmount  { get; set; }
        public DateTime StartDate      { get; set; }
        public DateTime EndDate        { get; set; }
        public bool    IsActive        { get; set; } = true;
        public string? BannerImage     { get; set; }
        public string? ProductIds      { get; set; }
    }
    public class AddImageDto
    {
        public string ImageUrl { get; set; } = "";
    }
    public class VariantDto
    {
        public int SizeId  { get; set; }
        public int ColorId { get; set; }
        public int Stock   { get; set; }
    }
}
