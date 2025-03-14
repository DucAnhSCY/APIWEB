using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication9.Models2;

namespace WebApplication9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly DBContextTest2 _context;


        public CommentController(DBContextTest2 context)
        {
            _context = context;
        }
        // Lấy danh sách tất cả Comment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _context.Comments.ToListAsync();
            return Ok(comments);
        }
        // Lấy Comment theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = "Không tìm thấy bình luận." });
            }
            return Ok(comment);
        }
        // Thêm Comment mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(int postId, int regUserid, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest(new { message = "Nội dung không được để trống." });
            }
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return BadRequest(new { message = "Bài viết không tồn tại." });
            }
            var regUser = await _context.RegisteredUsers.FindAsync(regUserid);
            if (regUser == null)
            {
                return BadRequest(new { message = "Người dùng không tồn tại." });
            }
            var comment = new Comment
            {
                PostId = postId,
                RegUserId = regUserid,
                Content = content,
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm bình luận thành công", comment });
        }
        // Cập nhật Comment
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, string content)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = "Không tìm thấy bình luận." });
            }
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest(new { message = "Nội dung không được để trống." });
            }
            comment.Content = content;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật bình luận thành công", comment });
        }
        // Xóa Comment
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new { message = "Không tìm thấy bình luận." });
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa bình luận thành công" });
        }
    }
}
