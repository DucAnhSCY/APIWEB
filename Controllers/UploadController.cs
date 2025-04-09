using Microsoft.AspNetCore.Mvc;
using diendan2.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace diendan2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly DigitalOceanSpacesService _spacesService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public UploadController(DigitalOceanSpacesService spacesService, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _spacesService = spacesService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("config")]
        public IActionResult GetSpacesConfig()
        {
            try
            {
                var config = _configuration.GetSection("DigitalOcean");
                return Ok(new
                {
                    Endpoint = config["Endpoint"],
                    BucketName = config["BucketName"],
                    Region = config["Region"],
                    // Don't return actual keys but confirm they exist
                    HasAccessKey = !string.IsNullOrEmpty(config["AccessKey"]),
                    HasSecretKey = !string.IsNullOrEmpty(config["SecretKey"]),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving config: {ex.Message}" });
            }
        }

        // For local file system fallback when DigitalOcean fails
        [HttpPost("local-image")]
        public async Task<IActionResult> UploadLocalImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });

            try
            {
                // Validate file is an image
                if (!IsImageFile(file.FileName))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image files are allowed." } });
                }

                // Create uploads/images directory if it doesn't exist
                string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate URL for the saved image
                string url = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";

                return Ok(new
                {
                    uploaded = 1,
                    fileName = fileName,
                    url = url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });

            try
            {
                // Validate file is an image
                if (!IsImageFile(file.FileName))
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image files are allowed." } });
                }

                // Generate a unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";

                try
                {
                    // Try to upload file to DigitalOcean Spaces
                    var fileUrl = await _spacesService.UploadFileAsync(file, fileName);
                    
                    // Return response compatible with CKEditor
                    return Ok(new 
                    {
                        uploaded = 1,
                        fileName = fileName,
                        url = fileUrl
                    });
                }
                catch (Exception doEx)
                {
                    // Log the detailed DigitalOcean error
                    Console.WriteLine($"DigitalOcean upload failed: {doEx.Message}");
                    
                    // Fall back to local file storage
                    string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    string url = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";

                    return Ok(new
                    {
                        uploaded = 1,
                        fileName = fileName,
                        url = url,
                        note = "Used local storage due to DigitalOcean error"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        [HttpPost("video")]
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
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
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

        // For compatibility with CKEditor 4
        [HttpPost("ckeditor")]
        public async Task<IActionResult> CKEditorUpload(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest(new { uploaded = 0, error = new { message = "No file uploaded." } });

            try
            {
                // Check if it's an image or video file
                string ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
                bool isImage = IsImageFile(upload.FileName);
                bool isVideo = IsVideoFile(upload.FileName);

                if (!isImage && !isVideo)
                {
                    return BadRequest(new { uploaded = 0, error = new { message = "Invalid file type. Only image or video files are allowed." } });
                }

                if (isImage)
                {
                    try
                    {
                        // Try DigitalOcean first
                        // Generate a unique filename for image
                        var fileName = $"{Guid.NewGuid()}{ext}";

                        // Upload image to DigitalOcean Spaces
                        var fileUrl = await _spacesService.UploadFileAsync(upload, fileName);
                        
                        // Return response compatible with CKEditor
                        return Ok(new 
                        {
                            uploaded = 1,
                            fileName = fileName,
                            url = fileUrl
                        });
                    }
                    catch (Exception doEx)
                    {
                        // Log the error and fall back to local storage
                        Console.WriteLine($"DigitalOcean upload failed: {doEx.Message}");
                        
                        // Fall back to local file storage
                        string fileName = $"{Guid.NewGuid()}{ext}";
                        string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await upload.CopyToAsync(fileStream);
                        }

                        string url = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";

                        return Ok(new
                        {
                            uploaded = 1,
                            fileName = fileName,
                            url = url
                        });
                    }
                }
                else // isVideo
                {
                    // Check file size (max 50MB)
                    if (upload.Length > 50 * 1024 * 1024)
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
                    string uniqueFileName = $"{Guid.NewGuid()}_{upload.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await upload.CopyToAsync(fileStream);
                    }

                    // Generate URL for the saved video
                    string url = $"{Request.Scheme}://{Request.Host}/uploads/videos/{uniqueFileName}";

                    // Return response compatible with CKEditor
                    return Ok(new
                    {
                        uploaded = 1,
                        fileName = uniqueFileName,
                        url
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uploaded = 0, error = new { message = $"Error uploading file: {ex.Message}" } });
            }
        }

        [HttpGet("url/{fileName}")]
        public IActionResult GetFileUrl(string fileName)
        {
            try
            {
                // Check if it's likely a video file
                if (IsVideoFile(fileName))
                {
                    // Generate URL for video file
                    string url = $"{Request.Scheme}://{Request.Host}/uploads/videos/{fileName}";
                    return Ok(new { url });
                }
                else if (IsImageFile(fileName))
                {
                    // Try to get from DigitalOcean first
                    try
                    {
                        var fileUrl = _spacesService.GetFileUrl(fileName);
                        return Ok(new { url = fileUrl });
                    }
                    catch
                    {
                        // If that fails, check if it exists locally
                        string localPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "images", fileName);
                        if (System.IO.File.Exists(localPath))
                        {
                            string url = $"{Request.Scheme}://{Request.Host}/uploads/images/{fileName}";
                            return Ok(new { url });
                        }
                        else
                        {
                            return NotFound(new { message = "File not found in any storage location" });
                        }
                    }
                }
                else
                {
                    return BadRequest(new { message = "Unsupported file type" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting file URL: {ex.Message}");
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be empty");
            }

            try
            {
                // Check if it's likely a video file
                if (IsVideoFile(fileName))
                {
                    // Delete video file from local storage
                    string filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "videos", fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        return Ok(new { message = "Video file deleted successfully" });
                    }
                    else
                    {
                        return NotFound(new { message = "Video file not found" });
                    }
                }
                else if (IsImageFile(fileName))
                {
                    // First try to delete from DigitalOcean
                    fileName = Path.GetFileName(fileName);
                    bool deletedFromDO = false;
                    
                    try
                    {
                        await _spacesService.DeleteFileAsync(fileName);
                        deletedFromDO = true;
                    }
                    catch (Exception doEx)
                    {
                        Console.WriteLine($"Error deleting from DigitalOcean: {doEx.Message}");
                    }
                    
                    // Also check local storage
                    string localPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "images", fileName);
                    bool deletedLocal = false;
                    
                    if (System.IO.File.Exists(localPath))
                    {
                        System.IO.File.Delete(localPath);
                        deletedLocal = true;
                    }
                    
                    if (deletedFromDO || deletedLocal)
                    {
                        return Ok(new { message = "Image file deleted successfully" });
                    }
                    else
                    {
                        return NotFound(new { message = "Image file not found in any storage location" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Unsupported file type" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        private bool IsImageFile(string fileName)
        {
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
        }

        private bool IsVideoFile(string fileName)
        {
            string[] permittedExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
        }
    }
} 