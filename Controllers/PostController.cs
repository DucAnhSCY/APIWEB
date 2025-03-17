using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;

namespace diendan2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public PostController(DBContextTest2 context)
    {
        _context = context;
    }

    // GET: api/Post
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDTO>>> GetPosts()
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Thread)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                ThreadId = p.ThreadId,
                ThreadTitle = p.Thread.Title,
                UserId = p.UserId,
                Username = p.User.Username,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                CommentCount = p.Comments.Count,
                LikeCount = p.Likes.Count
            })
            .ToListAsync();
    }

    // GET: api/Post/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDetailDTO>> GetPost(int id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Thread)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Include(p => p.Reports)
                .ThenInclude(r => r.User)
            .Where(p => p.PostId == id)
            .Select(p => new PostDetailDTO
            {
                PostId = p.PostId,
                ThreadId = p.ThreadId,
                ThreadTitle = p.Thread.Title,
                UserId = p.UserId,
                Username = p.User.Username,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                CommentCount = p.Comments.Count,
                LikeCount = p.Likes.Count,
                Comments = p.Comments.Select(c => new CommentDTO
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                Reports = p.Reports.Select(r => new ReportDTO
                {
                    ReportId = r.ReportId,
                    PostId = r.PostId,
                    UserId = r.UserId,
                    Username = r.User.Username,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (post == null)
        {
            return NotFound();
        }
        return post;
    }

    // GET: api/Post/Thread/5
    [HttpGet("Thread/{threadId}")]
    public async Task<ActionResult<IEnumerable<PostDTO>>> GetPostsByThread(int threadId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.ThreadId == threadId)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                ThreadId = p.ThreadId,
                ThreadTitle = p.Thread.Title,
                UserId = p.UserId,
                Username = p.User.Username,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                CommentCount = p.Comments.Count,
                LikeCount = p.Likes.Count
            })
            .ToListAsync();
    }

    // GET: api/Post/User/5
    [HttpGet("User/{userId}")]
    public async Task<ActionResult<IEnumerable<PostDTO>>> GetPostsByUser(int userId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Thread)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.UserId == userId)
            .Select(p => new PostDTO
            {
                PostId = p.PostId,
                ThreadId = p.ThreadId,
                ThreadTitle = p.Thread.Title,
                UserId = p.UserId,
                Username = p.User.Username,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                CommentCount = p.Comments.Count,
                LikeCount = p.Likes.Count
            })
            .ToListAsync();
    }

    // POST: api/Post
    [HttpPost]
    public async Task<ActionResult<PostDTO>> CreatePost(CreatePostDTO createPostDTO)
    {
        var post = new Post
        {
            ThreadId = createPostDTO.ThreadId,
            UserId = createPostDTO.UserId,
            Content = createPostDTO.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var postDTO = new PostDTO
        {
            PostId = post.PostId,
            ThreadId = post.ThreadId,
            UserId = post.UserId,
            Content = post.Content,
            CreatedAt = post.CreatedAt
        };

        return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, postDTO);
    }

    // PUT: api/Post/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, UpdatePostDTO updatePostDTO)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        post.Content = updatePostDTO.Content;
        _context.Entry(post).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
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

    // DELETE: api/Post/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PostExists(int id)
    {
        return _context.Posts.Any(e => e.PostId == id);
    }
} 