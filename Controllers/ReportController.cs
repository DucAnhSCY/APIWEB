using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diendan2.Models2;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public ReportController(DBContextTest2 context)
    {
        _context = context;
    }

    // Get all reports
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetAllReports()
    {
        var reports = await _context.Reports
            .Include(r => r.Post)
            .Include(r => r.User)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt ?? DateTime.MinValue
            })
            .ToListAsync();

        return Ok(reports);
    }

    // Get report by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDTO>> GetReportById(int id)
    {
        var report = await _context.Reports
            .Include(r => r.Post)
            .Include(r => r.User)
            .Where(r => r.ReportId == id)
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt ?? DateTime.MinValue
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound(new { message = "Report not found." });
        }

        return Ok(report);
    }

    // Get reports by post ID
    [HttpGet("ByPost/{postId}")]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportsByPost(int postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        var reports = await _context.Reports
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
                CreatedAt = r.CreatedAt ?? DateTime.MinValue
            })
            .ToListAsync();

        return Ok(reports);
    }

    // Create a new report
    [HttpPost("Insert")]
    public async Task<ActionResult<ReportDTO>> CreateReport(ReportCreateDto dto)
    {
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

        // Check if user has already reported this post
        var existingReport = await _context.Reports
            .FirstOrDefaultAsync(r => r.PostId == dto.PostId && r.UserId == dto.UserId);

        if (existingReport != null)
        {
            return BadRequest(new { message = "You have already reported this post." });
        }

        var report = new Report
        {
            PostId = dto.PostId,
            UserId = dto.UserId,
            Reason = dto.Reason,
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
            Username = user.Username,
            Reason = report.Reason,
            Status = report.Status,
            CreatedAt = report.CreatedAt ?? DateTime.MinValue
        };

        return CreatedAtAction(nameof(GetReportById), new { id = report.ReportId }, reportDTO);
    }

    // Update report status (for moderators/admins)
    [HttpPut("UpdateStatus/{id}")]
    public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] ReportUpdateDto dto)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound(new { message = "Report not found." });
        }

        // Validate status
        if (!new[] { "pending", "resolved", "rejected" }.Contains(dto.Status.ToLower()))
        {
            return BadRequest(new { message = "Invalid status. Must be 'pending', 'resolved', or 'rejected'." });
        }

        report.Status = dto.Status.ToLower();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Report status updated successfully." });
    }

    // Get pending reports count (for moderators/admins)
    [HttpGet("PendingCount")]
    public async Task<ActionResult<int>> GetPendingReportsCount()
    {
        var count = await _context.Reports
            .CountAsync(r => r.Status == "pending");

        return Ok(count);
    }

    // Get reports by status
    [HttpGet("ByStatus/{status}")]
    public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReportsByStatus(string status)
    {
        if (!new[] { "pending", "resolved", "rejected" }.Contains(status.ToLower()))
        {
            return BadRequest(new { message = "Invalid status. Must be 'pending', 'resolved', or 'rejected'." });
        }

        var reports = await _context.Reports
            .Include(r => r.Post)
            .Include(r => r.User)
            .Where(r => r.Status == status.ToLower())
            .Select(r => new ReportDTO
            {
                ReportId = r.ReportId,
                PostId = r.PostId,
                UserId = r.UserId,
                Username = r.User.Username,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt ?? DateTime.MinValue
            })
            .ToListAsync();

        return Ok(reports);
    }
}

// DTOs
public class ReportDTO
{
    public int ReportId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportCreateDto
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; }
}

public class ReportUpdateDto
{
    public string Status { get; set; }
} 