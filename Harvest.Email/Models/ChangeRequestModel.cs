using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Harvest.Email.Models
{
    public class ChangeRequestModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }
        public string CropType { get; set; }
        public string Crops { get; set; }
        public string Requirements { get; set; }
        public string ButtonUrlForQuote { get; set; }
        public string ButtonUrlForProject { get; set; }
        
    }
}
