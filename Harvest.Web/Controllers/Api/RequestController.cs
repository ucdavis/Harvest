using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            return Ok();
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

            // TODO: double check the percentages add up to 100%
            // TODO: add in fiscal officer info??
            foreach (var account in model.Accounts)
            {
                account.ProjectId = id;
                account.ApprovedById = null;
                account.ApprovedOn = null;
            }

            var quoteDetail = QuoteDetail.Deserialize(quote.Text);

            project.Acres = quoteDetail.Acres;
            project.AcreageRateId = quoteDetail.AcreageRateId;

            project.Accounts = new List<Account>(model.Accounts);
            project.Status = "PendingAccountApproval"; // TODO: update with enumerated values

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
                Start = project.Start,
                End = project.End,
                CreatedOn = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                IsActive = true,
                Requirements = project.Requirements,
                Status = "Requested" // TODO: remove once PR with defined steps is merged
            };

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