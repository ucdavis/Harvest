using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Models
{
    public class EmulateUserViewModel
    {
        [Display(Name = "Search by Email")]
        public string UserEmail { get; set; }
    }
}
