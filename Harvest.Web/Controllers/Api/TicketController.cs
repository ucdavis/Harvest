using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    public class TicketController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public TicketController(AppDbContext dbContext, IUserService userService, IEmailService emailService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
            _emailService = emailService;
        }


        [HttpGet]
        public async Task<ActionResult> GetList(int projectId, int? maxRows)
        {
            var ticketsQuery = _dbContext.Tickets.Where(a => a.ProjectId == projectId).OrderByDescending(a => a.UpdatedOn);
            if (maxRows.HasValue)
            {
                //I actually don't like this take 5, too easy to loose that there maybe uncompleted tickets not showing here. but worry about it later.
                //Maybe return a model that has counts and other info.
                ticketsQuery = (IOrderedQueryable<Ticket>) ticketsQuery.Take(maxRows.Value);
            }
            return Ok(await ticketsQuery.ToArrayAsync());
        }

        [HttpGet]
        public ActionResult List(int id)
        {
            return View("React");
        }

        // create a new ticket via react
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        [HttpGet]
        public ActionResult Create()
        {
            return View("React");
        }

        [HttpPost]
        [Authorize(Policy= AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> UpdateWorkNotes(int projectId, int ticketId, [FromBody] string workNotes)
        {
            var ticketToUpdate = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);
            var currentUser = await _userService.GetCurrentUser();
            ticketToUpdate.WorkNotes = workNotes;
            ticketToUpdate.UpdatedBy = currentUser;
            ticketToUpdate.UpdatedOn = DateTime.UtcNow;

            _dbContext.Tickets.Update(ticketToUpdate);
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
            await _dbContext.SaveChangesAsync();

            await _emailService.NewTicketCreated(project, ticketToCreate);

            return Ok(project);
        }
        [Route("[controller]/[action]/{projectId}/{ticketId}")]
        public ActionResult Details(int projectId, int ticketId)
        {
            return View("React");
        }
        [HttpGet]
        [Route("[controller]/[action]/{projectId}/{ticketId}")]
        public async Task<ActionResult> Get(int projectId, int ticketId)
        {
            var ticket = await _dbContext.Tickets
                .Where(a => a.Id == ticketId && a.ProjectId == projectId)
                .Select(a => new
                    Ticket
                    { Id= a.Id,
                        Name = a.Name, CreatedBy = a.CreatedBy, CreatedOn = a.CreatedOn, UpdatedBy = a.UpdatedBy,
                        Requirements = a.Requirements, WorkNotes = a.WorkNotes,
                        UpdatedOn = a.UpdatedOn, DueDate = a.DueDate, Status = a.Status, Messages = a.Messages,
                        Attachments = a.Attachments
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
            var ticket = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);

            var ticketMessageToCreate = new TicketMessage
            {
                Message = ticketMessage.Message, 
                CreatedBy = currentUser, 
                CreatedOn = DateTime.UtcNow
            };
            ticket.Messages.Add(ticketMessageToCreate);
            ticket.UpdatedBy = currentUser;
            ticket.UpdatedOn = DateTime.UtcNow;
            ticket.Status = "Updated";

            _dbContext.Tickets.Update(ticket);
            await _dbContext.SaveChangesAsync();
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator)
                .SingleAsync(a => a.Id == projectId);

            await _emailService.TicketReplyAdded(project, ticket, ticketMessageToCreate);

            var savedTm = await _dbContext.TicketMessages.Where(a => a.Id == ticketMessageToCreate.Id).Select(a => new TicketMessage{Id = a.Id, CreatedBy = a.CreatedBy, Message = a.Message}).SingleAsync();

            //Return message instead?
            return Ok(savedTm);

        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        [Route("[controller]/[action]/{projectId}/{ticketId}")]
        public async Task<ActionResult> UploadFiles(int projectId, int ticketId, [FromBody] TicketFilesModel model)
        {
            var currentUser = await _userService.GetCurrentUser();
            var ticket = await _dbContext.Tickets.SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);

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
                    CreatedOn = DateTime.UtcNow
                };
                ticketAttachmentsToCreate.Add(ticketAttachmentToCreate);
            }

            ticket.Attachments.AddRange(ticketAttachmentsToCreate);

            ticket.UpdatedBy = currentUser;
            ticket.UpdatedOn = DateTime.UtcNow;
            ticket.Status = "Updated";

            _dbContext.Tickets.Update(ticket);
            await _dbContext.SaveChangesAsync();

            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator)
                .SingleAsync(a => a.Id == projectId);

            var addedIds = ticketAttachmentsToCreate.Select(a => a.Id).ToArray();
            //TODO return other file info that may be needed
            var savedTa = await _dbContext.TicketAttachments.Where(a => addedIds.Contains(a.Id))
                .Select(a => new TicketAttachment() {Id = a.Id, CreatedBy = a.CreatedBy, FileName = a.FileName, CreatedOn = a.CreatedOn})
                .ToListAsync();

            await _emailService.TicketAttachmentAdded(project, ticket, savedTa.ToArray());

            //Return message instead?
            return Ok(savedTa);
        }

    }
    public class TicketFilesModel
    {
        public TicketAttachment[] Attachments { get; set; }
    }
}
