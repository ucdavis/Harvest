using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Harvest.Web.Controllers
{

    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class PermissionsController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;
        private readonly IUserService _userService;        

        public PermissionsController(AppDbContext dbContext, IIdentityService identityService, IUserService userService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if(team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }   

            IQueryable<Permission> permissionsQuery = _dbContext.Permissions
                .Include(a => a.User)
                .Include(a => a.Role)
                .Where(a => a.TeamId == null || a.TeamId == team.Id);
            
            //If you have System, show system.
            if (!await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                permissionsQuery = permissionsQuery.Where(a => a.Role.Name != Role.Codes.System && a.TeamId == team.Id);
            }
            
            var permissions = await permissionsQuery.ToListAsync();

            var viewModel = new UserPermissionsListModel();
            viewModel.Team = team;
            foreach (var permission in permissions)
            {
                if (viewModel.UserRoles.Any(a => a.User.Id == permission.User.Id))
                {
                    viewModel.UserRoles.Single(a => a.User.Id == permission.User.Id).Roles.Add(permission.Role);
                    if(permission.Role.Name == Role.Codes.Supervisor)
                    {
                        viewModel.UserRoles.Single(a => a.User.Id == permission.User.Id).SupervisorPermissionId = permission.Id;
                    }
                    if (permission.Role.Name == Role.Codes.Worker)
                    {
                        viewModel.UserRoles.Single(a => a.User.Id == permission.User.Id).WorkerPermissionId = permission.Id;
                    }
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
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Role> rolesQuery = _dbContext.Roles;

            //If you have System, show system.
            if (!await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                rolesQuery = rolesQuery.Where(a => a.Name != Role.Codes.System);
            }
            var viewModel = new AddUserRolesModel
            {
                Roles = await rolesQuery.ToListAsync(),
                TeamName = team.Name

            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddUserRolesModel model)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Role> rolesQuery = _dbContext.Roles;

            //If you have System, show system.
            if (!await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                rolesQuery = rolesQuery.Where(a => a.Name != Role.Codes.System);
            }
            var viewModel = new AddUserRolesModel
            {
                Roles = await rolesQuery.ToListAsync(),
                UserEmail = model.UserEmail,
                TeamName = team.Name
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

            //var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == model.UserEmail || u.Kerberos == model.UserEmail);
            User user = null;
            var users = await _dbContext.Users.Where(a => a.Email == model.UserEmail || a.Kerberos == model.UserEmail).ToArrayAsync();
            if(users.Length == 1)
            {
                user = users.First();
            }

            var role = await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == model.RoleId);
            if (!await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                if (role == null || role.Name == Role.Codes.System)
                {
                    ModelState.AddModelError("RoleId", "Role not found!");
                    return View(viewModel);
                }
            }
            else
            {
                if (role == null)
                {
                    ModelState.AddModelError("RoleId", "Role not found!");
                    return View(viewModel);
                }
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

            if(addUser && await _dbContext.Users.AnyAsync(a => a.Iam == user.Iam))
            {
                addUser = false;
                user = await _dbContext.Users.SingleAsync(a => a.Iam == user.Iam);
            }

            if (addUser)
            {
                await _dbContext.AddAsync(user);
            }

            var existingPermission = await _dbContext.Permissions.SingleOrDefaultAsync(a => a.UserId == user.Id && a.RoleId == role.Id && (a.TeamId == null || a.TeamId == team.Id));

            if (existingPermission != null)
            {
                ModelState.AddModelError(string.Empty, "User already in that role!");
                return View(viewModel);
            }

            if (role.Name == Role.Codes.FieldManager)
            {
                if (await _dbContext.Permissions.AnyAsync(a =>
                    a.UserId == user.Id && a.Role.Name == Role.Codes.Supervisor && a.TeamId == team.Id))
                {
                    ModelState.AddModelError(string.Empty, "Remove Supervisor Role before adding Field Manager role!");
                    return View(viewModel);
                }
            }

            if (role.Name == Role.Codes.Supervisor)
            {
                if (await _dbContext.Permissions.AnyAsync(a =>
                    a.UserId == user.Id && a.Role.Name == Role.Codes.FieldManager && a.TeamId == team.Id))
                {
                    ModelState.AddModelError(string.Empty, "User already has Field Manager role. Supervisor Role not needed.");
                    return View(viewModel);
                }
            }



            if (ModelState.IsValid)
            {
                var permission = new Permission();
                permission.User = user;
                permission.Role = role;
                if(role.Name != Role.Codes.System)
                {
                    permission.TeamId = team.Id;
                }
                await _dbContext.Permissions.AddAsync(permission);
                await _dbContext.SaveChangesAsync();

                Message = "User Permission added";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            if (await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                var viewModel = await _dbContext.Users.Where(a => a.Id == id).Include(a => a.Permissions.Where(w => w.TeamId == null || w.TeamId == team.Id))
                    .ThenInclude(a => a.Role).SingleAsync();
                return View(viewModel);
            }
            else
            {
                var viewModel = (await _dbContext.Users.Select(
                    a => new {
                        User = a,
                        Permissions = a.Permissions.Where(b => b.Role.Name != Role.Codes.System && b.TeamId == team.Id),
                        Roles = a.Permissions.Select(b => b.Role)
                    }).SingleAsync(a => a.User.Id == id)).User;
                return View(viewModel);
            }
                            
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int userId, int[] roles)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var user = await _dbContext.Users.Where(a => a.Id == userId).Include(a => a.Permissions).ThenInclude(a => a.Role).SingleAsync();
            if (!await _userService.HasAccess(AccessCodes.SystemAccess, TeamSlug))
            {
                if (await _dbContext.Roles.AnyAsync(a => a.Name == Role.Codes.System && roles.Contains(a.Id)))
                {
                    ErrorMessage = "Unknown Role selected";
                    return RedirectToAction("Index");
                }
            }

            if(roles.Length <= 0)
            {
                ErrorMessage = "No Roles Selected to remove.";
                return RedirectToAction("Delete", new { id = userId });
            }

            foreach(var role in roles)
            {
               _dbContext.Permissions.Remove(user.Permissions.Where(a => a.Role.Id == role && (a.TeamId == null || a.TeamId == team.Id)).Single());
            }
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var model = await _dbContext.Permissions.Where(a => a.Id == id).Include(a => a.User).Include(a => a.Role).Include(a => a.Parents).ThenInclude(a => a.User).Include(a => a.Children).ThenInclude(a => a.User).SingleOrDefaultAsync();
            //We don't really need the roles for parents and children, just the users because it is implied. Children are workers of the supervisor. Parents are supervisors of the worker.

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddWorkerToSupervisor(int id)
        {
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);
            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }
            var supervisorPermission = await _dbContext.Permissions
                .Where(a => a.Id == id && a.Role.Name == Role.Codes.Supervisor && (a.TeamId == null || a.TeamId == team.Id))
                .Include(a => a.User)
                //.Include(a => a.Children).ThenInclude(c => c.User)
                .SingleOrDefaultAsync();
            if (supervisorPermission == null)
            {
                ErrorMessage = "Supervisor not found.";
                return RedirectToAction("Index");
            }

            return View(supervisorPermission);
        }

        /// <summary>
        /// Id is permission id of the supervisor
        /// </summary>
        /// <param name="id">Supervisor's Permission</param>
        /// <param name="UserEmail">Email or Kerb of worker to add to supervisor's permissions</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddWorkerToSupervisor(int id, string UserEmail)
        {
            throw new NotImplementedException("Not implemented yet");
        }
    }
}
