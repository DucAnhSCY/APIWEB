using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication9.Models2;

namespace WebApplication9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly DBContextTest2 _context;

        public ReportController(DBContextTest2 context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả Report
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _context.Reports.ToListAsync();
            return Ok(reports);
        }
        // Lấy Report theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "Không tìm thấy Report." });
            }
            return Ok(report);
        }
        // Thêm Report mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(int postId, int? regUserId, int? modId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest("Lý do báo cáo không được để trống");
            }
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return BadRequest("Bài đăng không tồn tại");
            }

            var report = new Report
            {
                PostId = postId,
                RegUserId = regUserId,
                ModId = modId,
                Reason = reason,
                Status = "pending",
                CreatedAt = DateTime.Now
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Báo cáo bài viết thành công!", report });
        }
        // Cập nhật Report
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "Không tìm thấy báo cáo." });
            }
            report.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái báo cáo thành công!", report });
        }
        // Xóa Report
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound(new { message = "Không tìm thấy báo cáo." });
            }
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa báo cáo thành công!" });
        }

    }
}
