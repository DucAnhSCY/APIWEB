using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace diendan2.Services
{
    public class SpacesService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public SpacesService()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = "https://blr1.digitaloceanspaces.com" // Endpoint URL
            };
            
            _s3Client = new AmazonS3Client(
                "dop_v1_4694320f44375bb89dcd5d4b735a8d6c47a947da7b649a8249468d63fa7348ab", // API Token (Access Key)
                "", // Secret Key not needed with DO Spaces API Token
                config
            );
            
            _bucketName = "luutruanh"; // Space name
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = $"images/{fileName}", // Store in the "images/" folder
                BucketName = _bucketName,
                CannedACL = S3CannedACL.PublicRead // Allow public access
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return $"https://{_bucketName}.blr1.digitaloceanspaces.com/images/{fileName}";
        }
    }
} 