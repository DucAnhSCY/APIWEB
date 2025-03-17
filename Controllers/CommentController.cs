using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;

namespace diendan2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public CommentController(DBContextTest2 context)
    {
        _context = context;
    }

    // GET: api/Comment
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments()
    {
        return await _context.Comments
            .Include(c => c.User)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    // GET: api/Comment/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDTO>> GetComment(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.CommentId == id)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (comment == null)
        {
            return NotFound();
        }
        return comment;
    }

    // GET: api/Comment/Post/5
    [HttpGet("Post/{postId}")]
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetCommentsByPost(int postId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User.Username,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    // POST: api/Comment
    [HttpPost]
    public async Task<ActionResult<CommentDTO>> CreateComment(CreateCommentDTO createCommentDTO)
    {
        var comment = new Comment
        {
            PostId = createCommentDTO.PostId,
            UserId = createCommentDTO.UserId,
            Content = createCommentDTO.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var commentDTO = new CommentDTO
        {
            CommentId = comment.CommentId,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };

        return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, commentDTO);
    }

    // PUT: api/Comment/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(int id, UpdateCommentDTO updateCommentDTO)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        comment.Content = updateCommentDTO.Content;
        _context.Entry(comment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CommentExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Comment/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CommentExists(int id)
    {
        return _context.Comments.Any(e => e.CommentId == id);
    }
}