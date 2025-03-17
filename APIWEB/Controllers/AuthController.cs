using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DBContextTest context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Đăng ký
        [HttpPost("Register")]
        public async Task<IActionResult> Register(string username, string password, string email)
        {
            try
            {
                _logger.LogInformation($"Registration attempt with username: {username}, email: {email}");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                    return BadRequest("Thông tin không được để trống.");

                if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                    return BadRequest("Tên đăng nhập hoặc email đã tồn tại.");

                var user = new User
                {
                    Username = username,
                    Email = email,
                    Password = password, // TODO: Mã hóa mật khẩu trước khi lưu
                    Role = "User", // Mặc định là User
                    Status = "active",
                    JoinDate = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Đăng ký thành công", 
                    user = new { 
                        userId = user.UserId,
                        username = user.Username,
                        email = user.Email,
                        role = user.Role
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "Lỗi server khi đăng ký." });
            }
        }

        // Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                _logger.LogInformation($"Login attempt with email: {email}");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return BadRequest(new { message = "Email và mật khẩu không được để trống." });

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

                if (user == null)
                    return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

                if (user.Status != "active")
                    return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa hoặc bị cấm." });

                return Ok(new { 
                    message = "Đăng nhập thành công", 
                    user = new { 
                        userId = user.UserId,
                        username = user.Username,
                        email = user.Email,
                        role = user.Role
                    } 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Lỗi server khi đăng nhập." });
            }
        }
    }
}