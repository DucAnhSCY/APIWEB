using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication9.Models2;

namespace WebApplication9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestController : ControllerBase
    {
        private readonly DBContextTest2 _context;

        public GuestController(DBContextTest2 context)
        {
            _context = context;
        }
        // Lấy danh sách tất cả khách
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var guests = await _context.Guests.ToListAsync();
            return Ok(guests);
        }

        // Lấy một khách theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound(new { message = "Không tìm thấy khách." });
            }
            return Ok(guest);
        }

        // Thêm một khách
        [HttpPost("Insert")]
        public IActionResult Insert(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest("Thông tin không được để trống.");
            var guest = new Guest
            {
                Username = username
            };
            _context.Guests.Add(guest);
            _context.SaveChanges();
            return Ok(guest);
        }

        // Xóa khách
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound(new { message = "Không tìm thấy khách." });
            }
            _context.Guests.Remove(guest);
            _context.SaveChanges();
            return Ok(new { message = "Xóa khách thành công." });
        }
    }
}
