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
            await CheckCreateRole("System");
            await CheckCreateRole("Admin");
            await CheckCreateRole("Supervisor");
            await CheckCreateRole("Worker");

            return;
        }

        private async Task CheckCreateRole(string role)
        {
            if (!await _dbContext.Roles.AnyAsync(a => a.Name == role))
            {
                var roleToCreate = new Role {Name = role};
                await _dbContext.Roles.AddAsync(roleToCreate);
                await _dbContext.SaveChangesAsync();
            }
            
            return;
        }
    }
}
