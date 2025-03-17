using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using diendan2.Models2;

namespace diendan2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public ReportController(DBContextTest2 context)
    {
        _context = context;
    }

    // GET: api/Report
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReports()
    {
        return await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Post)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    // GET: api/Report/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDTO>> GetReport(int id)
    {
        var report = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Post)
            .Where(r => r.ReportId == id)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound();
        }
        return report;
    }

    // GET: api/Report/Post/5
    [HttpGet("Post/{postId}")]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportsByPost(int postId)
    {
        return await _context.Reports
            .Include(r => r.User)
            .Where(r => r.PostId == postId)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    // GET: api/Report/User/5
    [HttpGet("User/{userId}")]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportsByUser(int userId)
    {
        return await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Post)
            .Where(r => r.UserId == userId)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    // GET: api/Report/Status/pending
    [HttpGet("Status/{status}")]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportsByStatus(string status)
    {
        return await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Post)
            .Where(r => r.Status == status)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    // POST: api/Report
    [HttpPost]
    public async Task<ActionResult<ReportDTO>> CreateReport(CreateReportDTO createReportDTO)
    {
        var report = new Report
        {
            PostId = createReportDTO.PostId,
            UserId = createReportDTO.UserId,
            Reason = createReportDTO.Reason,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var reportDTO = new ReportDTO
        {
            ReportId = report.ReportId,
            PostId = report.PostId,
            UserId = report.UserId,
            Reason = report.Reason,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        };

        return CreatedAtAction(nameof(GetReport), new { id = report.ReportId }, reportDTO);
    }

    // PUT: api/Report/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReport(int id, ReportDTO reportDTO)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        report.Reason = reportDTO.Reason;
        report.Status = reportDTO.Status;
        _context.Entry(report).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReportExists(id))
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

    // PUT: api/Report/5/Status
    [HttpPut("{id}/Status")]
    public async Task<IActionResult> UpdateReportStatus(int id, UpdateReportStatusDTO updateReportStatusDTO)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        report.Status = updateReportStatusDTO.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Report/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReportExists(int id)
    {
        return _context.Reports.Any(e => e.ReportId == id);
    }
} 