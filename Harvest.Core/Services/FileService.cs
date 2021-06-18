using System;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;

namespace Harvest.Core.Services
{
    public interface IFileService
    {
        Uri GetDownloadUrl(string container, string blob);
        Uri GetUploadUrl(string container);
    }
    
    public class FileService : IFileService
    {
        private StorageSettings _storageSettings;

        public FileService(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings.Value;
        }

        public Uri GetDownloadUrl(string container, string blob)
        {
            var client = new BlobClient(_storageSettings.Endpoint, container, blob);

            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container,
                BlobName = blob,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return client.GenerateSasUri(sasBuilder);
        }

        public Uri GetUploadUrl(string container)
        {
            var client = new BlobContainerClient(_storageSettings.Endpoint, container);

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container,
                Resource = "c",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);


            return client.GenerateSasUri(sasBuilder);
        }
    }
}