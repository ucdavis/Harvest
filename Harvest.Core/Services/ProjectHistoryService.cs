using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;

namespace Harvest.Core.Services
{
    public interface IProjectHistoryService
    {
        Task<ProjectHistory> AddProjectHistory(Project project, string action, string description, object detailsToSerialize, User user = null);
    }

    public class ProjectHistoryService : IProjectHistoryService
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectHistoryService(IUserService userService, AppDbContext dbContext, JsonSerializerOptions jsonOptions)
        {
            _userService = userService;
            _dbContext = dbContext;
            _jsonOptions = jsonOptions;
        }

        public async Task<ProjectHistory> AddProjectHistory(Project project, string action, string description, object detailsToSerialize, User user = null)
        {
            user ??= await _userService.GetCurrentUser();

            var details = detailsToSerialize == null ? "" : JsonSerializer.Serialize(detailsToSerialize, _jsonOptions);

            var projectHistory = new ProjectHistory
            {
                Project = project,
                Action = action,
                Description = user != null ? $"{description} by {user.Name}" : description,
                Details = details,
                ActionDate = DateTime.UtcNow,
                Actor = user,
            };

            _dbContext.ProjectHistory.Add(projectHistory);

            return projectHistory;
        }
    }
}
