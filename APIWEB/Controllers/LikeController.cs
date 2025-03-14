using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication9.Models2;

namespace WebApplication9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly DBContextTest2 _context;
        public LikeController(DBContextTest2 context)
        {
            _context = context;
        }
        // Lấy danh sách tất cả Like
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var likes = await _context.Likes.ToListAsync();
            return Ok(likes);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var like = await _context.Likes.FindAsync(id);
            if (like == null)
            {
                return NotFound(new { message = "Không tìm thấy Like." });
            }
            return Ok(like);
        }
        // Thêm lượt Like mới

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(int postId, int regUserId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return BadRequest(new { message = "Bài viết không tồn tại." });
            }
            var regUser = await _context.RegisteredUsers.FindAsync(regUserId);
            if (regUser == null)
            {
                return BadRequest(new { message = "Người dùng không tồn tại." });
            }
            var like = new Like
            {
                PostId = postId,
                RegUserId = regUserId,
            };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm lượt Like thành công", like });
        }
        // Xóa lượt Like
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int postId, int regUserId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.RegUserId == regUserId);

            if (like == null)
            {
                return NotFound(new { message = "Người dùng chưa thích bài đăng này." });
            }
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa lượt Like thành công." });
        }
    }
}
