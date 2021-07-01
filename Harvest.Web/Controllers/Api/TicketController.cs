using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Migrations.SqlServer;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
    public class TicketController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public TicketController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
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
        [HttpGet]
        public ActionResult Create()
        {
            return View("React");
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Ticket ticket)
        {

            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId);
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
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                //.Include(a => a.Attachments)
                .Include(a => a.Messages)
                .ThenInclude(a => a.CreatedBy)
                .SingleAsync(a => a.Id == ticketId && a.ProjectId == projectId);
            return Ok(ticket);
        }

        [HttpGet]
        [Route("[controller]/[action]/{projectId}/{ticketId}")]
        public async Task<ActionResult> GetAttachments(int projectId, int ticketId)
        {
            var ticketAttachments = await _dbContext.TicketAttachments
                .Include(a => a.CreatedBy)
                .Where(a => a.TicketId == ticketId).ToArrayAsync();
            return Ok(ticketAttachments);
        }

        [HttpGet]
        [Route("[controller]/[action]/{projectId}/{ticketId}")]
        public async Task<ActionResult> GetMessages(int projectId, int ticketId)
        {
            var ticketMessages = await _dbContext.TicketMessages
                .Include(a => a.CreatedBy)
                .Where(a => a.TicketId == ticketId).ToArrayAsync();
            return Ok(ticketMessages);
        }
    }
}
