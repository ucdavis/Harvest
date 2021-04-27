using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Harvest.Email.Models
{
    public class AccountPendingApprovalModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }

        public string ButtonUrl { get; set; }
        public List<AccountsForApprovalModel> AccountsList { get; set; }



    }
    public class AccountsForApprovalModel
    {
        public string Account { get; set; }
        public string Name { get; set; }
        public string Percent { get; set; }
    }
}
