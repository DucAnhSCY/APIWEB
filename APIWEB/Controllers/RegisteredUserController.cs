using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIWEB.Models;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisteredUserController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<RegisteredUserController> _logger;

        public RegisteredUserController(DBContextTest context, ILogger<RegisteredUserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 🔹 Lấy danh sách người dùng (với phân trang)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate page parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                _logger.LogInformation($"Getting registered users page {page} with page size {pageSize}");

                // Get total count with minimal overhead
                var totalCount = await _context.RegisteredUsers.CountAsync();

                // Paginate and select only necessary fields to reduce data transfer
                var users = await _context.RegisteredUsers
                    .AsNoTracking() // Improves performance for read-only operations
                    .OrderByDescending(u => u.RegUserId) // Order by ID for consistent paging
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDTO
                    {
                        RegUserId = u.RegUserId,
                        Username = u.Username,
                        Email = u.Email,
                        Status = u.Status,
                        JoinDate = (DateTime)u.JoinDate
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
                _logger.LogError(ex, "Error retrieving registered users");
                return StatusCode(500, new { message = "Lỗi server khi tải danh sách người dùng." });
            }
        }

        // 🔹 Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting registered user by ID: {id}");

                // Use projection to limit data retrieved
                var user = await _context.RegisteredUsers
                    .AsNoTracking()
                    .Where(u => u.RegUserId == id)
                    .Select(u => new UserDTO
                    {
                        RegUserId = u.RegUserId,
                        Username = u.Username,
                        Email = u.Email,
                        Status = u.Status,
                        JoinDate = (DateTime)u.JoinDate
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
                return StatusCode(500, new { message = "Lỗi server khi tìm người dùng." });
            }
        }

        // 🔹 Tìm kiếm người dùng theo tên
        [HttpGet("Search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                _logger.LogInformation($"Searching users by name: {name}, page {page}, pageSize {pageSize}");

                // Create an optimized query
                var query = _context.RegisteredUsers
                    .AsNoTracking()
                    .Where(u => u.Username.Contains(name));

                // Get total count first
                var totalCount = await query.CountAsync();

                // Then get paginated results
                var users = await query
                    .OrderByDescending(u => u.RegUserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDTO
                    {
                        RegUserId = u.RegUserId,
                        Username = u.Username,
                        Email = u.Email,
                        Status = u.Status,
                        JoinDate = (DateTime)u.JoinDate
                    })
                    .ToListAsync();

                // Calculate pagination metadata
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalCount == 0)
                {
                    return Ok(new
                    {
                        message = "Không tìm thấy người dùng nào phù hợp.",
                        totalCount = 0,
                        totalPages = 0,
                        currentPage = page,
                        pageSize,
                        users = new List<UserDTO>()
                    });
                }

                return Ok(new
                {
                    message = $"Tìm thấy {totalCount} người dùng phù hợp.",
                    totalCount,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching users with name: {name}");
                return StatusCode(500, new { message = "Lỗi server khi tìm kiếm người dùng." });
            }
        }

        // 🔹 Thêm người dùng mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromForm] CreateUserDTO createUserDTO)
        {
            try
            {
                _logger.LogInformation($"Inserting new user: {createUserDTO.Username}, {createUserDTO.Email}");

                // Validate input
                if (string.IsNullOrEmpty(createUserDTO.Username) ||
                    string.IsNullOrEmpty(createUserDTO.Email) ||
                    string.IsNullOrEmpty(createUserDTO.Password))
                {
                    return BadRequest(new { message = "Thông tin không được để trống." });
                }

                // Check if username already exists - use FirstOrDefaultAsync for efficiency
                if (await _context.RegisteredUsers.AnyAsync(u => u.Username == createUserDTO.Username))
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác." });
                }

                // Check if email already exists
                if (await _context.RegisteredUsers.AnyAsync(u => u.Email == createUserDTO.Email))
                {
                    return BadRequest(new { message = "Email đã được sử dụng. Vui lòng sử dụng email khác." });
                }

                // Validate email format
                if (!IsValidEmail(createUserDTO.Email))
                {
                    return BadRequest(new { message = "Email không đúng định dạng." });
                }

                // Validate password strength
                if (createUserDTO.Password.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự." });
                }

                var user = new RegisteredUser
                {
                    Username = createUserDTO.Username,
                    Email = createUserDTO.Email,
                    Password = createUserDTO.Password, // TODO: Mã hóa mật khẩu trước khi lưu
                    Status = "Active", // Always set status to Active by default
                    JoinDate = DateTime.Now
                };

                _context.RegisteredUsers.Add(user);
                await _context.SaveChangesAsync();

                var userDto = new UserDTO
                {
                    RegUserId = user.RegUserId,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status,
                    JoinDate = (DateTime)user.JoinDate
                };

                return Ok(new
                {
                    message = "Thêm người dùng thành công",
                    user = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user registration");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // 🔹 Cập nhật người dùng
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateUserDTO updateUserDTO)
        {
            try
            {
                _logger.LogInformation($"Updating user ID: {id}");

                // Find user with minimal data loading
                var user = await _context.RegisteredUsers.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found during update");
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                // Check if username is being changed and already exists
                if (user.Username != updateUserDTO.Username &&
                    await _context.RegisteredUsers.AnyAsync(u => u.Username == updateUserDTO.Username))
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác." });
                }

                // Check if email is being changed and already exists
                if (user.Email != updateUserDTO.Email &&
                    await _context.RegisteredUsers.AnyAsync(u => u.Email == updateUserDTO.Email))
                {
                    return BadRequest(new { message = "Email đã được sử dụng. Vui lòng sử dụng email khác." });
                }

                // Validate email format
                if (!IsValidEmail(updateUserDTO.Email))
                {
                    return BadRequest(new { message = "Email không đúng định dạng." });
                }

                // Update user properties
                user.Username = updateUserDTO.Username;
                user.Email = updateUserDTO.Email;

                // Only update password if provided
                if (!string.IsNullOrEmpty(updateUserDTO.Password))
                {
                    // Validate password strength
                    if (updateUserDTO.Password.Length < 6)
                    {
                        return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự." });
                    }

                    user.Password = updateUserDTO.Password; // TODO: Cân nhắc mã hóa mật khẩu
                }

                if (!string.IsNullOrEmpty(updateUserDTO.Status))
                {
                    user.Status = updateUserDTO.Status;
                }

                // Use a timeout to prevent long-running transactions
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                var userDto = new UserDTO
                {
                    RegUserId = user.RegUserId,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status,
                    JoinDate = (DateTime)user.JoinDate
                };

                return Ok(new
                {
                    message = "Cập nhật người dùng thành công",
                    user = userDto
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.RegisteredUsers.AnyAsync(u => u.RegUserId == id))
                {
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }
                return StatusCode(500, new { message = "Lỗi xung đột dữ liệu khi cập nhật người dùng." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi cập nhật người dùng: " + ex.Message });
            }
        }

        // 🔹 Xóa người dùng
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting user ID: {id}");

                // Use a more efficient approach to find and delete
                var user = await _context.RegisteredUsers.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found during delete");
                    return NotFound(new { message = "Không tìm thấy người dùng." });
                }

                // Use a transaction with timeout to prevent long-running operations
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.RegisteredUsers.Remove(user);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                _logger.LogInformation($"User deleted successfully: {user.Username} (ID: {user.RegUserId})");
                return Ok(new { message = "Xóa người dùng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, new { message = "Lỗi server khi xóa người dùng: " + ex.Message });
            }
        }

        // 🔹 Đăng nhập người dùng
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"User login attempt with email: {loginRequest?.Email}");

                if (loginRequest == null)
                {
                    _logger.LogWarning("Login failed: Invalid login request");
                    return BadRequest(new { message = "Invalid login request - request is null." });
                }

                // Kiểm tra xem có dữ liệu đầu vào không
                if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    _logger.LogWarning("Login failed: email or password is empty");
                    return BadRequest(new { message = "Email and password are required." });
                }

                // Use an optimized query that only selects needed fields
                var user = await _context.RegisteredUsers
                    .AsNoTracking()
                    .Where(u => u.Email == loginRequest.Email)
                    .Select(u => new {
                        u.RegUserId,
                        u.Username,
                        u.Email,
                        u.Password,
                        u.Status
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: no user found with email {loginRequest.Email}");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                // Kiểm tra mật khẩu
                if (user.Password != loginRequest.Password)
                {
                    _logger.LogWarning($"Login failed: incorrect password for {loginRequest.Email}");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                _logger.LogInformation($"User login successful: {user.Username} (ID: {user.RegUserId})");

                return Ok(new
                {
                    message = "Login successful!",
                    isAuthenticated = true,
                    userId = user.RegUserId,
                    username = user.Username,
                    email = user.Email,
                    role = "RegisteredUser"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user login");
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }

        // Helper method to validate email format
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
    }

    public class UserDTO
    {
        public int RegUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

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