using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _dbContext;

        public DbInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Initialize(bool recreateDb)
        {
            if (recreateDb)
            {
                //do what needs to be done?
            }

            //Make sure roles exist
            await CheckCreateRole(Role.Codes.System);
            await CheckCreateRole(Role.Codes.Admin);
            await CheckCreateRole(Role.Codes.FieldManager);
            await CheckCreateRole(Role.Codes.Supervisor);
            await CheckCreateRole(Role.Codes.Worker);

            await _dbContext.SaveChangesAsync();

            var systemRole = await _dbContext.Roles.SingleAsync(a => a.Name == Role.Codes.System);
            var user = new User
            {
                Email = "jsylvestre@ucdavis.edu",
                Kerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Iam = "1000009309"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "srkirkland@ucdavis.edu",
                Kerberos = "postit",
                FirstName = "Scott",
                LastName = "Kirkland",
                Iam = "1000029584"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "swebermilne@ucdavis.edu",
                Kerberos = "sweber",
                FirstName = "Spruce",
                LastName = "Weber-Milne",
                Iam = "1000255034"
            };
            await CheckOrCreatePermission(systemRole, user);
            await _dbContext.SaveChangesAsync();
            return;
        }

        private async Task CheckOrCreatePermission(Role systemRole, User user)
        {
            var userToCreate = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == user.Iam);
            if (userToCreate == null)
            {
                userToCreate = user;
                await _dbContext.Users.AddAsync(userToCreate);
            }

            var permission =
                await _dbContext.Permissions.SingleOrDefaultAsync(a =>
                    a.User.Id == userToCreate.Id && a.Role.Id == systemRole.Id);
            if (permission == null)
            {
                permission = new Permission {User = userToCreate, Role = systemRole};
                await _dbContext.Permissions.AddAsync(permission);
            }
        }

        private async Task CheckCreateRole(string role)
        {
            if (!await _dbContext.Roles.AnyAsync(a => a.Name == role))
            {
                var roleToCreate = new Role {Name = role};
                await _dbContext.Roles.AddAsync(roleToCreate);
            }
            
            return;
        }
    }
}
