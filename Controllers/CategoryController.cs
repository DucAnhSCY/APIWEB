using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using diendan2.Models2;
using diendan2;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly DBContextTest2 _context;

    public CategoryController(DBContextTest2 context)
    {
        _context = context;
    }

    // ✅ Get all categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }

    // ✅ Get category by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }
        return Ok(category);
    }

    // ✅ Create a new category (Admin only)
    [HttpPost("Insert")]
    public async Task<IActionResult> CreateCategory([FromQuery] string name)
    {
        var user = User.Identity.Name; // Check logged-in user
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Console.WriteLine($"User: {user}, Roles: {string.Join(", ", roles)}");

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Category name cannot be empty." });
        }

        if (_context.Categories.Any(c => c.Name.ToLower() == name.ToLower()))
        {
            return BadRequest(new { message = "Category name already exists." });
        }

        var category = new Category { Name = name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, category);
    }


    // ✅ Update an existing category
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromQuery] string name)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Category name cannot be empty." });
        }
        if (_context.Categories.Any(c => c.Name.ToLower() == name.ToLower() && c.CategoryId != id))
        {
            return BadRequest(new { message = "Category name already exists." });
        }

        category.Name = name;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category updated successfully." });
    }

    // ✅ Delete a category (Admin only)
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully." });
    }
}
public class CategoryDTO
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
}

