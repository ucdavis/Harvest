using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Core.Models.Settings;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly StorageSettings storageSettings;
        private readonly IFileService fileService;
        private readonly IProjectHistoryService _historyService;

        public TicketController(AppDbContext dbContext, IUserService userService, IEmailService emailService,
            IOptions<StorageSettings> storageSettings, IFileService fileService, IProjectHistoryService historyService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
            _emailService = emailService;
            this.storageSettings = storageSettings.Value;
            this.fileService = fileService;
            _historyService = historyService;
        }

        [HttpGet]
        [Authorize(policy: AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> GetList(int projectId, int? maxRows)
        {
            var ticketsQuery = _dbContext.Tickets.Where(a => a.ProjectId == projectId).OrderByDescending(a => a.UpdatedOn);
            if (maxRows.HasValue)
            {
                //I actually don't like this take 5, too easy to loose that there maybe uncompleted tickets not showing here. but worry about it later.
                //Maybe return a model that has counts and other info.
                ticketsQuery = (IOrderedQueryable<Ticket>)ticketsQuery.Take(maxRows.Value);
            }
            return Ok(await ticketsQuery.ToArrayAsync());
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public ActionResult List(int id)
        {
            return View("React");
        }

        // create a new ticket via react
        [HttpGet]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public ActionResult Create()
        {
            return View("React");
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public ActionResult NeedsAttention()
        {
            return View("React");
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public ActionResult Mine()
        {
            return View("React");
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> RequiringPIAttention(int? limit)
        {
            var user = await _userService.GetCurrentUser();

            // Get list of top N open tickets in PI projects
            var openTickets = _dbContext.Tickets
                .Where(a => a.Status != Ticket.Statuses.Complete && a.Project.IsActive && a.Project.PrincipalInvestigatorId == user.Id);

            if (limit.HasValue) { openTickets = openTickets.Take(limit.Value); }

            return Ok(await openTickets.OrderByDescending(a => a.UpdatedOn).ToArrayAsync());
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> RequiringManagerAttention(int? limit)
        {
            // Get list of top N open tickets in all projects
            var openTickets = _dbContext.Tickets
                .Where(a => a.Status != Ticket.Statuses.Complete && a.Project.IsActive);

            if (limit.HasValue) { openTickets = openTickets.Take(limit.Value); }

            return Ok(await openTickets.OrderByDescending(a => a.UpdatedOn).ToArrayAsync());
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> UpdateWorkNotes(int projectId, int ticketId, [FromBody] string workNotes)
        {
            var ticketToUpdate = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);
            var oldWorkNotes = ticketToUpdate.WorkNotes;
            var currentUser = await _userService.GetCurrentUser();
            ticketToUpdate.WorkNotes = workNotes;
            ticketToUpdate.UpdatedBy = currentUser;
            ticketToUpdate.UpdatedOn = DateTime.UtcNow;

            _dbContext.Tickets.Update(ticketToUpdate);
            await _historyService.TicketNotesUpdated(projectId, ticketToUpdate);
            await _dbContext.SaveChangesAsync();

            return Ok(ticketToUpdate);

        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Ticket ticket)
        {

            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == projectId);
            var currentUser = await _userService.GetCurrentUser();

            //TODO: Any authentication? 

            var ticketToCreate = new Ticket();
            ticketToCreate.ProjectId = projectId;
            ticketToCreate.Name = ticket.Name;
            ticketToCreate.Requirements = ticket.Requirements;
            ticketToCreate.DueDate = ticket.DueDate;
            ticketToCreate.CreatedById = currentUser.Id;
            ticketToCreate.CreatedOn = DateTime.UtcNow;

            // If there are attachments, fill out details and add to project
            foreach (var attachment in ticket.Attachments)
            {
                ticketToCreate.Attachments.Add(new TicketAttachment()
                {
                    Identifier = attachment.Identifier,
                    FileName = attachment.FileName,
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = currentUser.Id
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Something is wrong");
            }

            await _dbContext.Tickets.AddAsync(ticketToCreate);
            await _historyService.TicketCreated(project.Id, ticketToCreate);
            await _dbContext.SaveChangesAsync();

            await _emailService.NewTicketCreated(project, ticketToCreate);

            return Ok(project);
        }
        [HttpGet("[controller]/[action]/{projectId}/{ticketId}")]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public ActionResult Details(int projectId, int ticketId)
        {
            return View("React");
        }

        [HttpGet("[controller]/[action]/{projectId}/{ticketId}")]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Get(int projectId, int ticketId)
        {
            var ticket = await _dbContext.Tickets
                .Where(a => a.Id == ticketId && a.ProjectId == projectId)
                .Select(a => new
                    Ticket
                {
                    Id = a.Id,
                    Name = a.Name,
                    CreatedBy = a.CreatedBy,
                    CreatedOn = a.CreatedOn,
                    UpdatedBy = a.UpdatedBy,
                    Requirements = a.Requirements,
                    WorkNotes = a.WorkNotes,
                    UpdatedOn = a.UpdatedOn,
                    DueDate = a.DueDate,
                    Status = a.Status,
                    Completed = a.Completed,
                    Messages = a.Messages.Select(b => new TicketMessage { Id = b.Id, CreatedBy = b.CreatedBy, CreatedOn = b.CreatedOn, Message = b.Message }).ToList(),
                    Attachments = a.Attachments.Select(b => new TicketAttachment { Id = b.Id, CreatedBy = b.CreatedBy, CreatedOn = b.CreatedOn, FileName = b.FileName, Identifier = b.Identifier, SasLink = fileService.GetDownloadUrl(storageSettings.ContainerName, b.Identifier).AbsoluteUri }).ToList(),
                })
                .SingleAsync();

            return Ok(ticket);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Reply(int projectId, int ticketId, [FromBody] TicketMessage ticketMessage)
        {
            if (ticketMessage == null || string.IsNullOrWhiteSpace(ticketMessage.Message))
            {
                return BadRequest("No message entered");
            }

            var currentUser = await _userService.GetCurrentUser();
            var ticket = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId && !a.Completed);

            var ticketMessageToCreate = new TicketMessage
            {
                Message = ticketMessage.Message,
                CreatedBy = currentUser,
                CreatedOn = DateTime.UtcNow,
                TicketId = ticket.Id
            };

            ticket.Messages.Add(ticketMessageToCreate);
            ticket.UpdatedBy = currentUser;
            ticket.UpdatedOn = DateTime.UtcNow;
            ticket.Status = Ticket.Statuses.Updated;

            _dbContext.Tickets.Update(ticket);
            await _historyService.TicketReplyCreated(ticket.ProjectId, ticketMessageToCreate);
            await _dbContext.SaveChangesAsync();
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Accounts)
                .SingleAsync(a => a.Id == projectId);

            await _emailService.TicketReplyAdded(project, ticket, ticketMessageToCreate);

            var savedTm = await _dbContext.TicketMessages.Where(a => a.Id == ticketMessageToCreate.Id).Select(a => new TicketMessage { Id = a.Id, CreatedBy = a.CreatedBy, Message = a.Message }).SingleAsync();

            //Return message instead?
            return Ok(savedTm);

        }

        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        [HttpPost("[controller]/[action]/{projectId}/{ticketId}")]
        public async Task<ActionResult> UploadFiles(int projectId, int ticketId, [FromBody] TicketFilesModel model)
        {
            var currentUser = await _userService.GetCurrentUser();
            var ticket = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId && !a.Completed);

            var ticketAttachmentsToCreate = new List<TicketAttachment>();
            foreach (var attachment in model.Attachments)
            {
                var ticketAttachmentToCreate = new TicketAttachment()
                {
                    Identifier = attachment.Identifier,
                    FileName = attachment.FileName,
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType,
                    CreatedBy = currentUser,
                    CreatedOn = DateTime.UtcNow,
                };
                ticketAttachmentsToCreate.Add(ticketAttachmentToCreate);
            }

            ticket.Attachments.AddRange(ticketAttachmentsToCreate);

            ticket.UpdatedBy = currentUser;
            ticket.UpdatedOn = DateTime.UtcNow;
            ticket.Status = Ticket.Statuses.Updated;

            _dbContext.Tickets.Update(ticket);
            await _historyService.TicketFilesAttached(projectId, ticketAttachmentsToCreate);
            await _dbContext.SaveChangesAsync();

            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator)
                .SingleAsync(a => a.Id == projectId);

            var addedIds = ticketAttachmentsToCreate.Select(a => a.Id).ToArray();
            //TODO return other file info that may be needed
            var savedTa = await _dbContext.TicketAttachments.Where(a => addedIds.Contains(a.Id))
                .Select(a => new TicketAttachment() { Id = a.Id, CreatedBy = a.CreatedBy, FileName = a.FileName, CreatedOn = a.CreatedOn, Identifier = a.Identifier, SasLink = fileService.GetDownloadUrl(storageSettings.ContainerName, a.Identifier).AbsoluteUri, })
                .ToListAsync();

            await _emailService.TicketAttachmentAdded(project, ticket, savedTa.ToArray());

            //Return message instead?
            return Ok(savedTa);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Close(int projectId, int ticketId)
        {
            var currentUser = await _userService.GetCurrentUser();
            var ticket = await _dbContext.Tickets.Include(a => a.Project).ThenInclude(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);

            ticket.Status = Ticket.Statuses.Complete;
            ticket.UpdatedBy = currentUser;
            ticket.UpdatedOn = DateTime.UtcNow;
            ticket.Completed = true;


            _dbContext.Tickets.Update(ticket);
            await _dbContext.SaveChangesAsync();

            await _emailService.TicketClosed(ticket.Project, ticket, currentUser);

            return Ok();
        }

    }
    public class TicketFilesModel
    {
        public TicketAttachment[] Attachments { get; set; }
    }
}
