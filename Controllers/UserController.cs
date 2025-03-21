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
                Status = "Active", // Đảm bảo trạng thái mặc định là Active khi đăng ký
                JoinDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User registered successfully.");
        }

        // ✅ LOGIN and get JWT Token
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = "invalid_model", message = "Invalid model state." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return BadRequest(new { code = "invalid_credentials", message = "Invalid email or password." });
            }

            // Check account status
            if (user.Status == "Inactive")
            {
                return BadRequest(new { code = "account_inactive", message = "Your account is inactive. Please contact an administrator." });
            }
            
            if (user.Status == "Ban")
            {
                return BadRequest(new { code = "account_banned", message = "Your account has been banned. Please contact an administrator for more information." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("status", user.Status) 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7), // Token expires after 7 days
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
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


        // ✅ UPDATE user status (Admin only)
        [HttpPut("UpdateStatus/{id}")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusDTO model)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound("User not found.");

            // Validate status
            if (string.IsNullOrEmpty(model.Status))
                return BadRequest("Status cannot be empty.");

            // Only allow specific status values
            var allowedStatuses = new[] { "Active", "Inactive", "Ban" };
            if (!allowedStatuses.Contains(model.Status))
                return BadRequest("Invalid status. Allowed statuses are: Active, Inactive, Ban.");

            // Update status
            user.Status = model.Status;
            _context.SaveChanges();

            return Ok(new { message = "User status updated successfully", user = new { user.UserId, user.Username, user.Status } });
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
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ✅ Verify Password
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }
}