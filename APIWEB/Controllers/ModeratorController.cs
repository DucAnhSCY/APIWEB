using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.Text.Json.Serialization;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModeratorController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<ModeratorController> _logger;

        public ModeratorController(DBContextTest context, ILogger<ModeratorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] ModeratorLoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Moderator login attempt with email: {loginRequest?.ModeratorEmail}");

                if (loginRequest == null || !ModelState.IsValid)
                {
                    _logger.LogWarning("Login failed: Invalid login request");
                    return BadRequest(new { message = "Email và mật khẩu không được để trống." });
                }

                var moderator = await _context.Moderators
                    .FirstOrDefaultAsync(m => m.Email == loginRequest.ModeratorEmail);

                if (moderator == null)
                {
                    _logger.LogWarning($"Login failed: Email not found: {loginRequest.ModeratorEmail}");
                    return Unauthorized(new { message = "Email không tồn tại." });
                }

                if (moderator.Password != loginRequest.ModeratorPassword)
                {
                    _logger.LogWarning($"Login failed: Incorrect password for {loginRequest.ModeratorEmail}");
                    return Unauthorized(new { message = "Mật khẩu không chính xác." });
                }

                if (moderator.Status != "Active")
                {
                    _logger.LogWarning($"Login failed: Account disabled for {loginRequest.ModeratorEmail}");
                    return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên." });
                }

                _logger.LogInformation($"Moderator login successful: {moderator.Username} (ID: {moderator.ModId})");

                // Loại bỏ mật khẩu trước khi trả về thông tin
                return Ok(new
                {
                    message = "Đăng nhập thành công!",
                    isAuthenticated = true,
                    userId = moderator.ModId,
                    username = moderator.Username,
                    email = moderator.Email,
                    status = moderator.Status,
                    joinDate = moderator.JoinDate,
                    role = "Moderator"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in moderator login");
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }
    }

    public class ModeratorLoginRequest
    {
        [JsonPropertyName("moderatorEmail")]
        public string ModeratorEmail { get; set; } = string.Empty;

        [JsonPropertyName("moderatorPassword")]
        public string ModeratorPassword { get; set; } = string.Empty;
    }
}