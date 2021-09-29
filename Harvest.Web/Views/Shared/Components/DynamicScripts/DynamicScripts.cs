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
             * Get the important JS files in the static build folder
             * Should include the following files:
             * - [num].[hash].chunk.js (determined from the generated index.html by CRA)
             * - main.*.js
             * - runtime-main.*.js

             * Other chunks do not need to be included here, since they will be dynamically requested only when needed

             =======
             We would like to inline the runtime-main file, then load 3.*.chunk.js, and then main.*.chunk.js
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

            var scripts = new List<string>();

            // Assumption: We need the chunk file that contains code from react.production.min.js
            // Assumption: The chunk file will always be accompanied by a corresponding [chunk file name].LICENSE.txt file that contains the string "react.production"
            // Assumption: No other LICENSE.txt file will contain "react.production" (though it shouldn't break anything if it does happen)
            var regex = new Regex("react\\.production");
            foreach (var fileName in files.Where(f => Regex.IsMatch(f.Name, "^[0-9]*\\..*\\.chunk\\.js\\.LICENSE.txt$")).Select(f => f.Name))
            {
                foreach (var line in File.ReadLines(fileName))
                {
                    if (regex.IsMatch(line))
                    {
                        scripts.Add(fileName.Replace(".LICENSE.txt", ""));
                        break;
                    }
                }
            }

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