using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Harvest.Web.Controllers
{

    public abstract class ApiController : Controller
    {

        public int? TeamId
        {
            get {
                var TeamId = User.Claims.FirstOrDefault(c => c.Type == "TeamId");
                if (TeamId == null || !int.TryParse(TeamId.Value, out var teamId))
                {
                    return null;
                }
                return teamId;
            }
        }

        public int? PermissionId
        {
            get
            {
                var permissionIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PermissionId");
                if (permissionIdClaim == null || !int.TryParse(permissionIdClaim.Value, out var permissionId))
                {
                    return null;
                }
                return permissionId;
            }
        }

        public string? TeamSlug
        {
            get
            {
                var teamSlugClaim = User.Claims.FirstOrDefault(c => c.Type == "TeamSlug");
                return teamSlugClaim?.Value;
            }
        }
    }
}
