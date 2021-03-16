using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Services
{
    public interface IFinancialService
    {
        Task<string> GetAccountName(string chart, string account, string subAccount);
        //Task<KfsAccount> GetAccount(string chart, string account);
        Task<bool> IsAccountValid(string chart, string account, string subAccount);
        Task<bool> IsObjectValid(string chart, string objectCode);
        Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject);
        Task<bool> IsProjectValid(string project);
        Task<string> GetProjectName(string project);
        Task<string> GetSubAccountName(string chart, string account, string subAccount);
        Task<string> GetObjectName(string chart, string objectCode);
        Task<string> GetSubObjectName(string chart, string account, string objectCode, string subObject);

        Task<bool> IsOrgChildOfOrg(string childChart, string childOrg, string parentChart, string parentOrg);

        //Task<AccountValidationModel> IsAccountValidForRegistration(FinancialAccount account);
        //Task<AccountValidationModel> IsAccountValidForRegistration(string account);
    }
}
