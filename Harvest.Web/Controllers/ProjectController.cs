using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ProjectController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
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
            var user = await _userService.GetCurrentUser();

            project.CreatedBy = user;
            project.Status = "Requested";

            _dbContext.Projects.Add(project);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
