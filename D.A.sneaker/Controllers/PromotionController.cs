using D.A.sneaker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    public class PromotionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PromotionController(AppDbContext context) => _context = context;

        /// <summary>
        /// Lấy danh sách khuyến mãi đang active + trong thời hạn (public API)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePromotions()
        {
            var now = DateTime.Now;
            var promos = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.DiscountPercent)
                .ThenByDescending(p => p.DiscountAmount)
                .ToListAsync();

            return Ok(promos);
        }

        /// <summary>
        /// Lấy sản phẩm đang sale kèm thông tin giảm giá
        /// </summary>
        [HttpGet("sale-products")]
        public async Task<IActionResult> GetSaleProducts()
        {
            var now = DateTime.Now;
            var promos = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();

            if (!promos.Any())
                return Ok(new { promotions = new List<object>(), products = new List<object>() });

            // Thu thập tất cả product IDs từ promotions
            var promoProductMap = new Dictionary<int, (int percent, decimal amount, string promoName, DateTime endDate)>();

            foreach (var promo in promos)
            {
                List<int>? pids = null;
                if (!string.IsNullOrEmpty(promo.ProductIds))
                {
                    try { pids = JsonSerializer.Deserialize<List<int>>(promo.ProductIds); }
                    catch { }
                }

                if (pids != null && pids.Any())
                {
                    foreach (var pid in pids)
                    {
                        if (!promoProductMap.ContainsKey(pid) ||
                            promo.DiscountPercent > promoProductMap[pid].percent)
                        {
                            promoProductMap[pid] = (promo.DiscountPercent, promo.DiscountAmount, promo.Name, promo.EndDate);
                        }
                    }
                }
                else
                {
                    // Áp dụng tất cả: lấy tất cả active products
                    var allPids = await _context.Products
                        .Where(p => p.IsActive)
                        .Select(p => p.Id)
                        .ToListAsync();
                    foreach (var pid in allPids)
                    {
                        if (!promoProductMap.ContainsKey(pid) ||
                            promo.DiscountPercent > promoProductMap[pid].percent)
                        {
                            promoProductMap[pid] = (promo.DiscountPercent, promo.DiscountAmount, promo.Name, promo.EndDate);
                        }
                    }
                }
            }

            var productIds = promoProductMap.Keys.ToList();
            var products = await _context.Products
                .Where(p => p.IsActive && productIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Brand,
                    p.Price,
                    mainImage = string.IsNullOrEmpty(p.MainImage) ? ""
                        : (p.MainImage.StartsWith("http") || p.MainImage.StartsWith("/"))
                            ? p.MainImage : "/images/" + p.MainImage,
                    p.Rating,
                    p.SoldCount,
                    reviewCount = _context.Reviews.Count(r => r.ProductId == p.Id)
                })
                .ToListAsync();

            var result = products.Select(p =>
            {
                var promo = promoProductMap[p.Id];
                decimal salePrice;
                if (promo.percent > 0)
                    salePrice = p.Price - (p.Price * promo.percent / 100);
                else
                    salePrice = Math.Max(0, p.Price - promo.amount);

                return new
                {
                    p.Id,
                    p.Name,
                    p.Brand,
                    originalPrice = p.Price,
                    salePrice = Math.Round(salePrice, 0),
                    discountPercent = promo.percent > 0 ? promo.percent
                        : (int)Math.Round((double)((p.Price - salePrice) / p.Price) * 100),
                    promoName = promo.promoName,
                    endDate = promo.endDate,
                    p.mainImage,
                    p.Rating,
                    p.SoldCount,
                    p.reviewCount
                };
            }).OrderByDescending(p => p.discountPercent).ToList();

            return Ok(new { promotions = promos, products = result });
        }
    }
}
