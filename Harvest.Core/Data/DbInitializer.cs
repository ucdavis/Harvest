using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Data
{
    public class DbInitializer
    {
        private readonly AppDbContext _dbContext;

        public DbInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}
