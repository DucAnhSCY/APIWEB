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
    public class VideoController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public VideoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });
            }

            try
            {
                // Check if file is a video
                if (!IsVideoFile(file.FileName))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only video files are allowed." } });
                }

                // Check file size (max 50MB)
                if (file.Length > 50 * 1024 * 1024)
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "File size exceeds the limit of 50MB." } });
                }

                // Create uploads/videos directory if it doesn't exist
                string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "videos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate URL for the saved video
                string url = $"{Request.Scheme}://{Request.Host}/uploads/videos/{uniqueFileName}";

                // Return response in format expected by the client
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

        private bool IsVideoFile(string fileName)
        {
            string[] permittedExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && Array.Exists(permittedExtensions, e => e == ext);
        }
    }
}