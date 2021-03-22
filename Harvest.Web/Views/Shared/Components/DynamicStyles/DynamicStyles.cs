using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Harvest.Web.Views.Shared.Components.DynamicStyles
{
    [ViewComponent(Name = "DynamicStyles")]
    public class DynamicStyles : ViewComponent
    {
        private readonly IFileProvider _fileProvider;

        public DynamicStyles(IFileProvider fileProvider)
        {
            this._fileProvider = fileProvider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var files = _fileProvider.GetDirectoryContents("ClientApp/build/static/css");

            // Get all CSS files
            // TODO: does loading order matter?  is there ever more than one file?
            var fileNames = files.Where(f => Regex.IsMatch(f.Name, "^.*\\.css$")).OrderByDescending(f => f.Name).Select(f => f.Name);
            return View(await Task.FromResult(fileNames.ToArray()));
        }
    }
}