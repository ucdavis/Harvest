using System;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;

namespace Harvest.Core.Services
{
    public interface IFileService
    {
        Uri GetDownloadUrl(string container = "upload");
        Uri GetUploadUrl(string container = "upload");
    }

    public class FileService : IFileService
    {
        private StorageSettings _storageSettings;

        public FileService(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings.Value;
        }

        public Uri GetDownloadUrl(string container)
        {
            return GetServiceSasUriForContainer(container, BlobContainerSasPermissions.Read);
        }

        public Uri GetUploadUrl(string container)
        {
            return GetServiceSasUriForContainer(container, BlobContainerSasPermissions.Write);
        }

        private Uri GetServiceSasUriForContainer(string container, BlobContainerSasPermissions permissions = BlobContainerSasPermissions.Read, DateTimeOffset? expiresOn = null)
        {
            // var endpoint = "TODO:AUTH";

            var client = new BlobContainerClient(endpoint, container);

            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (client.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = client.Name,
                    Resource = "c"
                };


                sasBuilder.ExpiresOn = expiresOn.HasValue ? expiresOn.Value : DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(permissions);

                return client.GenerateSasUri(sasBuilder);
            }
            else
            {
                throw new InvalidOperationException("BlobContainerClient must be authorized with Shared Key credentials to create a service SAS.");
            }
        }
    }
}