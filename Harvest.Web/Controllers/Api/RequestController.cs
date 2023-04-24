using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Serilog;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class RequestController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;
        private readonly StorageSettings _storageSettings;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;
        private readonly IExpenseService _expenseService;

        private const int MinimumStaleDays = 18; 

        public RequestController(AppDbContext dbContext, 
            IUserService userService, 
            IOptions<StorageSettings> storageSettings, 
            IFileService fileService, 
            IProjectHistoryService historyService, 
            IEmailService emailService, 
            IExpenseService expenseService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _storageSettings = storageSettings.Value;
            _fileService = fileService;
            _emailService = emailService;
            _expenseService = expenseService;
            _historyService = historyService;
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Cancel(int projectId)
        {
            var statuses = new string[]
                {Project.Statuses.ChangeRequested, Project.Statuses.Requested, Project.Statuses.QuoteRejected};
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId && a.Team.Slug == TeamSlug && a.IsActive && statuses.Contains(a.Status));
            project.IsActive = false;
            project.UpdateStatus(Project.Statuses.Canceled);
            var quote = await _dbContext.Quotes.SingleOrDefaultAsync(a => a.ProjectId == projectId);
            if (quote != null)
            {
                quote.Status = Quote.Statuses.Canceled;
            }

            await _historyService.ProjectRequestCanceled(projectId, project);

            await _dbContext.SaveChangesAsync();

            return Ok(project);
        }

 
        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorandFinance)] 
        public async Task<ActionResult> Approve(int projectId, [FromBody] RequestApprovalModel model)
        {
            Project project;
            QuoteDetail quoteDetail;

            var currentUser = await _userService.GetCurrentUser();

            if (await _dbContext.Projects.AnyAsync(p => p.Id == projectId && p.OriginalProject != null))
            {
                // this was a change request that has been approved, so copy everything over to original and inActiveate change request
                var changeRequestProject = await _dbContext.Projects.SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);

                if (changeRequestProject.PrincipalInvestigator.Iam != currentUser.Iam)
                {
                    var staleDays = (int)((DateTime.UtcNow - changeRequestProject.LastStatusUpdatedOn).TotalDays);
                    if (staleDays <= MinimumStaleDays)
                    {
                        return BadRequest("You are not the principal investigator for this project and it isn't stale enough.");
                    }
                }

                // quote for change request
                var quote = await _dbContext.Quotes.SingleAsync(q => q.ProjectId == projectId);
                var originalQuote =
                    await _dbContext.Quotes.SingleAsync(a => a.ProjectId == changeRequestProject.OriginalProjectId);

                //Try to get acreage 
                var originalDetail = originalQuote.Text.DeserializeWithGeoJson<QuoteDetail>();
                var newQuoteDetail = quote.Text.DeserializeWithGeoJson<QuoteDetail>();
                var acreageDiff = (Decimal) (newQuoteDetail.Acres - originalDetail.Acres );


                originalQuote.Status = Quote.Statuses.Superseded;
                originalQuote.ProjectId = changeRequestProject.Id; //Assign the original quote to the change request project

                quote.Status = Quote.Statuses.Approved;
                quoteDetail = QuoteDetail.Deserialize(quote.Text);
                quote.ProjectId = changeRequestProject.OriginalProjectId.Value;
                quote.ApprovedById = currentUser.Id;
                quote.ApprovedOn = DateTime.UtcNow;

                project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(p => p.Id == changeRequestProject.OriginalProjectId);

                changeRequestProject.IsActive = false; // soft delete change request so we don't see it anymore
                changeRequestProject.UpdateStatus(Project.Statuses.ChangeApplied);
                changeRequestProject.Quote = originalQuote;

                // replace original project info with newly approved project info
                project.PrincipalInvestigatorId = changeRequestProject.PrincipalInvestigatorId;
                project.Crop = changeRequestProject.Crop;
                project.CropType = changeRequestProject.CropType;
                project.Start = changeRequestProject.Start;
                project.End = changeRequestProject.End;
                project.IsApproved = true;
                project.Requirements = changeRequestProject.Requirements;
                project.UpdateStatus(Project.Statuses.Active);
                project.Accounts = new List<Account>();
                project.Attachments = new List<ProjectAttachment>();
                project.Quote = quote;

                // clear old accounts
                _dbContext.Accounts.RemoveRange(_dbContext.Accounts.Where(a => a.ProjectId == project.Id));

                // change attachments to point to original project if they are not already part of that project
                var originalAttachments = await _dbContext.ProjectAttachments.Where(a => a.ProjectId == project.Id).ToListAsync();
                var changeRequestAttachments = await _dbContext.ProjectAttachments.Where(a => a.ProjectId == changeRequestProject.Id).ToListAsync();

                foreach (var attachment in changeRequestAttachments)
                {
                    // if this new attachment isn't one of the original attachments, then reassign to the original project
                    if (!originalAttachments.Any(a => a.Identifier == attachment.Identifier))
                    {
                        attachment.Project = project;
                    }
                }

                await _expenseService.CreateChangeRequestAdjustmentMaybe(project, newQuoteDetail, originalDetail);
            }
            else
            {
                // not a change request, so just get the project
                project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);

                
                if(project.PrincipalInvestigator.Iam != currentUser.Iam)
                {
                    var staleDays = (int)((DateTime.UtcNow - project.LastStatusUpdatedOn).TotalDays);
                    if(staleDays <= MinimumStaleDays)
                    {
                        return BadRequest("You are not the principal investigator for this project and it isn't stale enough.");
                    }
                }

                var quote = await _dbContext.Quotes.SingleAsync(q => q.ProjectId == projectId);
                quote.Status = Quote.Statuses.Approved;
                quote.ApprovedOn = DateTime.UtcNow;
                quote.ApprovedById = currentUser.Id;
                quoteDetail = QuoteDetail.Deserialize(quote.Text);
            }


            var percentage = 0.0m;

            foreach (var account in model.Accounts)
            {
                account.ProjectId = projectId;

                // Accounts will be auto-approved by quote approver
                account.ApprovedById = currentUser.Id;
                account.ApprovedOn = DateTime.UtcNow;
                percentage += account.Percentage;
                if (account.Percentage < 0)
                {
                    return BadRequest("Negative Percentage Detected");
                }
            }

            if (percentage != 100.0m)
            {
                return BadRequest("Percentage of accounts is not 100%");
            }

            project.Acres = quoteDetail.Acres;
            project.AcreageRateId = quoteDetail.AcreageRateId;
            //Ignore accounts that don't have a percentage
            project.Accounts = new List<Account>(model.Accounts.Where(a => a.Percentage > 0.0m).ToArray());
            project.UpdateStatus(Project.Statuses.Active);
            project.IsApproved = true;
            project.QuoteTotal = (decimal)quoteDetail.GrandTotal;
            project.Name = quoteDetail.ProjectName;

            foreach (var quoteField in quoteDetail.Fields)
            {
                // now that fields are locked in, add to new db fields
                // SQL Server requires polygons be counter clock-wise, so if it isn't then reverse it
                var poly = quoteField.Geometry;
                if (!poly.Shell.IsCCW)
                {
                    poly = (Polygon)poly.Reverse();
                }

                if (poly.IsEmpty || !poly.IsValid)
                {
                    Log.Error("Invalid polygon detected in quote");
                    continue;
                }


                var field = new Field { Crop = quoteField.Crop, Details = quoteField.Details, IsActive = true, ProjectId = project.Id, Location = poly };

                project.Fields.Add(field);
            }

            await _historyService.QuoteApproved(project.Id, model.Accounts);

            await _dbContext.SaveChangesAsync();

            await _emailService.QuoteApproved(project);

            return Ok(project);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorOnly)]
        public async Task<ActionResult> RejectQuote(int projectId, [FromBody] QuoteRejectionModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return BadRequest();
            }
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Team).SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            var quote = await _dbContext.Quotes.SingleAsync(a => a.ProjectId == projectId);

            var currentUser = await _userService.GetCurrentUser();

            var ticketToCreate = new Ticket();
            ticketToCreate.ProjectId = projectId;
            ticketToCreate.Name = "Quote rejected";
            ticketToCreate.Requirements = model.Reason;
            ticketToCreate.DueDate = null;
            ticketToCreate.CreatedById = currentUser.Id;
            ticketToCreate.CreatedOn = DateTime.UtcNow;

            await _dbContext.Tickets.AddAsync(ticketToCreate);
            await _historyService.TicketCreated(project.Id, ticketToCreate);

            project.UpdateStatus(Project.Statuses.QuoteRejected);
            quote.Status = Quote.Statuses.Rejected;

            await _historyService.QuoteRejected(project.Id, model.Reason);

            await _dbContext.SaveChangesAsync();

            await _emailService.QuoteDenied(project, model.Reason);

            return Ok(new { project, ticket = ticketToCreate });
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorandFinance)]
        public async Task<ActionResult> ChangeAccount(int projectId, [FromBody] RequestApprovalModel model)
        {
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            var currentUser = await _userService.GetCurrentUser();

            var percentage = 0.0m;
            foreach (var account in model.Accounts)
            {
                account.ProjectId = projectId;

                // Accounts will be auto-approved by quote approver
                account.ApprovedById = currentUser.Id;
                account.ApprovedOn = DateTime.UtcNow;
                percentage += account.Percentage;
                if (account.Percentage < 0)
                {
                    return BadRequest("Negative Percentage Detected");
                }
            }

            if (percentage != 100.0m)
            {
                return BadRequest("Percentage of accounts is not 100%");
            }
            //Ignore accounts that don't have a percentage
            project.Accounts = new List<Account>(model.Accounts.Where(a => a.Percentage > 0).ToArray());

            // TODO: Maybe inactivate instead?
            // remove any existing accounts that we no longer need
            _dbContext.RemoveRange(_dbContext.Accounts.Where(x => x.ProjectId == projectId));

            await _historyService.AccountChanged(project.Id, model.Accounts);

            await _dbContext.SaveChangesAsync();

            return Ok(project);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Files(int projectId, [FromBody] ProjectFilesModel model)
        {
            var currentUser = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects.Include(a => a.Attachments).Include(p => p.PrincipalInvestigator).Include(p => p.CreatedBy).SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            var projectAttachmentsToCreate = new List<ProjectAttachment>();

            foreach (var attachment in model.Attachments)
            {
                var newProject = new ProjectAttachment
                {
                    Identifier = attachment.Identifier,
                    FileName = attachment.FileName,
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = currentUser.Id,
                    SasLink = _fileService.GetDownloadUrl(_storageSettings.ContainerName, attachment.Identifier).AbsoluteUri,
                };

                projectAttachmentsToCreate.Add(newProject);
            }

            project.Attachments.AddRange(projectAttachmentsToCreate);

            _dbContext.Projects.Update(project);
            await _historyService.ProjectFilesAttached(project.Id, model.Attachments);
            await _dbContext.SaveChangesAsync();

            return Ok(projectAttachmentsToCreate);
        }

        //TODO: Test if this works with the team api...
        [HttpPost]
        [Route("/api/{controller}/{action}")]
        public async Task<ActionResult> Create([FromBody] Project project)
        {
            var currentUser = await _userService.GetCurrentUser();
            var changeRequest = false;

            var newProject = new Project
            {
                Crop = project.Crop,
                CropType = project.CropType,
                Start = project.Start,
                End = project.End,
                CreatedOn = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                IsActive = true,
                IsApproved = false,
                Requirements = project.Requirements,
                Status = Project.Statuses.Requested
            };
            
            // ensure team is set
            var team = await _dbContext.Teams.Where(t => t.Slug == project.Team.Slug).SingleAsync();
            newProject.TeamId = team.Id;

            if (project.Id > 0)
            {
                // this project already exists, so we are requesting a change
                //Make sure the project exists and the user is the PI
                if (!await _dbContext.Projects.AnyAsync(p => p.Id == project.Id && p.PrincipalInvestigator.Iam == currentUser.Iam))
                {
                    //OK, PI is not initiating this change, so we need to check if they are a member of the team in the Field Manager role.
                    if(!await _userService.HasAnyTeamRoles(team.Slug, new[] { Role.Codes.FieldManager }))
                    {
                        return BadRequest("Not Authorized");
                    }
                }

                changeRequest = true;
                newProject.UpdateStatus(Project.Statuses.ChangeRequested);
                newProject.OriginalProjectId = project.Id;
                newProject.Acres = project.Acres;

                // get the quote for this project and copy over so we have a good starting point
                var originalProjectQuote = await _dbContext.Quotes.SingleAsync(q => q.ProjectId == project.Id);

                var quoteCopy = new Quote
                {
                    Project = newProject,
                    InitiatedById = originalProjectQuote.InitiatedById,
                    CreatedDate = originalProjectQuote.CreatedDate,
                    Status = Quote.Statuses.Created,
                    Text = originalProjectQuote.Text,
                    Total = originalProjectQuote.Total,
                };

                await _dbContext.AddAsync(quoteCopy);
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

            // If there are attachments, fill out details and add to project
            foreach (var attachment in project.Attachments)
            {
                newProject.Attachments.Add(new ProjectAttachment
                {
                    Identifier = attachment.Identifier,
                    FileName = attachment.FileName,
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                });
            }

            // TODO: when is name determined? Currently by quote creator but can it be changed?
            newProject.Name = piName + "-" + project.Start.ToString("MMMMyyyy");

            await _dbContext.Projects.AddAsync(newProject);
            await _historyService.RequestCreated(newProject);
            await _dbContext.SaveChangesAsync();

            if (changeRequest)
            {
                await _emailService.ChangeRequest(newProject);
            }
            else
            {
                await _emailService.NewFieldRequest(newProject);
            }


            return Ok(newProject);
        }
    }

    public class RequestApprovalModel
    {
        public Account[] Accounts { get; set; }
    }

    public class ProjectFilesModel
    {
        public ProjectAttachment[] Attachments { get; set; }
    }

    public class QuoteRejectionModel
    {
        public string Reason { get; set; }
    }
}