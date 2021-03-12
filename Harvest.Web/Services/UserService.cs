using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Services
{
    public interface IUserService
    {
        Task<User> GetCurrentUser();
    }

    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        // Get the current user, creating them if necessary
        public async Task<User> GetCurrentUser()
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;
            var userClaims = _httpContextAccessor.HttpContext.User.Claims.ToArray();
            string iamId = userClaims.Single(c => c.Type == "ucdPersonIAMID").Value;

            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == iamId);

            if (dbUser != null)
            {
                return dbUser;
            }
            else
            {
                var newUser = new User {
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
        }
    }
}