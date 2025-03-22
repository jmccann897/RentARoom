using Microsoft.AspNetCore.Http;
using RentARoom.Services.IServices;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;

namespace RentARoom.Tests.RentARoom.IntegrationTests
{
    public class AzureBlobServiceIntegrationTests : IAsyncLifetime
    {
        private readonly AzureBlobService _service;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobServiceIntegrationTests()
        {
            // Configure to use Azurite's connection string and a test container.
            var inMemorySettings = new Dictionary<string, string>
            {
                { "AzureBlobStorage:ConnectionString", "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;" },
                { "AzureBlobStorage:ContainerName", "testcontainer" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new AzureBlobService(configuration);
            _containerClient = _service.GetContainerClient();
        }

        public async Task InitializeAsync()
        {
            // Create the test container if it doesn't exist.
            await _containerClient.CreateIfNotExistsAsync();
        }

        public async Task DisposeAsync()
        {
            // Delete the test container after the tests.
            await _containerClient.DeleteIfExistsAsync();
        }

        [Fact]
        public async Task AzureBlobService_UploadFileAsync_Should_UploadAndReturnUrl()
        {
            // Arrange
            // Load a real image file from disk (replace with your file path)
            byte[] imageData = File.ReadAllBytes("test-rentaroom.png");
            var mockFormFile = new MockFormFile("test-rentaroom.png", "image/png", imageData);

            // Act
            var result = await _service.UploadFileAsync(mockFormFile);

            // Assert
            Assert.StartsWith("http://127.0.0.1:10000/devstoreaccount1/testcontainer/", result);

            // Cleanup
            var blobName = result.Replace("http://127.0.0.1:10000/devstoreaccount1/testcontainer/", "");
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        [Fact]
        public async Task AzureBlobService_UploadFileAsync_Should_ThrowException_IfEmptyFile()
        {
            // Arrange
            byte[] emptyData = new byte[0];
            var mockFormFile = new MockFormFile("empty.txt", "text/plain", emptyData);

            // Act & Assert
            await Assert.ThrowsAsync<SixLabors.ImageSharp.UnknownImageFormatException>(() => _service.UploadFileAsync(mockFormFile));
        }

        [Fact]
        public async Task AzureBlobService_UploadFileAsync_Should_ThrowException_IfInvalidFileFormat()
        {
            // Arrange
            byte[] invalidData = System.Text.Encoding.UTF8.GetBytes("This is not an image.");
            var mockFormFile = new MockFormFile("invalid.txt", "text/plain", invalidData);

            // Act & Assert
            await Assert.ThrowsAsync<SixLabors.ImageSharp.UnknownImageFormatException>(() => _service.UploadFileAsync(mockFormFile));
        }

        [Fact]
        public async Task AzureBlobService_DeleteFileAsync_Should_DeleteFile_IfExists()
        {
            // Arrange
            byte[] imageData = File.ReadAllBytes("test-rentaroom.png");
            var mockFormFile = new MockFormFile("test-rentaroom.png", "image/png", imageData);
            var uploadResult = await _service.UploadFileAsync(mockFormFile);
            var blobName = uploadResult.Replace("http://127.0.0.1:10000/devstoreaccount1/testcontainer/", "");

            // Act
            await _service.DeleteFileAsync(blobName);

            // Assert
            var blobClient = _containerClient.GetBlobClient(blobName);
            var exists = await blobClient.ExistsAsync();
            Assert.False(exists.Value);
        }

        [Fact]
        public async Task AzureBlobService_DeleteFileAsync_Should_NotThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFileName = "nonexistentfile.txt";

            // Act
            await _service.DeleteFileAsync(nonExistentFileName);

            // Assert
            // In azure blob storage, the delete method does not throw an exception if the file does not exist.
            // We use the deleteIfExists method.
            Assert.True(true);
        }

        [Fact]
        public async Task AzureBlobService_ListFilesAsync_Should_ReturnListOfFileNames()
        {
            // Arrange
            byte[] imageData1 = File.ReadAllBytes("test-rentaroom.png");
            byte[] imageData2 = File.ReadAllBytes("test-rentaroom.png");
            var mockFormFile1 = new MockFormFile("file1.png", "image/png", imageData1);
            var mockFormFile2 = new MockFormFile("file2.png", "image/png", imageData2);

            await _service.UploadFileAsync(mockFormFile1);
            await _service.UploadFileAsync(mockFormFile2);

            // Act
            var fileNames = await _service.ListFilesAsync();

            // Assert
            Assert.Contains(fileNames, name => name.Contains("file1"));
            Assert.Contains(fileNames, name => name.Contains("file2"));

            // Cleanup
            foreach (var fileName in fileNames)
            {
                await _containerClient.GetBlobClient(fileName).DeleteIfExistsAsync();
            }
        }
    }

    // Helper class to create a mock IFormFile
    public class MockFormFile : IFormFile
    {
        private readonly byte[] _content;

        public MockFormFile(string fileName, string contentType, byte[] content)
        {
            FileName = fileName;
            ContentType = contentType;
            _content = content;
            Length = _content.Length;
        }

        public string ContentType { get; }
        public string ContentDisposition => null;
        public IHeaderDictionary Headers => null;
        public long Length { get; }
        public string Name => null;
        public string FileName { get; }

        public Stream OpenReadStream()
        {
            return new MemoryStream(_content);
        }

        public void CopyTo(Stream target)
        {
            using (var stream = new MemoryStream(_content))
            {
                stream.CopyTo(target);
            }
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            using (var stream = new MemoryStream(_content))
            {
                await stream.CopyToAsync(target, cancellationToken);
            }
        }
    }
}