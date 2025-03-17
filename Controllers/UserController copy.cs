using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using diendan2.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;
using System.ComponentModel.DataAnnotations;

namespace diendan.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DBContextTest2 _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;

        public UserController(DBContextTest2 context, IConfiguration config, ILogger<UserController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // ✅ REGISTER a new user
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                // Validate password complexity
                if (!IsPasswordValid(model.Password))
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character." });
                }

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    return BadRequest(new { message = "Email is already taken." });

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    return BadRequest(new { message = "Username is already taken." });

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "User",
                    Status = "active",
                    JoinDate = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New user registered: {user.Username}");
                return Ok(new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        // ✅ LOGIN and get JWT Token
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogWarning($"Failed login attempt for email: {request.Email}");
                    return BadRequest(new { message = "Invalid email or password" });
                }

                if (user.Status != "active")
                {
                    return BadRequest(new { message = "Account is not active." });
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation($"User logged in successfully: {user.Username}");
                return Ok(new { token, user = new { user.Username, user.Role } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        // ✅ GET all users (Admin only)
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.Role,
                        u.Status,
                        u.JoinDate
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { message = "An error occurred while retrieving users." });
            }
        }

        // ✅ GET user profile (Authenticated users only)
        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (userId == null)
                    return Unauthorized(new { message = "Invalid user token." });

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                    return NotFound(new { message = "User not found." });

                return Ok(new { user.UserId, user.Username, user.Email, user.Role, user.JoinDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { message = "An error occurred while retrieving profile." });
            }
        }

        // ✅ UPDATE user role (Admin only)
        [HttpPut("UpdateRole/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDTO model)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                if (string.IsNullOrEmpty(model.Role))
                    return BadRequest(new { message = "Role cannot be empty." });

                var allowedRoles = new[] { "User", "Admin", "Moderator" };
                if (!allowedRoles.Contains(model.Role))
                    return BadRequest(new { message = "Invalid role. Allowed roles are: User, Admin, Moderator." });

                user.Role = model.Role;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User role updated: {user.Username} -> {model.Role}");
                return Ok(new { message = "User role updated successfully", user = new { user.UserId, user.Username, user.Role } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role");
                return StatusCode(500, new { message = "An error occurred while updating role." });
            }
        }

        // ✅ DELETE user (Admin only)
        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User deleted: {user.Username}");
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new { message = "An error occurred while deleting user." });
            }
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

        // ✅ Password validation
        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
} 