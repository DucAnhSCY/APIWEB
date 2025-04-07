using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;

namespace diendan2.Services
{
    public class DigitalOceanSpacesService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _endpoint;

        public DigitalOceanSpacesService(IConfiguration configuration)
        {
            var config = configuration.GetSection("DigitalOcean");
            var accessKey = config["AccessKey"];
            var secretKey = config["SecretKey"];
            _endpoint = config["Endpoint"];
            _bucketName = config["BucketName"];

            var s3Config = new AmazonS3Config
            {
                ServiceURL = _endpoint,
                ForcePathStyle = true, // Use path-style for DigitalOcean Spaces
                UseHttp = true
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 10MB limit");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Allowed types: JPG, JPEG, PNG, GIF");

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            try
            {
                await _s3Client.PutObjectAsync(request);
                return GetFileUrl(fileName);
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Error uploading to DigitalOcean Spaces: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
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
                var exists = await _s3Client.GetObjectMetadataAsync(_bucketName, fileName).ConfigureAwait(false);
                if (exists == null)
                {
                    throw new Exception($"File '{fileName}' does not exist in bucket '{_bucketName}'");
                }

                await _s3Client.DeleteObjectAsync(request).ConfigureAwait(false);

                var stillExists = await _s3Client.GetObjectMetadataAsync(_bucketName, fileName).ConfigureAwait(false);
                if (stillExists != null)
                {
                    throw new Exception($"File '{fileName}' could not be deleted from bucket '{_bucketName}'");
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"File '{fileName}' not found, already deleted.");
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Error deleting from DigitalOcean Spaces: {ex.Message}", ex);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The operation was canceled.");
            }
        }


        public string GetFileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty");

            return $"{_endpoint}/{_bucketName}/{fileName}";
        }
    }
} 