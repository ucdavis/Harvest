using System;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Harvest.Core.Services
{
    public class FileService
    {
        public FileService()
        {

        }

        public Uri GetUploadUrl(string container = "upload")
        {
            var endpoint = "TODO:AUTH";

            var client = new BlobContainerClient(endpoint, container);

            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (client.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = client.Name,
                    Resource = "c"
                };

                // allow writing for 1 hr
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);

                return client.GenerateSasUri(sasBuilder);
            }
            else
            {
                throw new InvalidOperationException("BlobContainerClient must be authorized with Shared Key credentials to create a service SAS.");
            }
        }
    }
}