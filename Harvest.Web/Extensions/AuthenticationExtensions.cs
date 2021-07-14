using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Harvest.Web.Extensions
{
    public static class AuthenticationExtensions
    {
        public static async Task<string> GetUserRoles(this HttpContext context)
        {
            var user = context.User.GetUserInfo();

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            var roles = await dbContext.Permissions.Where(p => p.User.Iam == user.Iam).Select(p => p.Role.Name).ToArrayAsync();

            return JsonSerializer.Serialize(roles);
        }

        public static string GetUserDetails(this HttpContext context) {
            var user = context.User.GetUserInfo();

            return JsonSerializer.Serialize(user, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}