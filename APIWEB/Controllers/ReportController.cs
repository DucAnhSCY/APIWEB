using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(DBContextTest context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy danh sách tất cả Report
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all reports");
                
                var reports = await _context.Reports
                    .AsNoTracking()
                    .Include(r => r.Post)
                    .Include(r => r.User)
                    .Select(r => new ReportDTO
                    {
                        ReportId = r.ReportId,
                        PostId = r.PostId,
                        UserId = r.UserId,
                        UserName = r.User.Username,
                        Reason = r.Reason,
                        Status = r.Status ?? "pending",
                        CreatedAt = r.CreatedAt ?? DateTime.MinValue
                    })
                    .ToListAsync();

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách báo cáo." });
            }
        }

        // Lấy Report theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting report by ID: {id}");
                
                var report = await _context.Reports
                    .AsNoTracking()
                    .Include(r => r.Post)
                    .Include(r => r.User)
                    .Where(r => r.ReportId == id)
                    .Select(r => new ReportDTO
                    {
                        ReportId = r.ReportId,
                        PostId = r.PostId,
                        UserId = r.UserId,
                        UserName = r.User.Username,
                        Reason = r.Reason,
                        Status = r.Status ?? "pending",
                        CreatedAt = r.CreatedAt ?? DateTime.MinValue
                    })
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    _logger.LogWarning($"Report with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy báo cáo." });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving report with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin báo cáo." });
            }
        }

        // Thêm Report mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] ReportCreateDTO reportCreateDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new report for post ID: {reportCreateDTO.PostId}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate post exists
                var post = await _context.Posts.FindAsync(reportCreateDTO.PostId);
                if (post == null)
                {
                    return BadRequest(new { message = "Bài viết không tồn tại." });
                }

                // Validate user exists
                var user = await _context.Users.FindAsync(reportCreateDTO.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại." });
                }

                var report = new Report
                {
                    PostId = reportCreateDTO.PostId,
                    UserId = reportCreateDTO.UserId,
                    Reason = reportCreateDTO.Reason,
                    Status = "pending",
                    CreatedAt = DateTime.Now
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Tạo báo cáo thành công.",
                    reportId = report.ReportId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new report");
                return StatusCode(500, new { message = "Lỗi server khi tạo báo cáo mới." });
            }
        }

        // Cập nhật trạng thái Report
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReportUpdateDTO reportUpdateDTO)
        {
            try
            {
                _logger.LogInformation($"Updating status of report with ID: {id} to {reportUpdateDTO.Status}");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    _logger.LogWarning($"Update failed: Report with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy báo cáo." });
                }

                // Validate status
                if (reportUpdateDTO.Status != "pending" && reportUpdateDTO.Status != "resolved" && reportUpdateDTO.Status != "rejected")
                {
                    return BadRequest(new { message = "Trạng thái không hợp lệ. Trạng thái phải là 'pending', 'resolved', hoặc 'rejected'." });
                }

                report.Status = reportUpdateDTO.Status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái báo cáo thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status of report with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật trạng thái báo cáo." });
            }
        }

        // Xóa Report
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting report with ID: {id}");
                
                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    _logger.LogWarning($"Delete failed: Report with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy báo cáo." });
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa báo cáo thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting report with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa báo cáo." });
            }
        }
    }

    public class ReportDTO
    {
        public int ReportId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReportCreateDTO
    {
        [Required(ErrorMessage = "ID bài viết không được để trống")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "ID người dùng không được để trống")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Lý do không được để trống")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportUpdateDTO
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string Status { get; set; } = string.Empty;
    }
}
