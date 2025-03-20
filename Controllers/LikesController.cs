using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diendan2.Models2;

[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public LikesController(DBContextTest2 context)
    {
        _context = context;
    }

    // Get likes for a post
    [HttpGet("ByPost/{postId}")]
    public async Task<ActionResult<LikesSummaryDTO>> GetLikesByPost(int postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        var likes = await _context.Likes
            .Include(l => l.User)
            .Where(l => l.PostId == postId)
            .Select(l => new LikeDTO
            {
                LikeId = l.LikeId,
                PostId = l.PostId,
                UserId = l.UserId,
                Username = l.User.Username
            })
            .ToListAsync();

        var summary = new LikesSummaryDTO
        {
            PostId = postId,
            LikeCount = likes.Count,
            Likes = likes
        };

        return Ok(summary);
    }

    // Check if user has liked a post
    [HttpGet("Check/{postId}/{userId}")]
    public async Task<ActionResult<bool>> CheckUserLike(int postId, int userId)
    {
        var like = await _context.Likes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);

        return Ok(like);
    }

    // Add a like
    [HttpPost("Toggle")]
    public async Task<ActionResult<LikeDTO>> ToggleLike(LikeCreateDto dto)
    {
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == dto.PostId && l.UserId == dto.UserId);

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Like removed successfully." });
        }

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid User ID." });
        }

        var post = await _context.Posts.FindAsync(dto.PostId);
        if (post == null)
        {
            return BadRequest(new { message = "Invalid Post ID." });
        }

        var like = new Like
        {
            PostId = dto.PostId,
            UserId = dto.UserId
        };

        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        var likeDTO = new LikeDTO
        {
            LikeId = like.LikeId,
            PostId = like.PostId,
            UserId = like.UserId,
            Username = user.Username
        };

        return CreatedAtAction(nameof(GetLikesByPost), new { postId = dto.PostId }, likeDTO);
    }

    // Get likes by user
    [HttpGet("ByUser/{userId}")]
    public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikesByUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var likes = await _context.Likes
            .Include(l => l.Post)
            .Where(l => l.UserId == userId)
            .Select(l => new LikeDTO
            {
                LikeId = l.LikeId,
                PostId = l.PostId,
                UserId = l.UserId,
                Username = user.Username
            })
            .ToListAsync();

        return Ok(likes);
    }
}

// DTOs
public class LikeDTO
{
    public int LikeId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
}

public class LikeCreateDto
{
    public int PostId { get; set; }
    public int UserId { get; set; }
}

public class LikesSummaryDTO
{
    public int PostId { get; set; }
    public int LikeCount { get; set; }
    public List<LikeDTO> Likes { get; set; }
} 