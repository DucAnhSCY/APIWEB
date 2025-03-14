using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication9.Models2;

namespace WebApplication9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisteredUserController : ControllerBase
    {
        private readonly DBContextTest2 _context;

        public RegisteredUserController(DBContextTest2 context)
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

            if (_context.RegisteredUsers.Any(u => u.Email == user.Email || u.Username == user.Username))
            {
                return BadRequest(new { message = "Email or Username already existed." });
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
                userId = user.RegUserId,  // Add this line
                username = user.Username
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}