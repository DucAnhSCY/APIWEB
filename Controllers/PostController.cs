using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diendan2.Models2;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public PostController(DBContextTest2 context)
    {
        _context = context;
    }

    // Get all posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDTO>>> GetAllPosts()
    {
        var posts = await _context.Posts
            .Include(p => p.Thread)
            .Include(p => p.User)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                Content = p.Content,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                ThreadId = p.ThreadId ,
                UserId = p.UserId ?? 0,
                ThreadTitle = p.Thread.Title,
                Username = p.User.Username
            })
            .ToListAsync();

        return Ok(posts);
    }

    // Get post by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDTO>> GetPostById(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Thread)
            .Include(p => p.User)
            .Where(p => p.PostId == id)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                Content = p.Content,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                ThreadId = p.ThreadId ,
                UserId = p.UserId ?? 0,
                ThreadTitle = p.Thread.Title,
                Username = p.User.Username
            })
            .FirstOrDefaultAsync();

        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        return Ok(post);
    }

    // Get posts by thread ID
    [HttpGet("ByThread/{threadId}")]
    public async Task<ActionResult<IEnumerable<PostDTO>>> GetPostsByThread(int threadId)
    {
        var thread = await _context.Threads.FindAsync(threadId);
        if (thread == null)
        {
            return NotFound(new { message = "Thread not found." });
        }

        var posts = await _context.Posts
            .Include(p => p.User)
            .Where(p => p.ThreadId == threadId)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                Content = p.Content,
                CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                ThreadId = p.ThreadId ,
                UserId = p.UserId ?? 0,
                ThreadTitle = thread.Title,
                Username = p.User.Username
            })
            .ToListAsync();

        return Ok(posts);
    }

    // Create a new post
    [HttpPost("Insert")]
    public async Task<ActionResult<PostDTO>> InsertPost(PostCreateDto postDto)
    {
        var user = await _context.Users.FindAsync(postDto.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid User ID." });
        }

        var thread = await _context.Threads.FindAsync(postDto.ThreadId);
        if (thread == null)
        {
            return BadRequest(new { message = "Invalid Thread ID." });
        }

        var post = new Post
        {
            Content = postDto.Content,
            CreatedAt = DateTime.UtcNow,
            ThreadId = postDto.ThreadId,
            UserId = postDto.UserId
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var postDTO = new PostDTO
        {
            PostId = post.PostId,
            Content = post.Content,
            CreatedAt = post.CreatedAt ?? DateTime.MinValue,
            ThreadId = post.ThreadId ,
            UserId = post.UserId ?? 0,
            ThreadTitle = thread.Title,
            Username = user.Username
        };

        return CreatedAtAction(nameof(GetPostById), new { id = post.PostId }, postDTO);
    }

    // Update a post
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] PostUpdateDto dto)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        post.Content = dto.Content;
        post.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Post updated successfully." });
    }

    // Delete a post
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Post deleted successfully." });
    }
}

// DTOs
public class PostDTO
{
    public int PostId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ThreadId { get; set; }
    public int UserId { get; set; }
    public string ThreadTitle { get; set; }
    public string Username { get; set; }
}

public class PostCreateDto
{
    public string Content { get; set; }
    public int ThreadId { get; set; }
    public int UserId { get; set; }
}

public class PostUpdateDto
{
    public string Content { get; set; }
}