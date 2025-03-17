using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<LikeController> _logger;

        public LikeController(DBContextTest context, ILogger<LikeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy danh sách tất cả Like
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all likes");
                
                var likes = await _context.Likes
                    .AsNoTracking()
                    .Include(l => l.Post)
                    .Include(l => l.User)
                    .Select(l => new LikeDTO
                    {
                        LikeId = l.LikeId,
                        PostId = l.PostId,
                        UserId = l.UserId,
                        UserName = l.User.Username
                    })
                    .ToListAsync();

                return Ok(likes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all likes");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách lượt thích." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting like by ID: {id}");
                
                var like = await _context.Likes
                    .AsNoTracking()
                    .Include(l => l.Post)
                    .Include(l => l.User)
                    .Where(l => l.LikeId == id)
                    .Select(l => new LikeDTO
                    {
                        LikeId = l.LikeId,
                        PostId = l.PostId,
                        UserId = l.UserId,
                        UserName = l.User.Username
                    })
                    .FirstOrDefaultAsync();

                if (like == null)
                {
                    _logger.LogWarning($"Like with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy lượt thích." });
                }

                return Ok(like);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving like with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin lượt thích." });
            }
        }

        // Thêm lượt Like mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] LikeCreateDTO likeCreateDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new like for post ID: {likeCreateDTO.PostId} by user ID: {likeCreateDTO.UserId}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if post exists
                var post = await _context.Posts.FindAsync(likeCreateDTO.PostId);
                if (post == null)
                {
                    return BadRequest(new { message = "Bài viết không tồn tại." });
                }

                // Check if user exists
                var user = await _context.Users.FindAsync(likeCreateDTO.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại." });
                }

                // Check if like already exists
                var existingLike = await _context.Likes
                    .AnyAsync(l => l.PostId == likeCreateDTO.PostId && l.UserId == likeCreateDTO.UserId);
                
                if (existingLike)
                {
                    return BadRequest(new { message = "Người dùng đã thích bài viết này." });
                }

                var like = new Like
                {
                    PostId = likeCreateDTO.PostId,
                    UserId = likeCreateDTO.UserId
                };

                _context.Likes.Add(like);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Thêm lượt thích thành công.",
                    likeId = like.LikeId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new like");
                return StatusCode(500, new { message = "Lỗi server khi thêm lượt thích." });
            }
        }

        // Xóa lượt Like
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] int postId, [FromQuery] int userId)
        {
            try
            {
                _logger.LogInformation($"Deleting like for post ID: {postId} by user ID: {userId}");
                
                var like = await _context.Likes
                    .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
                
                if (like == null)
                {
                    _logger.LogWarning($"Delete failed: Like not found for post ID {postId} and user ID {userId}");
                    return NotFound(new { message = "Không tìm thấy lượt thích." });
                }

                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa lượt thích thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting like for post ID {postId} and user ID {userId}");
                return StatusCode(500, new { message = "Lỗi server khi xóa lượt thích." });
            }
        }
    }

    public class LikeDTO
    {
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class LikeCreateDTO
    {
        [Required(ErrorMessage = "ID bài viết không được để trống")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "ID người dùng không được để trống")]
        public int UserId { get; set; }
    }
}
