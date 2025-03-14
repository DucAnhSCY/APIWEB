using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;

namespace APIWEB.Controllers;
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DBContextTest2 _db;

        public AdminController(DBContextTest2 db)
        {
            _db = db;
        }

        // 🔹 Lấy danh sách Admin
        [HttpGet("GetList")]
        public IActionResult GetList()
        {
            return Ok(_db.Admins.ToList());
        }

        // 🔹 Lấy Admin theo ID
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null)
                return NotFound("Không tìm thấy Admin.");

            return Ok(admin);
        }

        // 🔹 Thêm Admin
        [HttpPost("Insert")]
        public IActionResult Insert(string username, string email, string password, string status)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return BadRequest("Thông tin không được để trống.");

            var admin = new Admin
            {
                Username = username,
                Email = email,
                Password = password, // TODO: Mã hóa mật khẩu trước khi lưu
                Status = status ?? "Active",
                JoinDate = DateTime.Now
            };

            _db.Admins.Add(admin);
            _db.SaveChanges();

            return Ok(admin);
        }

        // 🔹 Cập nhật Admin
        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, string username, string email, string password, string status)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null)
                return NotFound("Không tìm thấy Admin.");

            admin.Username = username;
            admin.Email = email;
            admin.Password = password; // TODO: Cân nhắc mã hóa mật khẩu
            admin.Status = status;

            _db.SaveChanges();
            return Ok(admin);
        }
    }
}
