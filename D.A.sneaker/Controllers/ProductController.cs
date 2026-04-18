        using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public ProductController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── Helper: lấy map giảm giá từ promotions đang active ──
        private async Task<Dictionary<int, (decimal salePrice, int percent, string promoName)>> GetActivePromoMap()
        {
            try
            {
                var now = DateTime.Now;
                var promos = await _context.Promotions
                    .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                    .ToListAsync();

                if (!promos.Any())
                    return new Dictionary<int, (decimal, int, string)>();

                // Load TẤT CẢ product prices 1 lần (tránh N+1 queries)
                var allPrices = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Id, p.Price })
                    .ToDictionaryAsync(p => p.Id, p => p.Price);

                var map = new Dictionary<int, (decimal salePrice, int percent, string promoName)>();

                foreach (var promo in promos)
                {
                    List<int>? pids = null;
                    if (!string.IsNullOrEmpty(promo.ProductIds))
                    {
                        try { pids = JsonSerializer.Deserialize<List<int>>(promo.ProductIds); }
                        catch { }
                    }

                    // Nếu không chỉ định sản phẩm cụ thể → áp dụng tất cả
                    if (pids == null || !pids.Any())
                        pids = allPrices.Keys.ToList();

                    foreach (var pid in pids)
                    {
                        if (!allPrices.TryGetValue(pid, out var originalPrice))
                            continue;

                        // Chỉ giữ promo có discount cao nhất
                        if (map.ContainsKey(pid) && promo.DiscountPercent <= map[pid].percent)
                            continue;

                        decimal salePrice;
                        int percent = promo.DiscountPercent;
                        if (percent > 0)
                        {
                            salePrice = originalPrice - (originalPrice * percent / 100);
                        }
                        else
                        {
                            salePrice = Math.Max(0, originalPrice - promo.DiscountAmount);
                            percent = originalPrice > 0
                                ? (int)Math.Round((double)((originalPrice - salePrice) / originalPrice) * 100)
                                : 0;
                        }

                        map[pid] = (Math.Round(salePrice, 0), percent, promo.Name);
                    }
                }

                return map;
            }
            catch (Exception ex)
            {
                // Nếu bảng Promotions chưa tồn tại hoặc lỗi → trả map rỗng
                System.Diagnostics.Debug.WriteLine($"[GetActivePromoMap] Error: {ex.Message}");
                return new Dictionary<int, (decimal, int, string)>();
            }
        }

        //--------------------------------------------------
        // GET LIST (SHOP PAGE)
        //--------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
               .Where(p => p.IsActive)
               .Select(p => new ProductCardDto
               {
                   Id = p.Id,
                   Name = p.Name,
                   Brand = p.Brand,
                   Price = p.Price,
                   ImageUrl = string.IsNullOrEmpty(p.MainImage) ? ""
                       : (p.MainImage.StartsWith("http") || p.MainImage.StartsWith("/"))
                           ? p.MainImage
                           : "/images/" + p.MainImage
               })
                .ToListAsync();

            // Áp dụng giá sale từ promotions đang active
            var promoMap = await GetActivePromoMap();
            foreach (var p in products)
            {
                if (promoMap.TryGetValue(p.Id, out var promo))
                {
                    p.SalePrice = promo.salePrice;
                    p.DiscountPercent = promo.percent;
                    p.PromoName = promo.promoName;
                }
            }

            return Ok(products);
        }

        //--------------------------------------------------
        // GET DETAIL (PRODUCT PAGE)  ⭐ QUAN TRỌNG
        //--------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants).ThenInclude(v => v.Size)
                .Include(p => p.Variants).ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var dto = new ProductDetailDTO
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                Description = product.Description,
                MainImage = string.IsNullOrEmpty(product.MainImage) ? ""
                    : (product.MainImage.StartsWith("http") || product.MainImage.StartsWith("/"))
                        ? product.MainImage
                        : "/images/" + product.MainImage,

                Images = product.Images
                    .Where(i => !string.IsNullOrEmpty(i.ImageUrl))
                    .Select(i => (i.ImageUrl.StartsWith("http") || i.ImageUrl.StartsWith("/"))
                        ? i.ImageUrl
                        : "/images/" + i.ImageUrl)
                    .ToList(),

                Variants = product.Variants.Select(v => new VariantDTO
                {
                    VariantId = v.Id,
                    Color = v.Color.Name,
                    Size = v.Size.Number,
                    Stock = v.Stock
                }).ToList()
            };

            // Áp dụng giá sale từ promotions đang active
            var promoMap = await GetActivePromoMap();
            if (promoMap.TryGetValue(id, out var promo))
            {
                dto.SalePrice = promo.salePrice;
                dto.DiscountPercent = promo.percent;
                dto.PromoName = promo.promoName;
            }

            return Ok(dto);
        }
        //--------------------------------------------------
        // SEARCH
        //--------------------------------------------------
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            var query = _context.Products.Where(p => p.IsActive);
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(await query.ToListAsync());

            keyword = keyword.ToLower();
            var products = await query
  .Where(p =>
      p.Name.ToLower().Contains(keyword) ||
      p.Brand.ToLower().Contains(keyword))
  .ToListAsync();

            return Ok(products);
        }
        [HttpGet("advanced")]
        public async Task<IActionResult> Advanced(
    int page = 1,
    int pageSize = 10,
    string? brand = null,
    decimal? min = null,
    decimal? max = null)
        {
            var query = _context.Products.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(x => x.Brand.Contains(brand));

            if (min.HasValue)
                query = query.Where(x => x.Price >= min);

            if (max.HasValue)
                query = query.Where(x => x.Price <= max);

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }
        //--------------------------------------------------
        // CREATE PRODUCT (ADMIN)
        //--------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null)
                return BadRequest();

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        //--------------------------------------------------
        // UPDATE PRODUCT (ADMIN)
        //--------------------------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updated)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Name = updated.Name;
            product.Brand = updated.Brand;
            product.Price = updated.Price;
            product.Description = updated.Description;
            product.MainImage = updated.MainImage;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
    }
}