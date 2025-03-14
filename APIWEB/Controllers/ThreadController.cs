using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly DBContextTest _dbContext;

        public ThreadController(DBContextTest dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var threads = await _dbContext.Threads
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.RegUser) // Include the user who created the thread
                .Select(t => new ThreadDTO
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Content = t.Content,
                    CategoryId = t.CategoryId ?? 0,
                    CategoryName = t.Category.Name,
                    RegUserId = t.RegUserId,
                    CreatorName = t.RegUser.Username, // Fetch creator's name
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue
                })
                .ToListAsync();

            return Ok(threads);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var thread = await _dbContext.Threads
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.ThreadId == id)
                .Select(t => new ThreadDTO
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Content = t.Content,
                    CategoryId = t.CategoryId ?? 0,
                    CategoryName = t.Category.Name,
                    RegUserId = t.RegUserId,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue
                })
                .FirstOrDefaultAsync();

            if (thread == null)
            {
                return NotFound(new { message = "Không tìm thấy Thread." });
            }

            return Ok(thread);
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] ThreadCreateDTO threadCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _dbContext.Categories.FindAsync(threadCreateDTO.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Lỗi: Danh mục không tồn tại. Vui lòng chọn danh mục hợp lệ." });
            }

            if (threadCreateDTO.RegUserId == null)
            {
                return BadRequest(new { message = "Thread phải có một người tạo (RegisteredUser)." });
            }

            var thread = new Models.Thread
            {
                Title = threadCreateDTO.Title,
                Content = threadCreateDTO.Content,
                CategoryId = threadCreateDTO.CategoryId,
                RegUserId = threadCreateDTO.RegUserId,
                CreatedAt = DateTime.Now
            };

            _dbContext.Threads.Add(thread);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm Thread thành công!",
                thread = new ThreadDTO
                {
                    ThreadId = thread.ThreadId,
                    Title = thread.Title,
                    Content = thread.Content,
                    CategoryId = thread.CategoryId ?? 0,
                    CategoryName = category.Name,
                    RegUserId = thread.RegUserId,
                    CreatedAt = thread.CreatedAt ?? DateTime.MinValue
                }
            });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ThreadUpdateDTO threadUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var thread = await _dbContext.Threads.FindAsync(id);
            if (thread == null)
            {
                return NotFound(new { message = "Không tìm thấy Thread." });
            }

            thread.Title = threadUpdateDTO.Title;
            thread.Content = threadUpdateDTO.Content;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật Thread thành công!", thread });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var thread = await _dbContext.Threads.FindAsync(id);
            if (thread == null)
            {
                return NotFound(new { message = "Không tìm thấy Thread." });
            }

            _dbContext.Threads.Remove(thread);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Xóa Thread thành công." });
        }
    }

    public class ThreadDTO
    {
        public int ThreadId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? RegUserId { get; set; }
        public string CreatorName { get; set; } // Add this field
        public DateTime CreatedAt { get; set; }

    }


    public class ThreadCreateDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? RegUserId { get; set; }
    }

    public class ThreadUpdateDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
