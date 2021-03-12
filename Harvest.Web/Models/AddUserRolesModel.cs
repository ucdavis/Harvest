using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Web.Models
{
    public class AddUserRolesModel
    {
        [Display(Name = "Role to Add")]
        public int RoleId { get; set; }

        [Display(Name = "User Email")]
        [Required]
        public string UserEmail { get; set; }

        public List<Role> Roles { get; set; }
    }
}
