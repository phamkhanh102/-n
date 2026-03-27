using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Helpers;
using D.A.sneaker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ GET CART
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var cart = await _context.CartItems
            .Include(x => x.Variant).ThenInclude(v => v.Product)
            .Include(x => x.Variant.Size)
            .Include(x => x.Variant.Color)
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return Ok(cart.Select(x => new
        {
            id = x.Id,
            name = x.Variant.Product.Name,
            price = x.Variant.Product.Price,
            size = x.Variant.Size.Number,
            color = x.Variant.Color.Name,
            quantity = x.Quantity,
            image = "/images/" + x.Variant.Product.MainImage
        }));
    }

    // ✅ ADD
    [HttpPost]
    public async Task<IActionResult> Add(AddCartDTO dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var variant = await _context.ProductVariants.FindAsync(dto.VariantId);

        if (variant == null)
            return BadRequest("Variant không tồn tại");

        if (variant.Stock < dto.Quantity)
            return BadRequest("Không đủ hàng");

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.VariantId == dto.VariantId);

        var newQty = (existing?.Quantity ?? 0) + dto.Quantity;

        if (variant.Stock < newQty)
            return BadRequest("Vượt quá số lượng tồn");

        if (existing != null)
        {
            existing.Quantity = newQty;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                VariantId = dto.VariantId,
                Quantity = dto.Quantity
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Added to cart",
            Data = null
        });
    }

    //UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var item = await _context.CartItems
            .Include(x => x.Variant)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (item == null)
            return NotFound();

        if (quantity <= 0)
            return BadRequest("Số lượng không hợp lệ");

        if (item.Variant.Stock < quantity)
            return BadRequest("Không đủ hàng");

        item.Quantity = quantity;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Updated",
            Data = null
        });
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var item = await _context.CartItems
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (item == null)
            return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Deleted",
            Data = null
        });
    }
    [HttpGet("total")]
    public async Task<IActionResult> Total()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var total = await _context.CartItems
            .Include(x => x.Variant).ThenInclude(v => v.Product)
            .Where(x => x.UserId == userId)
            .SumAsync(x => x.Quantity * x.Variant.Product.Price);

        return Ok(total);
    }
}