using diendan2.Models2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
[Route("api/[controller]")]
[ApiController]
public class ThreadController : ControllerBase
{
    private readonly DBContextTest2 _context;
    private readonly ILogger<ThreadController> _logger;

    public ThreadController(DBContextTest2 context, ILogger<ThreadController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get all threads with pagination
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ThreadDTO>>> GetAllThreads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            var query = _context.Threads
                .Include(t => t.Category)
                .Include(t => t.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Title.Contains(searchTerm) || t.Content.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            // Get total count
            var totalItems = await query.CountAsync();

            // Apply pagination
            var threads = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new ThreadDTO
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue,
                    CategoryId = t.CategoryId,
                    UserId = t.UserId,
                    CategoryName = t.Category.Name,
                    Username = t.User.Username
                })
                .ToListAsync();

            var response = new PaginatedResponse<ThreadDTO>
            {
                Items = threads,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving threads");
            return StatusCode(500, new { message = "An error occurred while retrieving threads." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ThreadDTO>> GetThreadById(int id)
    {
        try
        {
            var thread = await _context.Threads
                .Include(t => t.Category)
                .Include(t => t.User)
                .Where(t => t.ThreadId == id)
                .Select(t => new ThreadDTO
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue,
                    CategoryId = t.CategoryId,
                    UserId = t.UserId,
                    CategoryName = t.Category.Name,
                    Username = t.User.Username
                })
                .FirstOrDefaultAsync();

            if (thread == null)
            {
                return NotFound(new { message = "Thread not found." });
            }

            return Ok(thread);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thread {ThreadId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the thread." });
        }
    }

    // Create a new thread
    [HttpPost("Insert")]
    [Authorize]
    public async Task<ActionResult<ThreadDTO>> InsertThread(ThreadDTO threadDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(threadDTO.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid User ID" });
            }

            var category = await _context.Categories.FindAsync(threadDTO.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Invalid Category ID" });
            }

            var thread = new diendan2.Models2.Thread
            {
                Title = threadDTO.Title,
                Content = threadDTO.Content,
                CreatedAt = DateTime.UtcNow,
                CategoryId = threadDTO.CategoryId,
                UserId = threadDTO.UserId,
            };

            await _context.Threads.AddAsync(thread);
            await _context.SaveChangesAsync();

            threadDTO.ThreadId = thread.ThreadId;
            threadDTO.CategoryName = category.Name;
            threadDTO.Username = user.Username;

            _logger.LogInformation($"New thread created: {thread.Title} by user {user.Username}");
            return CreatedAtAction(nameof(GetThreadById), new { id = thread.ThreadId }, threadDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thread");
            return StatusCode(500, new { message = "An error occurred while creating the thread." });
        }
    }

    // Update a thread
    [HttpPut("Update/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateThread(int id, [FromBody] ThreadUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var thread = await _context.Threads.FindAsync(id);
            if (thread == null)
            {
                return NotFound(new { message = "Thread not found." });
            }

            // Check if user is authorized to update the thread
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null || int.Parse(userId) != thread.UserId)
            {
                return Forbid();
            }

            thread.Title = dto.Title;
            thread.Content = dto.Content;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Thread {id} updated by user {userId}");
            return Ok(new { message = "Thread updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating thread {ThreadId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the thread." });
        }
    }

    // Delete a thread
    [HttpDelete("Delete/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteThread(int id)
    {
        try
        {
            var thread = await _context.Threads.FindAsync(id);
            if (thread == null)
            {
                return NotFound(new { message = "Thread not found." });
            }

            // Check if user is authorized to delete the thread
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst("Role")?.Value;
            if (userId == null || (int.Parse(userId) != thread.UserId && userRole != "Admin"))
            {
                return Forbid();
            }

            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Thread {id} deleted by user {userId}");
            return Ok(new { message = "Thread deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting thread {ThreadId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the thread." });
        }
    }
}

public class ThreadDTO
{
    public int ThreadId { get; set; }
    
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Title { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public string CategoryName { get; set; }
    public string Username { get; set; }
}

public class ThreadUpdateDto
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Title { get; set; }
    
    [Required]
    public string Content { get; set; }
}

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
} 