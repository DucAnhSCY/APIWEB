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
            if ((await _context.RegisteredUsers.AnyAsync(u => u.Username == username || u.Email == email) || (await _context.Admins.AnyAsync(u => u.Username == username || u.Email == email))))
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
        // Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user1 = await _context.Admins.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            if(user1 == null )
            {
                var user = await _context.RegisteredUsers.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
                if (user == null)
                    return BadRequest("Tên đăng nhập hoặc mật khẩu không đúng.");
                return Ok(new { message = "Đăng nhập thành công", user });
            }
            else if(user1 != null)
            {
                return Ok(new { message = "Đăng nhập thành công", user1 });
            }
            else
            {
                return BadRequest("Tên đăng nhập hoặc mật khẩu không đúng.");
            }
        }
    }
}