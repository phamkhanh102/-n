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
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET ALL (Admin) ─────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Reviews
                .Include(x => x.User)
                .Include(x => x.Product)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new {
                    x.Id,
                    x.Rating,
                    x.Comment,
                    x.CreatedAt,
                    userName    = x.User.Name,
                    productName = x.Product.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        // ── GET BY PRODUCT ──────────────────────────────────────
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var data = await _context.Reviews
                .Include(x => x.User)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new {
                    x.Id,
                    x.Rating,
                    x.Comment,
                    x.CreatedAt,
                    userName = x.User.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        // ── CREATE ──────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating phải từ 1 đến 5");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId    = userId,
                Rating    = dto.Rating,
                Comment   = dto.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Tính lại trung bình rating
            var avgRating = await _context.Reviews
                .Where(r => r.ProductId == dto.ProductId)
                .AverageAsync(r => (double)r.Rating);

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product != null)
            {
                product.Rating = avgRating;
                await _context.SaveChangesAsync();
            }

            return Ok(new { review.Id, review.Rating, review.Comment, review.CreatedAt });
        }

        // ── DELETE (Admin) ──────────────────────────────────────
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound(new { error = "Không tìm thấy đánh giá" });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Cập nhật lại avg rating cho sản phẩm
            var productId = review.ProductId;
            var avg = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            var product = await _context.Products.FindAsync(productId);
            if (product != null) { product.Rating = avg; await _context.SaveChangesAsync(); }

            return Ok(new { message = "Đã xóa đánh giá" });
        }
    }
}
