using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ProjectController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ProjectController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }
        public ActionResult Index()
        {
            // // TODO: only show user's projects
            // var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            // return View(projects);
            return View("React");
        }

        // TODO: move or determine proper permissions
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Where(p => p.IsActive).ToArrayAsync());
        }

        // TODO: move or determine proper permissions
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Invoices(int projectId)
        {
            return Ok(await _dbContext.Invoices.Where(a => a.ProjectId == projectId).ToArrayAsync());
        }

        public ActionResult Details(int projectId)
        {
            // TODO: move routes so react handles this natively and place API stuff in own controller
            return View("React");
        }

        // TODO: permissions
        // Returns JSON info of the project
        public async Task<ActionResult> Get(int projectId)
        {
            return Ok(await _dbContext.Projects.Include(a => a.Attachments).Include(p => p.Accounts).Include(p => p.PrincipalInvestigator).Include(p => p.CreatedBy).SingleAsync(p => p.Id == projectId));
        }

        [HttpGet]
        public async Task<ActionResult> AccountApproval(int projectId)
        {
            var project = await _dbContext.Projects.Include(p => p.Accounts).SingleAsync(p => p.Id == projectId);

            return View(project);
        }

        public async Task<IActionResult> RefreshTotal(int projectId)
        {
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId);
            var invoiceTotal = await _dbContext.Invoices.Where(a =>
                    a.ProjectId == projectId &&
                    (a.Status == Invoice.Statuses.Pending || a.Status == Invoice.Statuses.Completed)).Select(a => a.Total).SumAsync();
            var originalTotal = project.ChargedTotal;
            if (project.ChargedTotal != invoiceTotal)
            {
                project.ChargedTotal = invoiceTotal;
                await _dbContext.SaveChangesAsync();
                return Content($"Project total updated from {originalTotal} to {project.ChargedTotal}");
            }

            return Content("Project already up to date.");

        }
    }
}
