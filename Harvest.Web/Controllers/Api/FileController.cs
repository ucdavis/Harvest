using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class FileController : SuperController
    {
        private readonly StorageSettings storageSettings;
        private readonly IFileService fileService;

        public FileController(IOptions<StorageSettings> storageSettings, IFileService fileService)
        {
            this.storageSettings = storageSettings.Value;
            this.fileService = fileService;
        }

        [HttpGet]
        [Route("/api/{controller}/{action}")]
        public string GetUploadDetails()
        {
            return fileService.GetUploadUrl(storageSettings.ContainerName).AbsoluteUri;
        }

        [HttpGet]
        [Route("/api/{controller}/{action}")]
        public string GetReadDetails()
        {
            // TODO: need to know what blob they want to read
            return fileService.GetDownloadUrl(storageSettings.ContainerName, "image.png").AbsoluteUri;
        }
    }
}