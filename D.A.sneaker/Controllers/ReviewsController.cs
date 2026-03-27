using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

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
                    UserName = x.User.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId    = userId,
                Rating    = dto.Rating,
                Comment   = dto.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(); // save first

            // Recalculate average from DB
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
    }
}
