using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ProjectController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<ActionResult> Index()
        {
            // TODO: only show user's projects
            var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            return View(projects);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Project project) {
            // TODO: validation!
            _dbContext.Projects.Add(project);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
