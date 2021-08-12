using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Email.Models
{
    public class ExpiringProjectsModel
    {
        public string Name { get; set; }
        public string EndDate { get; set; }
        public string ProjectUrl { get; set; }
    }

}
