using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisteredUserController : ControllerBase
    {
        private readonly DBContextTest _context;

        public RegisteredUserController(DBContextTest context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.RegisteredUsers.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.RegisteredUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
            return Ok(user);
        }

        [HttpPost("Insert")]
        public IActionResult Insert([FromBody] RegisteredUser user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((_context.Admins.Any(a => a.Email == user.Email || a.Username == user.Username))|| (_context.RegisteredUsers.Any(u => u.Email == user.Email || u.Username == user.Username)))
            {
                return BadRequest(new { message = "Email or Username already existed. Try again!" });
            }

            user.Status = user.Status ?? "Active";
            user.JoinDate = DateTime.Now;

            _context.RegisteredUsers.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, RegisteredUser user)
        {
            if (id != user.RegUserId)
            {
                return BadRequest(new { message = "ID không khớp." });
            }
            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RegisteredUsers.Any(u => u.RegUserId == id))
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }
                throw;
            }
            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.RegisteredUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
            _context.RegisteredUsers.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công." });
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid login request.");
            }

            var admin = _context.Admins
                .FirstOrDefault(a => a.Email == loginRequest.Email && a.Password == loginRequest.Password);

            if (admin != null)
            {
                return Ok(new
                {
                    message = "Login successful!",
                    isAuthenticated = true,
                    userId = admin.AdminId,
                    username = admin.Username,
                    role = "Admin"
                });
            }

            var user = _context.RegisteredUsers
                .FirstOrDefault(u => u.Email == loginRequest.Email && u.Password == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                message = "Login successful!",
                isAuthenticated = true,
                userId = user.RegUserId,
                username = user.Username,
                role = "RegisteredUser"
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}