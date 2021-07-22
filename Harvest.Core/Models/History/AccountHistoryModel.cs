using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class AccountHistoryModel
    {
        public AccountHistoryModel() { }

        public AccountHistoryModel(Account account)
        {
            Id = account.Id;
            ProjectId = account.ProjectId;
            Number = account.Number;
            Name = account.Name;
            AccountManagerName = account.AccountManagerName;
            AccountManagerEmail = account.AccountManagerEmail;
            Percentage = account.Percentage;
            ApprovedById = account.ApprovedById;
            ApprovedOn = account.ApprovedOn;
            ApprovedBy = UserHistoryModel.CreateFrom(account.ApprovedBy);
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string AccountManagerName { get; set; }
        public string AccountManagerEmail { get; set; }
        public decimal Percentage { get; set; }
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public UserHistoryModel ApprovedBy { get; set; }
    }
}
