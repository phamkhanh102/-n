using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Helpers;
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
    [Route("api/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var data = await _context.Wishlists
                .Include(x => x.Product)
                .Where(x => x.UserId == userId && x.Product != null)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName  = x.Product != null ? x.Product.Name : "",
                    ProductBrand = x.Product != null ? x.Product.Brand : "",
                    ProductPrice = x.Product != null ? x.Product.Price : 0,
                    ProductImg   = x.Product != null ? x.Product.MainImage : ""
                })
                .ToListAsync();

            var result = data.Select(x => new WishlistItemDTO
            {
                Id        = x.Id,
                ProductId = x.ProductId,
                Product   = new WishlistProductDTO
                {
                    Id        = x.ProductId,
                    Name      = x.ProductName ?? "",
                    Brand     = x.ProductBrand ?? "",
                    Price     = x.ProductPrice,
                    MainImage = string.IsNullOrEmpty(x.ProductImg) ? ""
                              : (x.ProductImg.StartsWith("http") || x.ProductImg.StartsWith("/")) ? x.ProductImg
                              : "/images/" + x.ProductImg
                }
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddWishlistDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var exists = await _context.Wishlists
                .AnyAsync(x => x.UserId == userId && x.ProductId == dto.ProductId);

            if (exists)
                return BadRequest("Đã tồn tại");

            _context.Wishlists.Add(new Wishlist
            {
                UserId = userId,
                ProductId = dto.ProductId
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Added to cart",
                Data = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Wishlists.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
