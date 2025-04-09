using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace diendan2.Services
{
    public class DigitalOceanSpacesService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly IConfiguration _configuration;
        private bool _isConfigValid = false;

        public DigitalOceanSpacesService(IConfiguration configuration)
        {
            _configuration = configuration;
            var config = configuration.GetSection("DigitalOcean");
            var accessKey = config["AccessKey"];
            var secretKey = config["SecretKey"];
            _endpoint = config["Endpoint"];
            _bucketName = config["BucketName"];

            // Validate configuration
            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || 
                string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_bucketName))
            {
                Console.WriteLine("DigitalOcean Spaces configuration is incomplete. Some settings are missing.");
                _isConfigValid = false;
            }
            else
            {
                _isConfigValid = true;
            }

            var s3Config = new AmazonS3Config
            {
                ServiceURL = _endpoint,
                ForcePathStyle = true, // Changed to true for DigitalOcean Spaces compatibility
                UseHttp = false
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        public async Task<bool> ValidateCredentialsAsync()
        {
            if (!_isConfigValid)
            {
                return false;
            }

            try
            {
                // Try a simple operation to validate credentials
                var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    MaxKeys = 1
                });
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error validating credentials: {ex.Message} (ErrorCode: {ex.ErrorCode})");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error validating credentials: {ex.Message}");
                return false;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (!_isConfigValid)
            {
                throw new InvalidOperationException("DigitalOcean Spaces configuration is invalid or incomplete.");
            }

            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Filter file type based on extension for security
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedImageExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"Invalid file type '{fileExtension}'. Allowed types: JPG, JPEG, PNG, GIF, BMP");
            }

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = GetContentType(fileExtension),
                CannedACL = S3CannedACL.PublicRead
            };

            try
            {
                // First validate credentials
                bool credentialsValid = await ValidateCredentialsAsync();
                if (!credentialsValid)
                {
                    throw new InvalidOperationException("DigitalOcean Spaces credentials are invalid or bucket does not exist.");
                }

                await _s3Client.PutObjectAsync(request);
                
                // Double-check that file was uploaded
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, fileName);
                }
                catch
                {
                    throw new Exception("File upload failed verification. The file was not found after upload.");
                }
                
                return GetFileUrl(fileName);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error Code: {ex.ErrorCode}, Status Code: {ex.StatusCode}, Message: {ex.Message}");
                if (ex.ErrorCode == "AccessDenied" || ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
                {
                    throw new Exception($"Authentication error with DigitalOcean Spaces: {ex.Message} (ErrorCode: {ex.ErrorCode}). Please check your credentials.");
                }
                throw new Exception($"Error uploading to DigitalOcean Spaces: {ex.Message} (ErrorCode: {ex.ErrorCode})", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw new Exception($"Error uploading to DigitalOcean Spaces: {ex.Message}", ex);
            }
        }

        // Get appropriate content type based on file extension
        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png", 
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (!_isConfigValid)
            {
                throw new InvalidOperationException("DigitalOcean Spaces configuration is invalid or incomplete.");
            }

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty");

            if (fileName.StartsWith("http"))
            {
                var uri = new Uri(fileName);
                fileName = Path.GetFileName(uri.AbsolutePath);
            }
            else if (fileName.Contains("/"))
            {
                fileName = Path.GetFileName(fileName);
            }

            fileName = fileName.Replace("\\", "").Replace("/", "");

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            try
            {
                // First validate credentials
                bool credentialsValid = await ValidateCredentialsAsync();
                if (!credentialsValid)
                {
                    throw new InvalidOperationException("DigitalOcean Spaces credentials are invalid or bucket does not exist.");
                }

                // Check if file exists before attempting to delete
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, fileName).ConfigureAwait(false);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException($"File '{fileName}' does not exist in bucket '{_bucketName}'");
                }

                await _s3Client.DeleteObjectAsync(request).ConfigureAwait(false);

                // Verify deletion
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, fileName).ConfigureAwait(false);
                    throw new Exception($"File '{fileName}' could not be deleted from bucket '{_bucketName}'");
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // This is expected - the file should be gone
                    return;
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"File '{fileName}' not found, already deleted.");
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "AccessDenied" || ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
                {
                    throw new Exception($"Authentication error with DigitalOcean Spaces: {ex.Message} (ErrorCode: {ex.ErrorCode}). Please check your credentials.");
                }
                throw new Exception($"Error deleting from DigitalOcean Spaces: {ex.Message}", ex);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The operation was canceled.");
            }
        }

        public string GetFileUrl(string fileName)
        {
            if (!_isConfigValid)
            {
                throw new InvalidOperationException("DigitalOcean Spaces configuration is invalid or incomplete.");
            }

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty");

            return $"{_endpoint}/{_bucketName}/{fileName}";
        }
    }
}