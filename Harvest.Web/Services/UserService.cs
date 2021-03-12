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
        private readonly IIdentityService _identityService;
        private readonly AppDbContext _dbContext;
        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IIdentityService identityService)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _dbContext = dbContext;
        }

        // Get the current user, creating them if necessary
        public async Task<User> GetCurrentUser()
        {
            var username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var dbUser = await _dbContext.Users.SingleOrDefaultAsync(a => a.Kerberos == username);

            if (dbUser != null)
            {
                return dbUser;
            }
            else
            {
                var newUser = await _identityService.GetByKerberos(username);
                
                _dbContext.Users.Add(newUser);

                await _dbContext.SaveChangesAsync();

                return newUser;
            }
        }
    }
}