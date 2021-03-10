using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Harvest.Web
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

            // Get all JS files
            // TODO: We might not actually need all of them since some of the chunks can be loaded later.  But it shouldn't hurt for now.
            var fileNames = files.Where(f => Regex.IsMatch(f.Name, "^.*\\.js$")).OrderByDescending(f => f.Name).Select(f => f.Name);
            return View(await Task.FromResult(fileNames.ToArray()));
        }
    }
}