using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;

namespace diendan2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LikeController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public LikeController(DBContextTest2 context)
    {
        _context = context;
    }

    // GET: api/Like
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikes()
    {
        return await _context.Likes.ToListAsync();
    }

    // GET: api/Like/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Like>> GetLike(int id)
    {
        var like = await _context.Likes.FindAsync(id);
        if (like == null)
        {
            return NotFound();
        }
        return like;
    }

    // GET: api/Like/Post/5
    [HttpGet("Post/{postId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesByPost(int postId)
    {
        return await _context.Likes.Where(l => l.PostId == postId).ToListAsync();
    }

    // GET: api/Like/User/5
    [HttpGet("User/{userId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesByUser(int userId)
    {
        return await _context.Likes.Where(l => l.UserId == userId).ToListAsync();
    }

    // POST: api/Like
    [HttpPost]
    public async Task<ActionResult<Like>> CreateLike(Like like)
    {
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLike), new { id = like.LikeId }, like);
    }

    // DELETE: api/Like/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLike(int id)
    {
        var like = await _context.Likes.FindAsync(id);
        if (like == null)
        {
            return NotFound();
        }

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool LikeExists(int id)
    {
        return _context.Likes.Any(e => e.LikeId == id);
    }
} 