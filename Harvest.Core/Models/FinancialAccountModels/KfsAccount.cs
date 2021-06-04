using System;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Core.Models.FinancialAccountModels
{
    public class KfsAccount
    {
        [StringLength(1)]
        public string ChartOfAccountsCode { get; set; }
        public string OrganizationCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public DateTime? AccountExpirationDate { get; set; }
        public bool? Closed { get; set; }

        public string SubFundGroupTypeCode { get; set; }

        public string SubFundGroupName { get; set; }
        public string SubFundGroupCode { get; set; }

        public string ProjectName { get; set; } //Different Lookup
        public string SubAccountName { get; set; } //Different Lookup
        public string SubAccount { get; set; } //Added for harvest
        public string Project { get; set; } //Added for harvest

        public override string ToString()
        {
            var extraAccountInfo = string.Empty;
            if (!string.IsNullOrWhiteSpace(SubAccount) || !string.IsNullOrWhiteSpace(Project))
            {
                if (!string.IsNullOrWhiteSpace(Project))
                {
                    extraAccountInfo = $"-{SubAccount}-{Project}";
                }
                else
                {
                    extraAccountInfo = $"-{SubAccount}";
                }
            }
            return $"{ChartOfAccountsCode}-{AccountNumber}{extraAccountInfo}";
        }

        public static implicit operator KfsAccount(string v)
        {
            throw new NotImplementedException();
        }
    }
}
