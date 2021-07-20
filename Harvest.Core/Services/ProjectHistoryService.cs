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

        /// <summary>
        /// Creates a <see cref="ProjectHistory"/> record for a given <paramref name="projectId"/>
        /// </summary>
        /// <remarks>If <paramref name="detailsToSerialize"/> contains domain entities, consider calling this before those entities get attached
        /// to a <see cref="AppDbContext"/> to avoid serializing more of the object graph.</remarks>
        Task<ProjectHistory> AddProjectHistory(int projectId, string action, string description, object detailsToSerialize, User user = null);
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

        /// <summary>
        /// Creates a <see cref="ProjectHistory"/> record for a given <paramref name="projectId"/>
        /// </summary>
        /// <remarks>If <paramref name="detailsToSerialize"/> contains domain entities, consider calling this before those entities get attached
        /// to a <see cref="AppDbContext"/> to avoid serializing more of the object graph.</remarks>
        public async Task<ProjectHistory> AddProjectHistory(int projectId, string action, string description, object detailsToSerialize, User user = null)
        {
            user ??= await _userService.GetCurrentUser();

            var details = detailsToSerialize == null ? "" : JsonSerializer.Serialize(detailsToSerialize, _jsonOptions);

            var projectHistory = new ProjectHistory
            {
                ProjectId = projectId,
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
