using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Humanizer;

namespace Harvest.Web.Models
{
    public class UserPermissionsListModel
    {
        public Team Team { get; set; }
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        public User User { get; set; }        

        public IList<Role> Roles { get; set; }
        public int? SupervisorPermissionId { get; set; }
        public int? WorkerPermissionId { get; set; }

        public UserRole(Permission permission)
        {
            User = permission.User;
            Roles = new List<Role>();
            Roles.Add(permission.Role);
        }

        public string RolesList
        {
            get { return string.Join(", ", Roles.OrderBy(x => x.Name).Select(a => a.Name.Humanize(LetterCasing.Sentence).SafeHumanizeTitle()).ToArray()); }
        }
    }
}
