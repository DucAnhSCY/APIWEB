using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.Text.Json.Serialization;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DBContextTest _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(DBContextTest db, ILogger<AdminController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // 🔹 Lấy danh sách Admin
        [HttpGet("GetList")]
        public IActionResult GetList()
        {
            _logger.LogInformation("Getting admin list");
            return Ok(_db.Admins.ToList());
        }

        // 🔹 Lấy Admin theo ID
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation($"Getting admin by ID: {id}");
            var admin = _db.Admins.Find(id);
            if (admin == null)
                return NotFound("Không tìm thấy Admin.");

            return Ok(admin);
        }

        // 🔹 Thêm Admin
        [HttpPost("Insert")]
        public IActionResult Insert(string adminUsername, string adminEmail, string adminPassword, string adminStatus)
        {
            _logger.LogInformation($"Inserting new admin: {adminUsername}, {adminEmail}");

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                return BadRequest("Thông tin không được để trống.");

            var admin = new Admin
            {
                Username = adminUsername,
                Email = adminEmail,
                Password = adminPassword, // TODO: Mã hóa mật khẩu trước khi lưu
                Status = adminStatus ?? "Active",
                JoinDate = DateTime.Now
            };

            _db.Admins.Add(admin);
            _db.SaveChanges();

            return Ok(admin);
        }

        // 🔹 Cập nhật Admin
        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, string adminUsername, string adminEmail, string adminPassword, string adminStatus)
        {
            _logger.LogInformation($"Updating admin ID: {id}");

            var admin = _db.Admins.Find(id);
            if (admin == null)
                return NotFound("Không tìm thấy Admin.");

            admin.Username = adminUsername;
            admin.Email = adminEmail;
            admin.Password = adminPassword; // TODO: Cân nhắc mã hóa mật khẩu
            admin.Status = adminStatus;

            _db.SaveChanges();
            return Ok(admin);
        }

        // 🔹 Đăng nhập Admin
        [HttpPost("Login")]
        public IActionResult Login([FromBody] AdminLoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Admin login attempt with email: {loginRequest?.AdminEmail}");

                if (loginRequest == null)
                {
                    _logger.LogWarning("Admin login failed: request is null");
                    return BadRequest(new { message = "Invalid login request - request is null." });
                }

                // Kiểm tra xem có dữ liệu đầu vào không
                if (string.IsNullOrEmpty(loginRequest.AdminEmail) || string.IsNullOrEmpty(loginRequest.AdminPassword))
                {
                    _logger.LogWarning("Admin login failed: email or password is empty");
                    return BadRequest(new { message = "Email and password are required." });
                }

                // Tìm admin trong database
                var admin = _db.Admins
                    .FirstOrDefault(a => a.Email == loginRequest.AdminEmail);

                if (admin == null)
                {
                    _logger.LogWarning($"Admin login failed: no admin found with email {loginRequest.AdminEmail}");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                // Kiểm tra mật khẩu
                if (admin.Password != loginRequest.AdminPassword)
                {
                    _logger.LogWarning($"Admin login failed: incorrect password for {loginRequest.AdminEmail}");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                _logger.LogInformation($"Admin login successful: {admin.Username} (ID: {admin.AdminId})");

                return Ok(new
                {
                    message = "Login successful!",
                    isAuthenticated = true,
                    userId = admin.AdminId,
                    username = admin.Username,
                    email = admin.Email,
                    role = "Admin"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin login");
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }
    }

    public class AdminLoginRequest
    {
        [JsonPropertyName("adminEmail")]
        public string AdminEmail { get; set; } = string.Empty;

        [JsonPropertyName("adminPassword")]
        public string AdminPassword { get; set; } = string.Empty;
    }
}