using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class RequestController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public RequestController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }

        // Get info on the project as well as current proposed quote
        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            return Ok(await _dbContext.Projects.Include(p => p.PrincipalInvestigator).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id));
        }

        // create a new request via react
        [HttpGet]
        public ActionResult Create()
        {
            return View("React");
        }

        // Approve a quote for the project
        [HttpGet]
        public ActionResult Approve(int id)
        {
            return View("React");
        }

        [HttpPost]
        public async Task<ActionResult> ApproveAsync(int id, [FromBody] RequestApprovalModel model)
        {
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == id);

            // get the currently unapproved quote for this project
            var quote = await _dbContext.Quotes.SingleAsync(q => q.ProjectId == id && q.ApprovedOn == null);
            
            var currentUser = await _userService.GetCurrentUser();

            // TODO: double check the percentages add up to 100%
            // TODO: add in fiscal officer info??
            foreach (var account in model.Accounts)
            {
                account.ProjectId = id;

                // Accounts will be auto-approved by quote approver
                account.ApprovedById = currentUser.Id;
                account.ApprovedOn = DateTime.UtcNow;
            }

            var quoteDetail = QuoteDetail.Deserialize(quote.Text);

            project.Acres = quoteDetail.Acres;
            project.AcreageRateId = quoteDetail.AcreageRateId;

            project.Accounts = new List<Account>(model.Accounts);
            project.Status = Project.Statuses.Active;
            project.QuoteId = quote.Id;
            project.QuoteTotal = quote.Total;

            foreach (var quoteField in quoteDetail.Fields)
            {
                // now that fields are locked in, add to new db fields
                // SQL Server requires polygons be counter clock-wise, so if it isn't then reverse it
                var poly = quoteField.Geometry;
                if (!poly.Shell.IsCCW) {
                    poly = (Polygon)poly.Reverse();
                }

                var field = new Field { Crop = quoteField.Crop, Details = quoteField.Details, IsActive = true, ProjectId = project.Id, Location = poly };

                project.Fields.Add(field);
            }

            // TODO: Maybe inactivate instead?
            // remove any existing accounts that we no longer need
            _dbContext.RemoveRange(_dbContext.Accounts.Where(x => x.ProjectId == id));

            await _dbContext.SaveChangesAsync();

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Project project)
        {
            var currentUser = await _userService.GetCurrentUser();

            var newProject = new Project
            {
                Crop = project.Crop,
                CropType = project.CropType,
                Start = project.Start,
                End = project.End,
                CreatedOn = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                IsActive = true,
                Requirements = project.Requirements,
                Status = Project.Statuses.Requested
            };

            if (project.Id > 0)
            {
                // this project already exists, so we are requesting a change
                newProject.Status = Project.Statuses.ChangeRequested;
                newProject.OriginalProjectId = project.Id;
            }

            // create PI if needed and assign to project
            var pi = await _dbContext.Users.SingleOrDefaultAsync(x => x.Iam == project.PrincipalInvestigator.Iam);
            string piName;
            if (pi != null)
            {
                newProject.PrincipalInvestigatorId = pi.Id;
                piName = pi.Name;
            }
            else
            {
                // TODO: if PI doesn't exist we'll just use what our client sent.  We may instead want to re-query to ensure the most up to date info?
                newProject.PrincipalInvestigator = project.PrincipalInvestigator;
                piName = project.PrincipalInvestigator.Name;
            }

            // TODO: when is name determined? Currently by quote creator but can it be changed?
            newProject.Name = piName + "-" + project.Start.ToString("MMMMyyyy");

            await _dbContext.Projects.AddAsync(newProject);
            await _dbContext.SaveChangesAsync();

            return Ok(newProject);
        }
    }

    public class RequestApprovalModel
    {
        public Account[] Accounts { get; set; }
    }
}