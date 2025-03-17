using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using APIWEB.Models;
namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly DBContextTest _dbContext;
        private readonly ILogger<ThreadController> _logger;

        public ThreadController(DBContextTest dbContext, ILogger<ThreadController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all threads");
                
                var threads = await _dbContext.Threads
                    .AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.User) // Include the user who created the thread
                    .Select(t => new ThreadDTO
                    {
                        ThreadId = t.ThreadId,
                        Title = t.Title,
                        Content = t.Content,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        UserId = t.UserId,
                        CreatorName = t.User.Username, // Fetch creator's name
                        CreatorRole = t.User.Role, // Include user role
                        CreatedAt = t.CreatedAt ?? DateTime.MinValue
                    })
                    .ToListAsync();

                return Ok(threads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all threads");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách chủ đề." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting thread by ID: {id}");
                
                var thread = await _dbContext.Threads
                    .AsNoTracking()
                    .Include(t => t.Category)
                    .Include(t => t.User)
                    .Where(t => t.ThreadId == id)
                    .Select(t => new ThreadDTO
                    {
                        ThreadId = t.ThreadId,
                        Title = t.Title,
                        Content = t.Content,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        UserId = t.UserId,
                        CreatorName = t.User.Username,
                        CreatorRole = t.User.Role,
                        CreatedAt = t.CreatedAt ?? DateTime.MinValue
                    })
                    .FirstOrDefaultAsync();

                if (thread == null)
                {
                    _logger.LogWarning($"Thread with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy chủ đề." });
                }

                return Ok(thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving thread with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin chủ đề." });
            }
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] ThreadCreateDTO threadCreateDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new thread: {threadCreateDTO.Title}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category exists
                var categoryExists = await _dbContext.Categories.AnyAsync(c => c.CategoryId == threadCreateDTO.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Danh mục không tồn tại." });
                }

                // Validate user exists
                var userExists = await _dbContext.Users.AnyAsync(u => u.UserId == threadCreateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại." });
                }

                var thread = new Thread
                {
                    Title = threadCreateDTO.Title,
                    Content = threadCreateDTO.Content,
                    CategoryId = threadCreateDTO.CategoryId,
                    UserId = threadCreateDTO.UserId,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Threads.Add(thread);
                await _dbContext.SaveChangesAsync();

                return Ok(new { 
                    message = "Tạo chủ đề thành công.",
                    threadId = thread.ThreadId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new thread");
                return StatusCode(500, new { message = "Lỗi server khi tạo chủ đề mới." });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ThreadUpdateDTO threadUpdateDTO)
        {
            try
            {
                _logger.LogInformation($"Updating thread with ID: {id}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var thread = await _dbContext.Threads.FindAsync(id);
                if (thread == null)
                {
                    _logger.LogWarning($"Update failed: Thread with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy chủ đề." });
                }

                // Validate category exists if it's being updated
                if (threadUpdateDTO.CategoryId != thread.CategoryId)
                {
                    var categoryExists = await _dbContext.Categories.AnyAsync(c => c.CategoryId == threadUpdateDTO.CategoryId);
                    if (!categoryExists)
                    {
                        return BadRequest(new { message = "Danh mục không tồn tại." });
                    }
                }

                thread.Title = threadUpdateDTO.Title;
                thread.Content = threadUpdateDTO.Content;
                thread.CategoryId = threadUpdateDTO.CategoryId;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Cập nhật chủ đề thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating thread with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật chủ đề." });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting thread with ID: {id}");
                
                var thread = await _dbContext.Threads.FindAsync(id);
                if (thread == null)
                {
                    _logger.LogWarning($"Delete failed: Thread with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy chủ đề." });
                }

                _dbContext.Threads.Remove(thread);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Xóa chủ đề thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting thread with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa chủ đề." });
            }
        }
    }

    public class ThreadDTO
    {
        public int ThreadId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ThreadCreateDTO
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "ID người dùng không được để trống")]
        public int UserId { get; set; }
    }

    public class ThreadUpdateDTO
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục không được để trống")]
        public int CategoryId { get; set; }
    }
}
