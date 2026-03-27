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
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(x => x.Email == user.Email))
            {
                return BadRequest("Email already exists");
            }
            if (!Regex.IsMatch(user.Password,
@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$"))
            {
                return BadRequest("Password yếu");
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // đảm bảo không bị null Role
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "User";
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Register success"
            });

        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login(LoginDTO loginUser)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == loginUser.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
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
                audience: _config["Jwt:Audience"], // ⭐ thêm dòng này
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );


            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                role = user.Role,
                name = user.Name
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
    