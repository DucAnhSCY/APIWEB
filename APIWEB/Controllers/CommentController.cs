using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<CommentController> _logger;

        public CommentController(DBContextTest context, ILogger<CommentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy danh sách tất cả Comment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all comments");
                
                var comments = await _context.Comments
                    .AsNoTracking()
                    .Include(c => c.Post)
                    .Include(c => c.User)
                    .Select(c => new CommentDTO
                    {
                        CommentId = c.CommentId,
                        PostId = c.PostId,
                        UserId = c.UserId,
                        UserName = c.User.Username,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt ?? DateTime.MinValue
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all comments");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách bình luận." });
            }
        }

        // Lấy Comment theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting comment by ID: {id}");
                
                var comment = await _context.Comments
                    .AsNoTracking()
                    .Include(c => c.Post)
                    .Include(c => c.User)
                    .Where(c => c.CommentId == id)
                    .Select(c => new CommentDTO
                    {
                        CommentId = c.CommentId,
                        PostId = c.PostId,
                        UserId = c.UserId,
                        UserName = c.User.Username,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt ?? DateTime.MinValue
                    })
                    .FirstOrDefaultAsync();

                if (comment == null)
                {
                    _logger.LogWarning($"Comment with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bình luận." });
                }

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving comment with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin bình luận." });
            }
        }

        // Thêm Comment mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] CommentCreateDTO commentCreateDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new comment for post ID: {commentCreateDTO.PostId}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate post exists
                var post = await _context.Posts.FindAsync(commentCreateDTO.PostId);
                if (post == null)
                {
                    return BadRequest(new { message = "Bài viết không tồn tại." });
                }

                // Validate user exists
                var user = await _context.Users.FindAsync(commentCreateDTO.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại." });
                }

                var comment = new Comment
                {
                    PostId = commentCreateDTO.PostId,
                    UserId = commentCreateDTO.UserId,
                    Content = commentCreateDTO.Content,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Tạo bình luận thành công.",
                    commentId = comment.CommentId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new comment");
                return StatusCode(500, new { message = "Lỗi server khi tạo bình luận mới." });
            }
        }

        // Cập nhật Comment
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommentUpdateDTO commentUpdateDTO)
        {
            try
            {
                _logger.LogInformation($"Updating comment with ID: {id}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    _logger.LogWarning($"Update failed: Comment with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bình luận." });
                }

                comment.Content = commentUpdateDTO.Content;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật bình luận thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating comment with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật bình luận." });
            }
        }

        // Xóa Comment
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting comment with ID: {id}");
                
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    _logger.LogWarning($"Delete failed: Comment with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bình luận." });
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa bình luận thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa bình luận." });
            }
        }
    }

    public class CommentDTO
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CommentCreateDTO
    {
        [Required(ErrorMessage = "ID bài viết không được để trống")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "ID người dùng không được để trống")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentUpdateDTO
    {
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;
    }
}
