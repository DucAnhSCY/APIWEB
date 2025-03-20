using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diendan2.Models2;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public CommentController(DBContextTest2 context)
    {
        _context = context;
    }

    // Get all comments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetAllComments()
    {
        var comments = await _context.Comments
            .Include(c => c.Post)
            .Include(c => c.User)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .ToListAsync();

        return Ok(comments);
    }

    // Get comment by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDTO>> GetCommentById(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.Post)
            .Include(c => c.User)
            .Where(c => c.CommentId == id)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .FirstOrDefaultAsync();

        if (comment == null)
        {
            return NotFound(new { message = "Comment not found." });
        }

        return Ok(comment);
    }

    // Get comments by post ID
    [HttpGet("ByPost/{postId}")]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentsByPost(int postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt ?? DateTime.MinValue,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username
            })
            .ToListAsync();

        return Ok(comments);
    }

    // Create a new comment
    [HttpPost("Insert")]
    public async Task<ActionResult<CommentDTO>> InsertComment(CommentCreateDto commentDto)
    {
        var user = await _context.Users.FindAsync(commentDto.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid User ID." });
        }

        var post = await _context.Posts.FindAsync(commentDto.PostId);
        if (post == null)
        {
            return BadRequest(new { message = "Invalid Post ID." });
        }

        var comment = new Comment
        {
            Content = commentDto.Content,
            CreatedAt = DateTime.UtcNow,
            PostId = commentDto.PostId,
            UserId = commentDto.UserId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var commentDTO = new CommentDTO
        {
            CommentId = comment.CommentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt ?? DateTime.MinValue,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Username = user.Username
        };

        return CreatedAtAction(nameof(GetCommentById), new { id = comment.CommentId }, commentDTO);
    }

    // Update a comment
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentUpdateDto dto)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound(new { message = "Comment not found." });
        }

        comment.Content = dto.Content;
        comment.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Comment updated successfully." });
    }

    // Delete a comment
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound(new { message = "Comment not found." });
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Comment deleted successfully." });
    }
}

// DTOs
public class CommentDTO
{
    public int CommentId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
}

public class CommentCreateDto
{
    public string Content { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
}

public class CommentUpdateDto
{
    public string Content { get; set; }
} 