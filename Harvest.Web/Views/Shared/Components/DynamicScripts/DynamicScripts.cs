using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Harvest.Web.Views.Shared.Components.DynamicScripts
{
    [ViewComponent(Name = "DynamicScripts")]
    public class DynamicScripts : ViewComponent
    {
        private readonly IFileProvider _fileProvider;

        public DynamicScripts(IFileProvider fileProvider)
        {
            this._fileProvider = fileProvider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var files = _fileProvider.GetDirectoryContents("ClientApp/build/static/js");

            /* 
             * Get all JS files in the static build folder
             * Should include the following files:
             * - main.*.js
             * - 2.[hash].chunk.js
             * - runtime-main.*.js
             * - [number].[hash].chunk.js (for each additional chunk)

             =======
             We would like to inline the runtime-main file, then load 2.*.chunk.js, and then main.*.chunk.js
            */

            var model = new DynamicScriptModel();
            var runtimeFile = files.Where(f => Regex.IsMatch(f.Name, "^.*runtime-main.*\\.js$")).First();

            // read the entire runtime-main file
            using (var reader = runtimeFile.CreateReadStream())
            {
                using (var streamReader = new StreamReader(reader))
                {
                    // we only care about the first line, which is just a bunch of minified JS
                    model.Runtime = await streamReader.ReadLineAsync();
                }
            }

            // Get all the chunk files
            var chunks = files.Where(f => Regex.IsMatch(f.Name, "^[0-9]*\\..*\\.chunk\\.js$")).Select(f => f.Name);

            var scripts = new List<string>();

            // read the 3.*.chunk.js files (maybe there is always just one?)
            scripts.AddRange(chunks.Where(c => c.StartsWith("3")));

            // now add in main.*.chunk.js
            scripts.AddRange(files.Where(f => Regex.IsMatch(f.Name, "^.*main\\..*\\.chunk\\.js$")).Select(f => f.Name));

            model.Scripts = scripts.ToArray();

            return View(model);
        }
    }

    public class DynamicScriptModel
    {
        public string[] Scripts { get; set; }
        public string Runtime { get; set; }
    }
}