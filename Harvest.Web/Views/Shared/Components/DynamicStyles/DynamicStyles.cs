using System.IO;
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
            // Get the CRA generated index file, which includes optimized scripts
            var indexPage = _fileProvider.GetFileInfo("ClientApp/build/index.html");

            // read the file
            var fileContents = await File.ReadAllTextAsync(indexPage.PhysicalPath);

            // find all link tags with the rel attribute set to stylesheet
            var styleSheetLinks = Regex.Matches(fileContents, "<link.*rel=\"stylesheet\".*>", RegexOptions.IgnoreCase);

            var styleLinksAsString = styleSheetLinks.Cast<Match>().Select(m => m.Value).ToList();

            return View(styleSheetLinks.Select(m => m.Value).ToArray());
        }
    }
}