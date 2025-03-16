using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DBContextTest _context;

        public PostController(DBContextTest context)
        {
            _context = context;
        }
        // Lấy danh sách tất cả bài viết
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts.ToListAsync();
            return Ok(posts);
        }
        // Lấy một bài viết theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            return Ok(post);
        }
        // Thêm một bài viết
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(int ThreadId, int RegUserId, int ModId, string Content)
        {
            if (string.IsNullOrEmpty(Content))
            {
                return BadRequest(new { message = "Nội dung không được để trống." });
            }
            var thread = await _context.Posts.FindAsync(ThreadId);
            if (thread == null)
            {
                return BadRequest(new { message = "Chủ đề không tồn tại." });
            }

            if (RegUserId != null && ModId != null)
            {
                return BadRequest(new { message = "Bài viết không thể có cả hai người viết và người quản trị." });
            }

            if (RegUserId != null)
            {
                var regUser = await _context.RegisteredUsers.FindAsync(RegUserId);
                if (regUser == null)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại." });
                }
            }
            else if (ModId != null)
            {
                var mod = await _context.Moderators.FindAsync(ModId);
                if (mod == null)
                {
                    return BadRequest(new { message = "Người quản trị không tồn tại." });
                }
            }
            else
            {
                return BadRequest(new { message = "Bài viết phải có người viết hoặc người quản trị." });
            }

            var post = new Post
            {
                ThreadId = ThreadId,
                RegUserId = RegUserId,
                ModId = ModId,
                Content = Content,
                CreatedAt = DateTime.Now
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return Ok(post);
        }
        //Xóa một bài viết
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa bài viết thành công." });
        }
    }
}