using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Web.Models;
using Microsoft.Diagnostics.Tracing;
using RazorLight;

namespace Harvest.Web.Services
{
    public interface IEmailBodyService
    {
        Task<string> BuildEmailBody();
    }

    public class EmailBodyService : IEmailBodyService
    {


        public async Task<string> BuildEmailBody()
        {
            var engine = GetRazorEngine();

            var model = new EmailModel
            {
                Name = "Bobby"
            };


            var yyy = await engine.CompileRenderAsync("Test.cshtml", model);

            return yyy;
        }


        private static RazorLightEngine GetRazorEngine()
        {
            var path = Path.GetFullPath("./Emails");

            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(path)
                .UseMemoryCachingProvider()             
                .Build();

            return engine;
        }
    }
}
