using Microsoft.AspNetCore.Mvc;
using diendan2.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;

namespace diendan2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly DigitalOceanSpacesService _spacesService;

        public ImageController(DigitalOceanSpacesService spacesService)
        {
            _spacesService = spacesService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });

            try
            {
                // Validate file is an image
                string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (string.IsNullOrEmpty(ext) || !Array.Exists(permittedExtensions, e => e == ext))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image files are allowed." } });
                }

                // Generate a unique filename
                var fileName = $"{Guid.NewGuid()}{ext}";

                // Upload file to DigitalOcean Spaces
                var fileUrl = await _spacesService.UploadFileAsync(file, fileName);
                
                // Return response compatible with CKEditor
                return Ok(new 
                {
                    uploaded = 1,
                    fileName = fileName,
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        // For compatibility with CKEditor 4
        [HttpPost("Image")]
        public async Task<IActionResult> CKEditorUpload(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });

            try
            {
                // Validate file is an image
                string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                string ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
                
                if (string.IsNullOrEmpty(ext) || !Array.Exists(permittedExtensions, e => e == ext))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image files are allowed." } });
                }

                // Generate a unique filename
                var fileName = $"{Guid.NewGuid()}{ext}";

                // Upload file to DigitalOcean Spaces
                var fileUrl = await _spacesService.UploadFileAsync(upload, fileName);
                
                // Return response compatible with CKEditor
                return Ok(new 
                {
                    uploaded = 1,
                    fileName = fileName,
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        [HttpGet("url/{fileName}")]
        public IActionResult GetImageUrl(string fileName)
        {
            try
            {
                var fileUrl = _spacesService.GetFileUrl(fileName);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting file URL: {ex.Message}");
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be empty");
            }

            try
            {
                // Nếu fileName chứa đường dẫn đầy đủ, chỉ lấy tên file
                fileName = Path.GetFileName(fileName);
                
                await _spacesService.DeleteFileAsync(fileName);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }
    }
} 