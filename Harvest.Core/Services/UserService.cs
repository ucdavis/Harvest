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
        Task<User> GetCurrentUser();
        Task<IEnumerable<string>> GetCurrentRoles();
        Task<bool> HasAccess(string accessCode);
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly AppDbContext _dbContext;
        private readonly RoleResolver _roleResolver;

        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, RoleResolver roleResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
            _roleResolver = roleResolver;
        }

        // Get the current user, creating them if necessary
        public async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No HttpContext found. Unable to retrieve or create User.");
                return null;
            }

            var username = _httpContextAccessor.HttpContext.User.Identity.Name;
            var userClaims = _httpContextAccessor.HttpContext.User.Claims.ToArray();
            string iamId = userClaims.Single(c => c.Type == IamIdClaimType).Value;

            return await _memoryCache.GetOrCreateAsync(iamId, async entry =>
            {
                // cache for sliding 20 minutes
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);

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
                        Kerberos = username
                    };

                    _dbContext.Users.Add(newUser);

                    await _dbContext.SaveChangesAsync();

                    return newUser;
                }
            });
        }

        public async Task<IEnumerable<string>> GetCurrentRoles()
        {
            var projectId = _httpContextAccessor.GetProjectId();

            var user = await GetCurrentUser();
            var userRoles = await _dbContext.Permissions
                .Where(p => p.UserId == user.Id)
                .Select(p => p.Role.Name)
                .ToArrayAsync();

            // if projectId is null, we just want to know if user is a PI of at least one project
            var isPrincipalInvestigator = await _dbContext.Projects.AnyAsync(p => (projectId == null || p.Id == projectId) && p.PrincipalInvestigatorId == user.Id);

            if (isPrincipalInvestigator)
            {
                return userRoles.Append(Role.Codes.PI);
            }

            return userRoles;
        }

        public async Task<bool> HasAccess(string accessCode)
        {
            var roles = _roleResolver(accessCode).Concat(_roleResolver(AccessCodes.SystemAccess));
            var userRoles = await GetCurrentRoles();
            return userRoles.Any(r => roles.Contains(r));
        }

        public const string IamIdClaimType = "ucdPersonIAMID";
    }

    public static class UserServiceExtensions
    {
        public static async Task<bool> HasAnyRoles(this IUserService userService, IEnumerable<string> roles)
        {
            var userRoles = await userService.GetCurrentRoles();
            return userRoles.Any(roles.Contains);
        }

        public static Task<bool> HasAnyRoles(this IUserService userService, string role, params string[] additionalRoles)
        {
            return HasAnyRoles(userService, additionalRoles.Append(role));
        }

        public static async Task<bool> HasOnlyRole(this IUserService userService, string role)
        {
            var userRoles = await userService.GetCurrentRoles();
            if (!userRoles.Contains(role))
            {
                return false;
            }
            return !userRoles.Any(r => r != role);
        }
    }
}