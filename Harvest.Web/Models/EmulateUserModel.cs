using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Models
{
    public class EmulateUserViewModel
    {
        [Display(Name = "Search by Email or Kerb")]
        public string Search { get; set; }
    }
}
