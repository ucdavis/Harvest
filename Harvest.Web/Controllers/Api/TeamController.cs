using Harvest.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    public class TeamController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public TeamController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Route("api/team")]
        [ResponseCache(Duration = 300)]
        public async Task<Team[]> Index()
        {
            // return all teams
            return await _dbContext.Teams.ToArrayAsync();
        }
    }
}
