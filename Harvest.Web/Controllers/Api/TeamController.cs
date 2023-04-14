using System.Linq;
using Harvest.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Harvest.Core.Models;

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
        public async Task<TeamPicker[]> Index()
        {
            // return all teams
            var teams = await _dbContext.Teams.Include(a => a.TeamDetail).Select(t => new TeamPicker()
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.TeamDetail.Description,
                FieldManagers = string.Join(", ", t.Permissions.Where(a => a.Role.Name == Role.Codes.FieldManager).Take(3).Select(fm => fm.User.Name))
            }).ToArrayAsync();

            return teams;
        }
        
        [Route("api/team/{id}")]
        [ResponseCache(Duration = 300)]
        public async Task<Team> Get(int id)
        {
            return await _dbContext.Teams.SingleOrDefaultAsync(t => t.Id == id);
        }
    }
}
