using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            return await engine.CompileRenderStringAsync<string>("home/index.cshtml", null, null);
        }


        private static RazorLightEngine GetRazorEngine()
        {
            var path = Path.GetFullPath(".");

            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(path)
                .UseMemoryCachingProvider()
                .Build();

            return engine;
        }
    }
}
