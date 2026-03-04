using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        //--------------------------------------------------
        // GET LIST (SHOP PAGE)
        //--------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Price = p.Price,
                    ImageUrl = p.MainImage   
                })
                .ToListAsync();

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
                MainImage = product.MainImage,

                Images = product.Images
    .Where(i => i.ImageUrl != product.MainImage) // tránh trùng ảnh chính
    .Select(i => i.ImageUrl)
    .ToList(),

                Variants = product.Variants.Select(v => new VariantDTO
                {
                    VariantId = v.Id,
                    Color = v.Color.Name,
                    Size = v.Size.Number,
                    Stock = v.Stock
                }).ToList()
            };

            return Ok(dto);
        }
        //--------------------------------------------------
        // SEARCH
        //--------------------------------------------------
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(await _context.Products.ToListAsync());

            keyword = keyword.ToLower();

            var products = await _context.Products
    .Include(p => p.Images)
    .Include(p => p.Variants)
        .ThenInclude(v => v.Size)
    .Include(p => p.Variants)
        .ThenInclude(v => v.Color)
    .ToListAsync();

            return Ok(products);
        }
    }
}