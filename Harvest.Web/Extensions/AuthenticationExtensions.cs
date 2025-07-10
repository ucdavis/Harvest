using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Harvest.Web.Extensions
{
    public static class AuthenticationExtensions
    {

        public static async Task<string> GetUserRoles(this HttpContext context)
        {
            string teamSlug = null;
            try
            {
                teamSlug = context.Request.Path.Value.Split('/')[1];
            }
            catch (System.Exception)
            {
                //Swallow it
            }

            var user = context.User.GetUserInfo();

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            IEnumerable<string> roles;
            if (!string.IsNullOrWhiteSpace(teamSlug))
            {
                roles = await dbContext.Permissions.Where(p => p.User.Iam == user.Iam && (p.Team == null || p.Team.Slug == teamSlug)).Select(p => p.Role.Name).ToArrayAsync();
            }
            else
            {
                //When there is no team, we want to get all roles so we can show them the team picker. (PI's never need to pick teams until they create a project)
                roles = await dbContext.Permissions.Where(p => p.User.Iam == user.Iam).Select(p => p.Role.Name).ToArrayAsync();
            }
            if (await dbContext.Projects.AnyAsync(p => p.PrincipalInvestigator.Iam == user.Iam) || await dbContext.ProjectPermissions.AnyAsync(a => a.User.Iam == user.Iam))
            {
                roles = roles.Append(Role.Codes.PI);
            }

            return JsonSerializer.Serialize(roles);
        }

        public static string GetUserDetails(this HttpContext context)
        {
            var user = context.User.GetUserInfo();

            return JsonSerializer.Serialize(user, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}