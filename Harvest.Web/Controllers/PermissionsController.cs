using Microsoft.AspNetCore.Mvc;
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
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.AdminAccess)]
    public class PermissionsController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public PermissionsController(AppDbContext dbContext, IIdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }
        public async Task<IActionResult> Index()
        {
            //TODO: Filter out Role.Codes.System roles?
            var permissions = await _dbContext.Permissions
                .Include(a => a.User)
                .Include(a => a.Role)
                .Where(a=> a.Role.Name != Role.Codes.System)
                .ToListAsync();

            var viewModel = new UserPermissionsListModel();
            foreach (var permission in permissions)
            {
                if (viewModel.UserRoles.Any(a => a.User.Id == permission.User.Id))
                {
                    viewModel.UserRoles.Single(a => a.User.Id == permission.User.Id).Roles.Add(permission.Role);
                }
                else
                {
                    viewModel.UserRoles.Add(new UserRole(permission));
                }
            }
            viewModel.UserRoles = viewModel.UserRoles.OrderBy(u => u.User.LastName).ThenBy(u => u.User.FirstName).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new AddUserRolesModel
            {
                Roles = await _dbContext.Roles.Where(a => a.Name != Role.Codes.System).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddUserRolesModel model)
        {
            var viewModel = new AddUserRolesModel
            {
                Roles = await _dbContext.Roles.Where(a => a.Name != Role.Codes.System).ToListAsync(),
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
            var role = await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == model.RoleId && r.Name != Role.Codes.System);

            if (role == null || role.Name == Role.Codes.System)
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

                Message = "User Permission added";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public IActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
