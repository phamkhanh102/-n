        using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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
                   ImageUrl = "/images/" + p.MainImage
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
                MainImage = "/images/" + product.MainImage,

                Images = product.Images
    .Select(i => "/images/" + i.ImageUrl)
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
            var query = _context.Products.AsQueryable();

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