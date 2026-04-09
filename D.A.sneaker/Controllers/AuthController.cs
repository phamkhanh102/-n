using D.A.sneaker.Data;
using D.A.sneaker.DTOs;
using D.A.sneaker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDTO dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { error = "Vui lòng điền đầy đủ thông tin." });
            }

            if (_context.Users.Any(x => x.Email == dto.Email))
            {
                return BadRequest(new { error = "Email này đã được sử dụng." });
            }

            // Kiểm tra username trùng
            if (!string.IsNullOrWhiteSpace(dto.Username) &&
                _context.Users.Any(x => x.Username == dto.Username))
            {
                return BadRequest(new { error = "Tên đăng nhập này đã được sử dụng." });
            }

            if (!Regex.IsMatch(dto.Password,
                @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$"))
            {
                return BadRequest(new { error = "Mật khẩu phải có ít nhất 6 ký tự, gồm chữ hoa, chữ thường và số." });
            }

            var user = new User
            {
                Name = dto.Name,
                Username = dto.Username ?? dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer",
                Status = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges(); // lưu user trước để có user.Id

            // ⭐ TẠO CUSTOMER RECORD NGAY KHI ĐĂNG KÝ ⭐
            // Đây là bước bắt buộc để đặt hàng hoạt động
            var customer = new Customer
            {
                UserId    = user.Id,
                Address   = "",
                City      = "",
                District  = "",
                Ward      = "",
                CreatedAt = DateTime.Now
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginUser)
        {
            var identifier = loginUser.Identifier?.Trim() ?? "";

            // Tìm user theo Email HOẶC Username
            var user = _context.Users
                .FirstOrDefault(x => x.Email == identifier || x.Username == identifier);

            if (user == null)
            {
                return Unauthorized(new { error = $"Tên đăng nhập hoặc email '{identifier}' không tồn tại." });
            }

            if (!BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
            {
                return Unauthorized(new { error = "Mật khẩu không đúng." });
            }


            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role ?? "User") //  cực quan trọng
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"], // thêm dòng này
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );


            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            var customer = _context.Customers.FirstOrDefault(c => c.UserId == user.Id);

            return Ok(new
            {
                token = jwt,
                id    = user.Id,
                name  = user.Name,
                email = user.Email,
                role  = user.Role,
                customerId = customer?.Id ?? 0
            });

        }
        [Authorize(Roles = "Admin")]

        [HttpGet("secret")]
        public IActionResult Secret()
        {
            return Ok("Bạn đã đăng nhập!");
        }

    }
}
    