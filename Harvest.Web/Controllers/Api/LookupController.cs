using Harvest.Core.Data;
using Harvest.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class LookupController : Controller
    {
        private readonly AppDbContext _dbContext;
        
        public LookupController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<ActionResult> SearchCrops(string type, string query)
        {
            //I think this is right, didn't test
            var crops = await _dbContext.CropLookups
                .Where(a => a.Type == type &&
                            (EF.Functions.Like(a.Crop, query.EfStartsWith()) ||
                             EF.Functions.Like(a.Crop, query.EfContains())))
                .OrderBy(a => a.Crop)
                .AsNoTracking()
                .Select(a => new {a.Crop})
                .ToListAsync();

            return Json(crops);
        }
    }
}
