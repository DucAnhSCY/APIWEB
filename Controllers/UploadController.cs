using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace diendan2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("Image")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });
            }

            try
            {
                // Check if file is an image
                if (!IsImageFile(upload.FileName))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image files are allowed." } });
                }

                // Create uploads directory if it doesn't exist
                string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name
                string uniqueFileName = $"{Guid.NewGuid()}_{upload.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }

                // Generate URL for the saved image
                string url = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";

                // Return response in format expected by CKEditor
                return Ok(new
                {
                    uploaded = 1,
                    fileName = uniqueFileName,
                    url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        private bool IsImageFile(string fileName)
        {
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && Array.Exists(permittedExtensions, e => e == ext);
        }
    }
}