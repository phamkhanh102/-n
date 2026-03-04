using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using D.A.sneaker.Data;
using D.A.sneaker.DTOs;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // UPDATE ADDRESS
        [HttpPut("{id}/address")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Không tìm thấy đơn");

            // ⭐ NGHIỆP VỤ QUAN TRỌNG
            if (order.Status != "Pending")
                return BadRequest("Đơn hàng đã được xử lý, không thể đổi địa chỉ");

            //order.Address = dto.Address;
            order.Ward = dto.Ward;
            order.District = dto.District;
            order.Province = dto.Province;

            await _context.SaveChangesAsync();

            return Ok("Đã cập nhật địa chỉ");
        }
    }
}