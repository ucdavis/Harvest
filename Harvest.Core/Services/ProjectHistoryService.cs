using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;

namespace Harvest.Core.Services
{
    public interface IProjectHistoryService
    {
        Task<ProjectHistory> AddProjectHistory(Project project, string action, string description, User user = null);
    }

    public class ProjectHistoryService : IProjectHistoryService
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _dbContext;

        public ProjectHistoryService(IUserService userService, AppDbContext dbContext)
        {
            _userService = userService;
            _dbContext = dbContext;
        }

        public async Task<ProjectHistory> AddProjectHistory(Project project, string action, string description, User user = null)
        {
            user ??= await _userService.GetCurrentUser();

            var projectHistory = new ProjectHistory
            {
                Project = project,
                Action = action,
                Description = description,
                ActionDate = DateTime.UtcNow,
                Actor = user,
            };

            _dbContext.ProjectHistory.Add(projectHistory);

            return projectHistory;
        }
    }
}
