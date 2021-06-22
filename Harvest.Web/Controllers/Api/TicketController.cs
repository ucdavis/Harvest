using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Services;
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
    }
}
