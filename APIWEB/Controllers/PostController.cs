using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<PostController> _logger;

        public PostController(DBContextTest context, ILogger<PostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy danh sách tất cả bài viết
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all posts");
                
                var posts = await _context.Posts
                    .AsNoTracking()
                    .Include(p => p.Thread)
                    .Include(p => p.User)
                    .Select(p => new PostDTO
                    {
                        PostId = p.PostId,
                        ThreadId = p.ThreadId,
                        UserId = p.UserId,
                        UserName = p.User != null ? p.User.Username : null,
                        Content = p.Content,
                        CreatedAt = p.CreatedAt ?? DateTime.MinValue
                    })
                    .ToListAsync();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách bài viết." });
            }
        }

        // Lấy một bài viết theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting post by ID: {id}");
                
                var post = await _context.Posts
                    .AsNoTracking()
                    .Include(p => p.Thread)
                    .Include(p => p.User)
                    .Where(p => p.PostId == id)
                    .Select(p => new PostDTO
                    {
                        PostId = p.PostId,
                        ThreadId = p.ThreadId,
                        UserId = p.UserId,
                        UserName = p.User != null ? p.User.Username : null,
                        Content = p.Content,
                        CreatedAt = p.CreatedAt ?? DateTime.MinValue
                    })
                    .FirstOrDefaultAsync();

                if (post == null)
                {
                    _logger.LogWarning($"Post with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bài viết." });
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving post with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin bài viết." });
            }
        }

        // Thêm một bài viết
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] PostCreateDTO postCreateDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new post in thread ID: {postCreateDTO.ThreadId}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate thread exists
                var thread = await _context.Threads.FindAsync(postCreateDTO.ThreadId);
                if (thread == null)
                {
                    return BadRequest(new { message = "Chủ đề không tồn tại." });
                }

                // Validate user exists if provided
                if (postCreateDTO.UserId.HasValue)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == postCreateDTO.UserId);
                    if (!userExists)
                    {
                        return BadRequest(new { message = "Người dùng không tồn tại." });
                    }
                }

                var post = new Post
                {
                    ThreadId = postCreateDTO.ThreadId,
                    UserId = postCreateDTO.UserId,
                    Content = postCreateDTO.Content,
                    CreatedAt = DateTime.Now
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Tạo bài viết thành công.",
                    postId = post.PostId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new post");
                return StatusCode(500, new { message = "Lỗi server khi tạo bài viết mới." });
            }
        }

        // Cập nhật một bài viết
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PostUpdateDTO postUpdateDTO)
        {
            try
            {
                _logger.LogInformation($"Updating post with ID: {id}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    _logger.LogWarning($"Update failed: Post with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bài viết." });
                }

                post.Content = postUpdateDTO.Content;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật bài viết thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật bài viết." });
            }
        }

        // Xóa một bài viết
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting post with ID: {id}");
                
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    _logger.LogWarning($"Delete failed: Post with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy bài viết." });
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa bài viết thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting post with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa bài viết." });
            }
        }
    }

    public class PostDTO
    {
        public int PostId { get; set; }
        public int ThreadId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PostCreateDTO
    {
        [Required(ErrorMessage = "ID chủ đề không được để trống")]
        public int ThreadId { get; set; }

        public int? UserId { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;
    }

    public class PostUpdateDTO
    {
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;
    }
}