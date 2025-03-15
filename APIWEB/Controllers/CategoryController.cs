using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using APIWEB.Models;

namespace APIWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly DBContextTest _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(DBContextTest context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy danh sách tất cả Category
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDTO { CategoryId = c.CategoryId, Name = c.Name })
                .ToListAsync();
            return Ok(categories);
        }

        // Lấy Category theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories
                .Select(c => new CategoryDTO { CategoryId = c.CategoryId, Name = c.Name })
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound(new { message = "Không tìm thấy Category." });
            }

            return Ok(category);
        }

        // Thêm Category mới
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] CreateCategoryDTO createCategoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Categories.AnyAsync(c => c.Name == createCategoryDTO.Name))
            {
                return BadRequest(new { message = "Lỗi: Tên danh mục đã tồn tại. Vui lòng chọn tên khác." });
            }

            var category = new Category { Name = createCategoryDTO.Name };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm danh mục thành công", category = new CategoryDTO { CategoryId = category.CategoryId, Name = category.Name } });
        }


        // Cập nhật Category
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO updateCategoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound(new { message = "Không tìm thấy Category." });
            }

            if (await _context.Categories.AnyAsync(c => c.Name == updateCategoryDTO.Name && c.CategoryId != id))
            {
                return BadRequest("Tên Category đã tồn tại.");
            }

            category.Name = updateCategoryDTO.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật danh mục thành công", category = new CategoryDTO { CategoryId = category.CategoryId, Name = category.Name } });
        }

        // Xóa Category
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound("Không tìm thấy Category.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa danh mục thành công." });
        }

        public class CategoryDTO
        {
            public int CategoryId { get; set; }
            public required string Name { get; set; }
        }

        public class CreateCategoryDTO
        {
            [Required(ErrorMessage = "Tên Category không được để trống.")]
            public required string Name { get; set; }
        }

        public class UpdateCategoryDTO
        {
            [Required(ErrorMessage = "Tên Category không được để trống.")]
            public required string Name { get; set; }
        }
    }
}