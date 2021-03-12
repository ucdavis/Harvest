﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public PermissionsController(AppDbContext dbContext, IIdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddUserRole()
        {
            var viewModel = new AddUserRolesModel
            {
                Roles = await _dbContext.Roles.Where(a => a.Name != "System").ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole(AddUserRolesModel model)
        {
            var viewModel = new AddUserRolesModel
            {
                Roles = await _dbContext.Roles.Where(a => a.Name != "System").ToListAsync(),
                UserEmail = model.UserEmail
            };
            if (model.RoleId == 0)
            {
                ModelState.AddModelError("RoleId", "Must select valid Role");
                return View(viewModel);
            }

            if (string.IsNullOrWhiteSpace(model.UserEmail))
            {
                ModelState.AddModelError("UserEmail", "Must Enter User email");
                return View(viewModel);
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == model.UserEmail || u.Kerberos == model.UserEmail);
            var role = await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == model.RoleId);

            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Role not found!");
                return View(viewModel);
            }

            var addUser = false;
            if (user == null)
            {
                addUser = true;
                if (model.UserEmail.Contains("@"))
                {
                    user = await _identityService.GetByEmail(model.UserEmail);
                }
                else
                {
                    user = await _identityService.GetByKerberos(model.UserEmail);
                }
            }
            if (user == null)
            {
                ModelState.AddModelError("UserEmail", "User Not found.");
                return View(viewModel);
            }

            if (addUser)
            {
                await _dbContext.AddAsync(user);
            }

            var existingPermission = await _dbContext.Permissions.SingleOrDefaultAsync(a => a.UserId == user.Id && a.RoleId == role.Id);

            if (existingPermission != null)
            {
                ModelState.AddModelError(string.Empty, "User already in that role!");
                return View(viewModel);
            }



            if (ModelState.IsValid)
            {
                var permission = new Permission();
                permission.User = user;
                permission.Role = role;
                await _dbContext.Permissions.AddAsync(permission);
                await _dbContext.SaveChangesAsync();
                
                //TODO: Return a message
                //TODO: Redirect to index
            }

            return View(viewModel);
        }
    }
}
