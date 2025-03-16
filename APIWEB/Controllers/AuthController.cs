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
        public AuthController(DBContextTest context)
        {
            _context = context;
        }

        // Đăng ký
        [HttpPost("Register")]
        public async Task<IActionResult> Register(string username, string password, string email)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                return BadRequest("Thông tin không được để trống.");
            if (await _context.RegisteredUsers.AnyAsync(u => u.Username == username || u.Email == email))
                return BadRequest("Tên đăng nhập hoặc email đã tồn tại.");

            var user = new RegisteredUser
            {
                Username = username,
                Email = email,
                Password = password, // TODO: Mã hóa mật khẩu trước khi lưu
                Status = "Active",
                JoinDate = DateTime.Now
            };
            _context.RegisteredUsers.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký thành công", user });
        }



        // Đăng nhập Admin
        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin(string adminEmail, string adminPassword)
        {
            // Kiểm tra xem admin có tồn tại không
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == adminEmail && a.Password == adminPassword);
            if (admin == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
            return Ok(new { message = "Đăng nhập thành công", admin });
        }

        // Đăng nhập Moderator
        [HttpPost("ModeratorLogin")]
        public async Task<IActionResult> ModeratorLogin(string moderatorEmail, string moderatorPassword)
        {
            // Kiểm tra xem moderator có tồn tại không
            var moderator = await _context.Moderators.FirstOrDefaultAsync(m =>
                m.Email == moderatorEmail && m.Password == moderatorPassword);

            if (moderator == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

            if (moderator.Status != "Active")
                return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên." });

            // Trả về thông tin moderator (không bao gồm mật khẩu)
            var moderatorResponse = new
            {
                moderator.ModId,
                moderator.Username,
                moderator.Email,
                moderator.Status,
                moderator.JoinDate,
                Role = "Moderator"
            };

            return Ok(new { message = "Đăng nhập thành công", moderator = moderatorResponse });
        }

        // Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.RegisteredUsers.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            if (user == null)
                return BadRequest("Tên đăng nhập hoặc mật khẩu không đúng.");
            return Ok(new { message = "Đăng nhập thành công", user });
        }
    }
}