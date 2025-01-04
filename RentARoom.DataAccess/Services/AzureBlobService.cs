using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using Image = SixLabors.ImageSharp.Image;
using System.IO;

namespace RentARoom.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        // Method to get the BlobServiceClient instance
        // Blob for file storage
        public BlobServiceClient GetBlobServiceClient()
        {
            return new BlobServiceClient(_connectionString);
        }

        // Method to get the BlobContainerClient instance
        // Container = file - base level storage
        public BlobContainerClient GetContainerClient()
        {
            var blobServiceClient = GetBlobServiceClient();
            return blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<string> UploadFileAsync(IFormFile file, int maxWidth = 1920, int maxHeight = 1080)
        {
            var containerClient = GetContainerClient();

            // Generate webp filename
            var webpFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}.webp";

            // Process the image and resize it
            using (var imageStream = file.OpenReadStream())
            using (var image = Image.Load(imageStream))
            {
                // Resize image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight)
                }));


                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, new WebpEncoder { Quality = 80 });
                    memoryStream.Position = 0;

                    // Upload processed image
                    var blobClient = containerClient.GetBlobClient(webpFileName);

                    await blobClient.UploadAsync(memoryStream, overwrite: true);

                    await blobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = "image/webp" });

                    // Return the URL of the uploaded file
                    return blobClient.Uri.AbsoluteUri;
                }
            }
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var containerClient = GetContainerClient();
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<IEnumerable<string>> ListFilesAsync()
        {
            var containerClient = GetContainerClient();
            var fileUrls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                fileUrls.Add(blobItem.Name);
            }

            return fileUrls;
        }
    }
}
