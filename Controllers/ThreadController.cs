using diendan2.Models2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ThreadController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public ThreadController(DBContextTest2 context)
    {
        _context = context;
    }

    // Get all threads
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ThreadDTO>>> GetAllThreads()
    {
        var threads = await _context.Threads
            .Include(t => t.Category)
            .Include(t => t.User)
            .Select(t => new ThreadDTO
            {
                ThreadId = t.ThreadId,
                Title = t.Title,
                Content = t.Content,
                CreatedAt = t.CreatedAt ?? DateTime.MinValue,
                CategoryId = t.CategoryId,
                UserId = t.UserId,
                CategoryName = t.Category.Name,
                Username = t.User.Username // ✅ Retrieve Username

            })
            .ToListAsync();

        return Ok(threads);
    }

    // Get threads by category ID
    [HttpGet("ByCategory/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ThreadDTO>>> GetThreadsByCategory(int categoryId)
    {
        // Check if category exists
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return NotFound(new { message = "Category not found" });
        }

        var threads = await _context.Threads
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => t.CategoryId == categoryId)
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

        return Ok(threads);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ThreadDTO>> GetThreadById(int id)
    {
        var thread = await _context.Threads
            .Include(t => t.Category)
            .Include(t => t.User) // ✅ Include User
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
                Username = t.User.Username // ✅ Retrieve Username
            })
            .FirstOrDefaultAsync();

        if (thread == null)
        {
            return NotFound();
        }

        return Ok(thread);
    }

    // Create a new thread
    [HttpPost("Insert")]
    public async Task<ActionResult<ThreadDTO>> InsertThread(ThreadDTO threadDTO)
    {
        var user = await _context.Users.FindAsync(threadDTO.UserId);
        if (user == null)
        {
            return BadRequest("Invalid User ID");
        }

        var thread = new diendan2.Models2.Thread
        {
            Title = threadDTO.Title,
            Content = threadDTO.Content,
            CreatedAt = DateTime.UtcNow,
            CategoryId = threadDTO.CategoryId,
            UserId = threadDTO.UserId,
        };

        _context.Threads.Add(thread);
        await _context.SaveChangesAsync();

        var category = await _context.Categories.FindAsync(thread.CategoryId);

        threadDTO.ThreadId = thread.ThreadId;
        threadDTO.CategoryName = category?.Name;
        threadDTO.Username = user.Username; // ✅ Username from `user` object

        return CreatedAtAction(nameof(GetThreadById), new { id = thread.ThreadId }, threadDTO);
    }


    // Update a thread
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateThread(int id, [FromBody] ThreadUpdateDto dto)
    {
        var thread = await _context.Threads.FindAsync(id);
        if (thread == null)
        {
            return NotFound(new { message = "Thread not found." });
        }

        thread.Title = dto.Title;
        thread.Content = dto.Content;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Thread updated successfully." });
    }

    // Delete a thread
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteThread(int id)
    {
        var thread = await _context.Threads.FindAsync(id);
        if (thread == null)
        {
            return NotFound(new { message = "Thread not found." });
        }

        _context.Threads.Remove(thread);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Thread deleted successfully." });
    }
}

public class ThreadDTO
{
    public int ThreadId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public string CategoryName { get; set; }
    public string Username { get; set; } // ✅ Add Username Property

}

public class ThreadCreateDto
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int CategoryId { get; set; }
    public string Username { get; set; } // Add username property
}

public class ThreadUpdateDto
{
    public string Title { get; set; }
    public string Content { get; set; }
}