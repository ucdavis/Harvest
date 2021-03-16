using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Harvest.Web.Models;
using Harvest.Web.Models.FinancialAccountModels;
using Harvest.Web.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace Harvest.Web.Services
{
    public interface IFinancialService
    {
        Task<string> GetAccountName(string chart, string account, string subAccount);
        Task<KfsAccount> GetAccount(string chart, string account);
        Task<bool> IsAccountValid(string chart, string account, string subAccount);
        Task<bool> IsObjectValid(string chart, string objectCode);
        Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject);
        Task<bool> IsProjectValid(string project);
        Task<string> GetProjectName(string project);
        Task<string> GetSubAccountName(string chart, string account, string subAccount);
        Task<string> GetObjectName(string chart, string objectCode);
        Task<string> GetSubObjectName(string chart, string account, string objectCode, string subObject);

        Task<bool> IsOrgChildOfOrg(string childChart, string childOrg, string parentChart, string parentOrg);

        Task<AccountManager> GetFiscalOfficerForAccount(string chart, string account);

        Task<AccountValidationModel> IsValid(string account);
        Task<AccountValidationModel> IsValid(KfsAccount account);
    }

    public class FinancialService : IFinancialService
    {
        private readonly FinancialLookupSettings _financialSettings;

        public FinancialService(IOptions<FinancialLookupSettings> financialSettings)
        {
            _financialSettings = financialSettings.Value;
        }

        public async Task<string> GetAccountName(string chart, string account, string subAccount)
        {
            string url;
            string validationUrl;
            if (!String.IsNullOrWhiteSpace(subAccount))
            {
                validationUrl =
                    $"{_financialSettings.AccountUrl}/subaccount/{chart}/{account}/{subAccount}/isvalid";
                url =
                    $"{_financialSettings.AccountUrl}/subaccount/{chart}/{account}/{subAccount}/name";
            }
            else
            {
                validationUrl = $"{_financialSettings.AccountUrl}/account/{chart}/{account}/isvalid";
                url = $"{_financialSettings.AccountUrl}/account/{chart}/{account}/name";
            }

            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();
            if (!JsonConvert.DeserializeObject<bool>(validationContents))
            {
                Log.Information($"Account not valid {account}");
                throw new Exception("Invalid Account");
            }


            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();


            var contents = await response.Content.ReadAsStringAsync();
            return contents;
        }

        public async Task<KfsAccount> GetAccount(string chart, string account)
        {
            string url = $"{_financialSettings.AccountUrl}/account/{chart}/{account}";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<KfsAccount>(contents);
        }

        public async Task<bool> IsAccountValid(string chart, string account, string subAccount)
        {
            string validationUrl;
            if (!String.IsNullOrWhiteSpace(subAccount))
            {
                validationUrl = $"{_financialSettings.AccountUrl}/subaccount/{chart}/{account}/{subAccount}/isvalid";
            }
            else
            {
                validationUrl = $"{_financialSettings.AccountUrl}/account/{chart}/{account}/isvalid";
            }

            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool>(validationContents);
        }

        public async Task<bool> IsObjectValid(string chart, string objectCode)
        {
            string validationUrl = $"{_financialSettings.AccountUrl}/object/{chart}/{objectCode}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool>(validationContents); 
        }

        public async Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject)
        {
            string validationUrl = $"{_financialSettings.AccountUrl}/subobject/{chart}/{account}/{objectCode}/{subObject}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool>(validationContents); //TEST THIS!!!
        }

        public async Task<bool> IsProjectValid(string project)
        {
            string url = $"{_financialSettings.AccountUrl}/project/{project}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(url);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool>(validationContents); 
        }

        public async Task<string> GetProjectName(string project)
        {
            string url = $"{_financialSettings.AccountUrl}/project/{project}/name";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return contents.Trim('"');
        }

        public async Task<string> GetSubAccountName(string chart, string account, string subAccount)
        {
            string url = $"{_financialSettings.AccountUrl}/subaccount/{chart}/{account}/{subAccount}/name";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return contents.Trim('"');
        }


        public async Task<string> GetObjectName(string chart, string objectCode)
        {
            string url = $"{_financialSettings.AccountUrl}/object/{chart}/{objectCode}/name";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return contents.Trim('"');
        }

        public async Task<string> GetSubObjectName(string chart, string account, string objectCode, string subObject)
        {
            string url = $"{_financialSettings.AccountUrl}/subobject/{chart}/{account}/{objectCode}/{subObject}/name";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return contents.Trim('"');
        }

        public async Task<bool> IsOrgChildOfOrg(string childChart, string childOrg, string parentChart, string parentOrg)
        {
            //https://financials.api.adminit.ucdavis.edu/org/3/OSBC/ischildof/3/AAES
            string url = $"{_financialSettings.OrganizationUrl}/{childChart}/{childOrg}/ischildof/{parentChart}/{parentOrg}";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(url);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool>(validationContents);
        }

        public async Task<AccountManager> GetFiscalOfficerForAccount(string chart, string account)
        {
            //https://financials.api.adminit.ucdavis.edu/fau/account/{chart}/{account}/fiscalofficer
            string url = $"{_financialSettings.AccountUrl}/account/{chart}/{account}/fiscalofficer";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccountManager>(contents);
        }

        public async Task<AccountValidationModel> IsValid(string account)
        {
            var rtValue = new AccountValidationModel();
            try
            {
                account = account.Trim();
                rtValue.KfsAccount = new KfsAccount();
                var delimiter = new string[] { "-" };
                var accountArray = account.Split(delimiter, StringSplitOptions.None);
                if (accountArray.Length < 2)
                {
                    rtValue.IsValid = false;
                    rtValue.Message = "Need chart and account";
                    rtValue.Field = "Account";
                    return rtValue;
                }

                rtValue.KfsAccount.chartOfAccountsCode = accountArray[0].ToUpper();
                rtValue.KfsAccount.accountNumber = accountArray[1].ToUpper();
                if (accountArray.Length > 2)
                {
                    rtValue.KfsAccount.subAccount = accountArray[2].ToUpper();
                }
                //TODO: Maybe a project?

                rtValue = await IsValid(rtValue.KfsAccount);
            }
            catch
            {
                rtValue.IsValid = false;
                rtValue.Message = "Unable to parse account string";
                rtValue.Field = "Account";
            }

            return rtValue;
        }

        public async Task<AccountValidationModel> IsValid(KfsAccount account)
        {
            var rtValue = new AccountValidationModel();
            rtValue.KfsAccount = account;

            if (!await IsAccountValid(account.chartOfAccountsCode, account.accountNumber, account.subAccount))
            {
                rtValue.IsValid = false;
                rtValue.Field = "Account";
                rtValue.Message = "Valid Account Not Found. (Invalid or Expired).";

                return rtValue;
            }

            if (!string.IsNullOrWhiteSpace(account.subAccount))
            {
                //Maybe we don't care for validation?
                rtValue.KfsAccount.SubAccountName = await GetSubAccountName(account.chartOfAccountsCode, account.accountNumber, account.subAccount);
            }

            if (!string.IsNullOrWhiteSpace(account.project))
            {
                if (!await IsProjectValid(account.project))
                {
                    rtValue.IsValid = false;
                    rtValue.Field = "Project";
                    rtValue.Message = "Project Not Valid.";
                    return rtValue;
                }
                else
                {
                    rtValue.KfsAccount.ProjectName = await GetProjectName(account.project);
                }
            }


            var accountLookup = new KfsAccount();
            accountLookup = await GetAccount(account.chartOfAccountsCode, account.accountNumber);
            rtValue.KfsAccount.accountName = accountLookup.accountName;
            rtValue.KfsAccount.organizationCode = accountLookup.organizationCode;
            rtValue.KfsAccount.subFundGroupCode = accountLookup.subFundGroupCode;
            rtValue.KfsAccount.subFundGroupTypeCode = accountLookup.subFundGroupTypeCode;
            rtValue.KfsAccount.subFundGroupName = accountLookup.subFundGroupName;

            //TODO:this lookup can get the fiscal officer and account manager populate the account manager?

            //Ok, not check if the org rolls up to our orgs
            //Decide if we want to check this
            //if (await IsOrgChildOfOrg(accountLookup.chartOfAccountsCode,
            //        accountLookup.organizationCode, "3", "AAES") ||
            //    await IsOrgChildOfOrg(accountLookup.chartOfAccountsCode,
            //        accountLookup.organizationCode,
            //        "L", "AAES"))
            //{
            //    rtValue.IsValid = true;
            //}
            //else
            //{
            //    rtValue.IsValid = false;
            //    rtValue.Field = "Account";
            //    rtValue.Message = "Account not in CAES org.";
            //}


            return rtValue;
        }
    }
}
