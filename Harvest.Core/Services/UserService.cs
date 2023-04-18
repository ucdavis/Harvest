using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Harvest.Core.Services
{
    public delegate string[] RoleResolver(string accessCode);

    public interface IUserService
    {
        Task<User> GetUser(Claim[] userClaims);
        Task<User> GetCurrentUser();
        Task<IEnumerable<TeamRoles>> GetCurrentRoles();
        Task<bool> HasAccess(string accessCode);
        Task<bool> HasAccess(string[] accessCodes);
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        private readonly RoleResolver _roleResolver;

        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, RoleResolver roleResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _roleResolver = roleResolver;
        }

        // Get any user based on their claims, creating if necessary
        public async Task<User> GetUser(Claim[] userClaims)
        {
            string iamId = userClaims.Single(c => c.Type == IamIdClaimType).Value;

            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == iamId);

            if (dbUser != null)
            {
                return dbUser; // already in the db, just return straight away
            }
            else
            {
                // not in the db yet, create new user and return
                var newUser = new User
                {
                    FirstName = userClaims.Single(c => c.Type == ClaimTypes.GivenName).Value,
                    LastName = userClaims.Single(c => c.Type == ClaimTypes.Surname).Value,
                    Email = userClaims.Single(c => c.Type == ClaimTypes.Email).Value,
                    Iam = iamId,
                    Kerberos = userClaims.Single(c=>c.Type == ClaimTypes.NameIdentifier).Value
                };

                _dbContext.Users.Add(newUser);

                await _dbContext.SaveChangesAsync();

                return newUser;
            }
        }

        // Get the current user, creating if necessary
        public async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No HttpContext found. Unable to retrieve or create User.");
                return null;
            }

            var userClaims = _httpContextAccessor.HttpContext.User.Claims.ToArray();

            return await GetUser(userClaims);
        }

        public async Task<IEnumerable<TeamRoles>> GetCurrentRoles()
        {
            var projectId = _httpContextAccessor.GetProjectId();
            string iamId = _httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == IamIdClaimType).Value;

            var userRoles = await _dbContext.Permissions
                .Where(p => p.User.Iam == iamId)
                .Select(p => new TeamRoles(p.Role.Name, p.Team.Slug))
                .ToArrayAsync();

            // if projectId is null, we just want to know if user is a PI of at least one project
            var isPrincipalInvestigator = await _dbContext.Projects.AnyAsync(p => (projectId == null || p.Id == projectId) && p.PrincipalInvestigator.Iam == iamId);

            if (isPrincipalInvestigator)
            {
                return userRoles.Append(new TeamRoles(Role.Codes.PI, null));
            }

            return userRoles;
        }

        public async Task<bool> HasAccess(string accessCode)
        {
            var roles = _roleResolver(accessCode).Concat(_roleResolver(AccessCodes.SystemAccess));
            var userRoles = await GetCurrentRoles();
            return userRoles.Any(r => roles.Contains(r.Role));
        }

        public async Task<bool> HasAccess(string[] accessCodes)
        {
            IEnumerable<string> roles = _roleResolver(AccessCodes.SystemAccess);
            foreach (var accessCode in accessCodes)
            {
                roles = roles.Concat(_roleResolver(accessCode));
            }
            var userRoles = await GetCurrentRoles();
            return userRoles.Any(r => roles.Contains(r.Role));
        }

        public const string IamIdClaimType = "ucdPersonIAMID";
    }

    public class TeamRoles
    {
        public TeamRoles(string role, string teamSlug)
        {
            Role = role;
            TeamSlug = teamSlug;
        }
        
        public string Role { get; set; }
        public string TeamSlug { get; set; }
    }

    public static class UserServiceExtensions
    {
        [Obsolete("Use HasAnyTeamRoles instead")]
        public static async Task<bool> HasAnyRoles(this IUserService userService, IEnumerable<string> roles)
        {
            var userRoles = await userService.GetCurrentRoles();
            return userRoles.Any(r => roles.Contains(r.Role));
        }
        public static async Task<bool> HasAnyTeamRoles(this IUserService userService, string teamSlug, IEnumerable<string> roles)
        {
            var userRoles = await userService.GetCurrentRoles();

            return userRoles.Where(a => a.Role == Role.Codes.System || a.TeamSlug == teamSlug).Any(r => roles.Contains(r.Role));
        }
    }
}