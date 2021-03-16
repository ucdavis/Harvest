using System;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Web.Models.FinancialAccountModels
{
    public class KfsAccount
    {
        [StringLength(1)]
        public string chartOfAccountsCode { get; set; }
        public string organizationCode { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? accountExpirationDate { get; set; }
        public bool? closed { get; set; }

        public string subFundGroupTypeCode { get; set; }

        public string subFundGroupName { get; set; }
        public string subFundGroupCode { get; set; }

        public string ProjectName { get; set; } //Different Lookup
        public string SubAccountName { get; set; } //Different Lookup
        public string subAccount { get; set; } //Added for harvest
        public string project { get; set; } //Added for harvest


        public static implicit operator KfsAccount(string v)
        {
            throw new NotImplementedException();
        }
    }
}
