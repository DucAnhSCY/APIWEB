using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<UserController> _logger;

        public UserController(DBContextTest context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 🔹 Lấy danh sách người dùng (với phân trang)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                _logger.LogInformation($"Getting users page {page} with page size {pageSize}, role filter: {role ?? "all"}");

                // Build query
                var query = _context.Users.AsNoTracking();
                
                // Apply role filter if specified
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // Get total count with minimal overhead
                var totalCount = await query.CountAsync();

                // Paginate and select only necessary fields to reduce data transfer
                var users = await query
                    .OrderByDescending(u => u.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDTO
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        Status = u.Status ?? "active",
                        JoinDate = u.JoinDate ?? DateTime.Now
                    })
                    .ToListAsync();

                // Calculate pagination metadata
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    totalCount,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách người dùng." });
            }
        }

        // 🔹 Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting user by ID: {id}");

                // Use projection to limit data retrieved
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == id)
                    .Select(u => new UserDTO
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        Status = u.Status ?? "active",
                        JoinDate = u.JoinDate ?? DateTime.Now
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi tải thông tin người dùng." });
            }
        }

        // 🔹 Tìm kiếm người dùng theo tên
        [HttpGet("Search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "Tên tìm kiếm không được để trống." });
                }

                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                _logger.LogInformation($"Searching users with name containing '{name}', page {page}, pageSize {pageSize}, role: {role ?? "all"}");

                // Build query
                var query = _context.Users.AsNoTracking()
                    .Where(u => u.Username.Contains(name));
                
                // Apply role filter if specified
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Get paginated results
                var users = await query
                    .OrderByDescending(u => u.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDTO
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        Status = u.Status ?? "active",
                        JoinDate = u.JoinDate ?? DateTime.Now
                    })
                    .ToListAsync();

                // Calculate pagination metadata
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    totalCount,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching users with name containing '{name}'");
                return StatusCode(500, new { message = "Lỗi server khi tìm kiếm người dùng." });
            }
        }

        // 🔹 Thêm người dùng mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromForm] CreateUserDTO createUserDTO)
        {
            try
            {
                _logger.LogInformation($"Creating new user: {createUserDTO.Username}, {createUserDTO.Email}, Role: {createUserDTO.Role}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate email format
                if (!IsValidEmail(createUserDTO.Email))
                {
                    return BadRequest(new { message = "Email không hợp lệ." });
                }

                // Check if username or email already exists
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Username == createUserDTO.Username || u.Email == createUserDTO.Email);

                if (existingUser)
                {
                    return BadRequest(new { message = "Tên đăng nhập hoặc email đã tồn tại." });
                }

                // Validate role
                if (!IsValidRole(createUserDTO.Role))
                {
                    return BadRequest(new { message = "Vai trò không hợp lệ. Vai trò phải là 'Admin', 'Moderator', hoặc 'User'." });
                }

                var user = new User
                {
                    Username = createUserDTO.Username,
                    Email = createUserDTO.Email,
                    Password = createUserDTO.Password, // TODO: Mã hóa mật khẩu trước khi lưu
                    Role = createUserDTO.Role,
                    Status = "active",
                    JoinDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Tạo người dùng thành công.",
                    user = new UserDTO
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.Status ?? "active",
                        JoinDate = user.JoinDate ?? DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new user");
                return StatusCode(500, new { message = "Lỗi server khi tạo người dùng mới." });
            }
        }

        // 🔹 Cập nhật thông tin người dùng
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateUserDTO updateUserDTO)
        {
            try
            {
                _logger.LogInformation($"Updating user with ID: {id}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Update failed: User with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                // Check if username is being changed and if it already exists
                if (updateUserDTO.Username != user.Username)
                {
                    var existingUsername = await _context.Users
                        .AnyAsync(u => u.Username == updateUserDTO.Username && u.UserId != id);

                    if (existingUsername)
                    {
                        return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
                    }
                }

                // Check if email is being changed and if it already exists
                if (updateUserDTO.Email != user.Email)
                {
                    // Validate email format
                    if (!IsValidEmail(updateUserDTO.Email))
                    {
                        return BadRequest(new { message = "Email không hợp lệ." });
                    }

                    var existingEmail = await _context.Users
                        .AnyAsync(u => u.Email == updateUserDTO.Email && u.UserId != id);

                    if (existingEmail)
                    {
                        return BadRequest(new { message = "Email đã tồn tại." });
                    }
                }

                // Validate role if it's being updated
                if (!string.IsNullOrEmpty(updateUserDTO.Role) && updateUserDTO.Role != user.Role)
                {
                    if (!IsValidRole(updateUserDTO.Role))
                    {
                        return BadRequest(new { message = "Vai trò không hợp lệ. Vai trò phải là 'Admin', 'Moderator', hoặc 'User'." });
                    }
                }

                // Validate status if it's being updated
                if (!string.IsNullOrEmpty(updateUserDTO.Status) && updateUserDTO.Status != user.Status)
                {
                    if (!IsValidStatus(updateUserDTO.Status))
                    {
                        return BadRequest(new { message = "Trạng thái không hợp lệ. Trạng thái phải là 'active', 'inactive', hoặc 'Ban'." });
                    }
                }

                // Update user properties
                user.Username = updateUserDTO.Username;
                user.Email = updateUserDTO.Email;

                // Only update password if provided
                if (!string.IsNullOrEmpty(updateUserDTO.Password))
                {
                    user.Password = updateUserDTO.Password; // TODO: Mã hóa mật khẩu trước khi lưu
                }

                // Only update role if provided
                if (!string.IsNullOrEmpty(updateUserDTO.Role))
                {
                    user.Role = updateUserDTO.Role;
                }

                // Only update status if provided
                if (!string.IsNullOrEmpty(updateUserDTO.Status))
                {
                    user.Status = updateUserDTO.Status;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật người dùng thành công.",
                    user = new UserDTO
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.Status ?? "active",
                        JoinDate = user.JoinDate ?? DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật thông tin người dùng." });
            }
        }

        // 🔹 Xóa người dùng
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting user with ID: {id}");

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Delete failed: User with ID {id} not found");
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa người dùng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa người dùng." });
            }
        }

        // 🔹 Đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Login attempt with email: {loginRequest?.Email}");

                if (loginRequest == null || !ModelState.IsValid)
                {
                    _logger.LogWarning("Login failed: Invalid login request");
                    return BadRequest(new { message = "Email và mật khẩu không được để trống." });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: Email not found: {loginRequest.Email}");
                    return Unauthorized(new { message = "Email không tồn tại." });
                }

                if (user.Password != loginRequest.Password)
                {
                    _logger.LogWarning($"Login failed: Incorrect password for {loginRequest.Email}");
                    return Unauthorized(new { message = "Mật khẩu không chính xác." });
                }

                if (user.Status != "active")
                {
                    _logger.LogWarning($"Login failed: User account is not active: {loginRequest.Email}");
                    return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa hoặc bị cấm." });
                }

                return Ok(new
                {
                    message = "Đăng nhập thành công.",
                    user = new UserDTO
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.Status ?? "active",
                        JoinDate = user.JoinDate ?? DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Lỗi server khi đăng nhập." });
            }
        }

        // Helper methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidRole(string role)
        {
            return role == "Admin" || role == "Moderator" || role == "User";
        }

        private bool IsValidStatus(string status)
        {
            return status == "active" || status == "inactive" || status == "Ban";
        }
    }

    // DTOs
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò không được để trống")]
        public string Role { get; set; } = "User";
    }

    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }
    }

    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}