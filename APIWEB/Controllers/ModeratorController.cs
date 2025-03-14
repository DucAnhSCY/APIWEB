using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModeratorController : ControllerBase
    {
        private readonly DBContextTest _dbContext;

        public ModeratorController(DBContextTest dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var moderators = await _dbContext.Moderators.ToListAsync();
            return Ok(moderators);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var moderator = await _dbContext.Moderators.FindAsync(id);
            if (moderator == null)
            {
                return NotFound(new { message = "Không tìm thấy người quản trị." });
            }
            return Ok(moderator);
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(string username , string email , string password , string status)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Thông tin không được để trống." });
            }
            if (await _dbContext.Moderators.AnyAsync(m => m.Username == username || m.Email == email))
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc email đã tồn tại." });
            }
            var moderator = new Moderator
            {
                Username = username,
                Email = email,
                Password = password,
                Status = status ?? "Active",
                JoinDate = DateTime.Now
            };
            _dbContext.Moderators.Add(moderator);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Thêm thành công quản trị viên", moderator });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Moderator updateModerator)
        {
            var moderator = await _dbContext.Moderators.FindAsync(id);
            if (moderator == null)
            {
                return NotFound(new { message = "Không tìm thấy người quản trị." });
            }
            if (string.IsNullOrEmpty(updateModerator.Username) || string.IsNullOrEmpty(updateModerator.Email) || string.IsNullOrEmpty(updateModerator.Password))
            {
                return BadRequest(new { message = "Thông tin không được để trống." });
            }
            if (await _dbContext.Moderators.AnyAsync(m => (m.Username == updateModerator.Username || m.Email == updateModerator.Email) && m.ModId != id))
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc email đã tồn tại." });
            }
            moderator.Username = updateModerator.Username;
            moderator.Email = updateModerator.Email;
            moderator.Password = updateModerator.Password;
            moderator.Status = updateModerator.Status;
            _dbContext.Moderators.Update(moderator);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin người quản trị thành công.", moderator });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var moderator = await _dbContext.Moderators.FindAsync(id);
            if (moderator == null)
            {
                return NotFound(new { message = "Không tìm thấy người quản trị." });
            }
            _dbContext.Moderators.Remove(moderator);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Xóa người quản trị thành công." });
        }
    }
}
