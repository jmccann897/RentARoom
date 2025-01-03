using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Utility;

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

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var containerClient = GetContainerClient();
            var blobClient = containerClient.GetBlobClient(file.FileName);

            // Upload the file to Azure Blob Storage
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            // Return the URL of the uploaded file
            return blobClient.Uri.AbsoluteUri;
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
