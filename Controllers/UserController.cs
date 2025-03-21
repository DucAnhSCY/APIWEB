using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using diendan2.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;
using Microsoft.AspNetCore.Identity.Data;

namespace diendan.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DBContextTest2 _context;
        private readonly IConfiguration _config;

        public UserController(DBContextTest2 context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ✅ REGISTER a new user
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterDTO model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
                return BadRequest("Email is already taken.");

            string hashedPassword = HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "User", // Default role is "User"
                Status = "active",
                JoinDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User registered successfully.");
        }

        // ✅ LOGIN and get JWT Token
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return BadRequest("Invalid email or password");
            }

            if (user.Status.ToLower() == "ban")
            {
                return BadRequest("Your account has been banned. Please contact support.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Username, user.Role } });
        }


        // ✅ GET all users (Admin only)
        [HttpGet("GetAll")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.Status,
                    u.JoinDate
                })
                .ToList();

            return Ok(users);
        }

        // ✅ GET user profile (Authenticated users only)
        [HttpGet("Profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
                return Unauthorized("Invalid user token.");

            var user = _context.Users.Find(int.Parse(userId));
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { user.UserId, user.Username, user.Email, user.Role, user.JoinDate });
        }

        // ✅ UPDATE user role (Admin only)
        [HttpPut("UpdateUser/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDTO model)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound("User not found.");

            // Update fields if provided
            if (!string.IsNullOrEmpty(model.Role))
            {
                var allowedRoles = new[] { "User", "Admin", "Moderator" };
                if (!allowedRoles.Contains(model.Role))
                    return BadRequest("Invalid role. Allowed roles: User, Admin, Moderator.");

                user.Role = model.Role;
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                var allowedStatuses = new[] { "Active", "Inactive", "Ban" };
                if (!allowedStatuses.Contains(model.Status))
                    return BadRequest("Invalid status. Allowed statuses: Active, Inactive, Ban.");

                user.Status = model.Status;
            }

            _context.SaveChanges();
            return Ok(new { message = "User updated successfully", user = new { user.UserId, user.Username, user.Role, user.Status } });
        }


        // ✅ DELETE user (Admin only)
        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok("User deleted successfully.");
        }

        // ✅ JWT Token Generation
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token); 
        }

        // ✅ Hash Password using Bcrypt
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // ✅ Verify Password
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }
    }
}