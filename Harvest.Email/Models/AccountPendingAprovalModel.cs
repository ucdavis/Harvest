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
        public string MjmlListOnly { get; private set; }

        /// <summary>
        /// Sets model values when running TestController.TestBody
        /// </summary>
        public void InitForMjml()
        {
            PI                  = "@Model.PI";
            ProjectName         = "@Model.ProjectName";
            ProjectStart        = "@Model.ProjectStart";
            ProjectEnd          = "@Model.ProjectEnd";
            ButtonUrl           = "@Model.ButtonUrl";
            var sb = new StringBuilder();
            sb.AppendLine("@foreach (var item in Model.AccountsList){");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td style=\"padding: 0 15px 0 0;\">@item.Account</td>");
            sb.AppendLine("<td style=\"padding: 0 15px;\">@item.Name</td>");
            sb.AppendLine("<td style=\"padding: 0 0 0 15px;\">@item.Percent</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("}");
            MjmlListOnly = sb.ToString();
        }

    }
    public class AccountsForApprovalModel
    {
        public string Account { get; set; }
        public string Name { get; set; }
        public string Percent { get; set; }
    }
}
