using CozyComfort.Data.Context;
using CozyComfort.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CozyComfort.API.DTOs;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // email already exists check
                var existingUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "Email already registered" });

                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

                    // role string -> enum convert
                    Role = Enum.TryParse<UserRole>(request.Role, true, out var r)
                        ? r
                        : UserRole.User
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return Ok(new
                {
                    message = "User Registered",
                    email = user.Email,
                    role = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid credentials" });

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Login Success",
                    role = user.Role.ToString(),
                    token = token,
                    userId = user.UserId,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ================= JWT TOKEN =================
        private string GenerateJwtToken(User user)
        {
            // config eke key thiyenawanam eka use karanawa
            var secretKey = _config["Jwt:Key"]
                ?? "SuperSecretKey123!SuperSecretKey123!"; // fallback 32+ chars

            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),

                Expires = DateTime.UtcNow.AddHours(2),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // ================= LOGIN REQUEST DTO =================
        public class LoginRequest
        {
            [Required]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }
    }
}
