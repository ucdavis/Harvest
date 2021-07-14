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
            await CheckCreateRole(Role.Codes.FieldManager);
            await CheckCreateRole(Role.Codes.Supervisor);
            await CheckCreateRole(Role.Codes.Worker);
            await CheckCreateRole(Role.Codes.PI);

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
            user = new User
            {
                Email = "cydoval@ucdavis.edu",
                Kerberos = "cydoval",
                FirstName = "Calvin",
                LastName = "Doval",
                Iam = "1000050298"
            };
            await CheckOrCreatePermission(systemRole, user);
            user = new User
            {
                Email = "bmwong@ucdavis.edu",
                Kerberos = "wongband",
                FirstName = "Bryan",
                LastName = "Wong",
                Iam = "1000274724"
            };
            await CheckOrCreatePermission(systemRole, user);
            await _dbContext.SaveChangesAsync();

            await CheckCreateSampleRates();
            await _dbContext.SaveChangesAsync();
            return;
        }

        private async Task CheckCreateSampleRates()
        {

            if (await _dbContext.Rates.AnyAsync())
            {
                return;
            }

            var createdBy = await _dbContext.Users.FirstAsync();

            var rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Russell Ranch Acreage";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES";
            rate.Price = 1150.00m;
            rate.Unit = "Per Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Acreage;
            rate.Description = "Plant Sciences Row Crop Acreage";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFLV";
            rate.Price = 1150.00m;
            rate.Unit = "Per Acre per Year";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "RR Skilled Labor";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRACRES";
            rate.Price = 60.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Labor;
            rate.Description = "PLS Mechanic";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS";
            rate.Price = 72.00m;
            rate.Unit = "Hourly";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Crop Destruction";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "";
            rate.Price = 66.67m;
            rate.Unit = "Per acre";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Other;
            rate.Description = "Century Project Soil Sample";
            rate.BillingUnit = "Russell Ranch";
            rate.Account = "3-RRCNTRY";
            rate.Price = 70.00m;
            rate.Unit = "Per sample";

            await _dbContext.Rates.AddAsync(rate);

            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "CAES Trencher (large)";
            rate.BillingUnit = "CA&ES";
            rate.Account = "3-FRMRATE";
            rate.Price = 685.00m;
            rate.Unit = "Daily";

            await _dbContext.Rates.AddAsync(rate);
            
            rate = DefaultRate(createdBy);
            rate.Type = Rate.Types.Equipment;
            rate.Description = "PLS Row Planter";
            rate.BillingUnit = "Plant Sciences";
            rate.Account = "3-APSNFDS";
            rate.Price = 34.91m;
            rate.Unit = "Per Acre";

            await _dbContext.Rates.AddAsync(rate);
        }

        private static Rate DefaultRate(User createdBy)
        {
            var rate = new Rate {CreatedById = createdBy.Id, UpdatedById = createdBy.Id, CreatedOn = DateTime.UtcNow};
            rate.UpdatedOn = rate.CreatedOn;
            return rate;
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
