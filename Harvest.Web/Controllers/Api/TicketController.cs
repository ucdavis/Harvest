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

namespace Harvest.Web.Controllers.Api
{
    public class TicketController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public TicketController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }


        // create a new request via react
        [HttpGet]
        public ActionResult Create()
        {
            return View("React");
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Ticket ticket)
        {
            if (projectId != ticket.ProjectId)
            {
                return BadRequest("Wrong Project Id");
            }

            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId);
            var currentUser = await _userService.GetCurrentUser();

            //TODO: Any authentication? Add in attachments?

            var ticketToCreate = new Ticket();
            ticketToCreate.ProjectId = projectId;
            ticketToCreate.Name = ticket.Name;
            ticketToCreate.Requirements = ticket.Requirements;
            ticketToCreate.DueDate = ticket.DueDate;
            ticketToCreate.CreatedById = currentUser.Id;

            await _dbContext.Tickets.AddAsync(ticketToCreate);
            await _dbContext.SaveChangesAsync();

            return Ok(project);
        }
    }
}
