using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Services.IServices
{
    public interface IAzureBlobService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task DeleteFileAsync(string fileName);
        Task<IEnumerable<string>> ListFilesAsync();

        BlobContainerClient GetContainerClient();
    }
}
