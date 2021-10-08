using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Harvest.Core.Services
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

        Task<KfsUser> GetFiscalOfficerForAccount(string chart, string account);

        Task<AccountValidationModel> IsValid(string account);
        Task<AccountValidationModel> IsValid(KfsAccount account);
        KfsAccount Parse(string account);
    }

    public class FinancialService : IFinancialService
    {
        private readonly FinancialLookupSettings _financialSettings;
        private readonly JsonSerializerOptions _serializerOptions;

        public FinancialService(IOptions<FinancialLookupSettings> financialSettings, JsonSerializerOptions serializerOptions)
        {
            _financialSettings = financialSettings.Value;
            _serializerOptions = serializerOptions;
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
            if (!JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions))
            {
                Log.Information("Account not valid {account}", account);
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

            return JsonSerializer.Deserialize<KfsAccount>(contents, _serializerOptions);
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

            return JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions);
        }

        public async Task<bool> IsObjectValid(string chart, string objectCode)
        {
            string validationUrl = $"{_financialSettings.AccountUrl}/object/{chart}/{objectCode}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions); 
        }

        public async Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject)
        {
            string validationUrl = $"{_financialSettings.AccountUrl}/subobject/{chart}/{account}/{objectCode}/{subObject}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(validationUrl);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions); //TEST THIS!!!
        }

        public async Task<bool> IsProjectValid(string project)
        {
            string url = $"{_financialSettings.AccountUrl}/project/{project}/isvalid";


            using var client = new HttpClient();
            var validationResponse = await client.GetAsync(url);
            validationResponse.EnsureSuccessStatusCode();

            var validationContents = await validationResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions); 
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

            return JsonSerializer.Deserialize<bool>(validationContents, _serializerOptions);
        }

        public async Task<KfsUser> GetFiscalOfficerForAccount(string chart, string account)
        {
            //https://financials.api.adminit.ucdavis.edu/fau/account/{chart}/{account}/fiscalofficer
            string url = $"{_financialSettings.AccountUrl}/account/{chart}/{account}/fiscalofficer";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KfsUser>(contents, _serializerOptions);
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

                rtValue.KfsAccount.ChartOfAccountsCode = accountArray[0].ToUpper();
                rtValue.KfsAccount.AccountNumber = accountArray[1].ToUpper();
                if (accountArray.Length > 2)
                {
                    rtValue.KfsAccount.SubAccount = accountArray[2].ToUpper();
                }
                //TODO: Maybe a project? or leave a placeholder for project?
                if (accountArray.Length > 3)
                {
                    rtValue.KfsAccount.ObjectCode = accountArray[3].Trim().ToUpper();
                }
                

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
            var rtValue = new AccountValidationModel {KfsAccount = account};

            if (!await IsAccountValid(account.ChartOfAccountsCode, account.AccountNumber, account.SubAccount))
            {
                rtValue.IsValid = false;
                rtValue.Field = "Account";
                rtValue.Message = "Valid Account Not Found. (Invalid or Expired).";

                return rtValue;
            }

            if (!string.IsNullOrWhiteSpace(account.SubAccount))
            {
                //Maybe we don't care for validation?
                rtValue.KfsAccount.SubAccountName = await GetSubAccountName(account.ChartOfAccountsCode, account.AccountNumber, account.SubAccount);
            }

            if (!string.IsNullOrWhiteSpace(account.Project))
            {
                if (!await IsProjectValid(account.Project))
                {
                    rtValue.IsValid = false;
                    rtValue.Field = "Project";
                    rtValue.Message = "Project Not Valid.";
                    return rtValue;
                }
                else
                {
                    rtValue.KfsAccount.ProjectName = await GetProjectName(account.Project);
                }
            }

            if (!string.IsNullOrWhiteSpace(account.ObjectCode))
            {
                if (account.ObjectCode.Trim().Length > 4)
                {
                    rtValue.IsValid = false;
                    rtValue.Field = "ObjectCode";
                    rtValue.Message = "Object Code is too long";
                    return rtValue;
                }
                if (!await IsObjectValid(account.ChartOfAccountsCode, account.ObjectCode))
                {
                    rtValue.IsValid = false;
                    rtValue.Field = "ObjectCode";
                    rtValue.Message = "Object Code is not valid";
                    return rtValue;
                }
            }

            var accountLookup = await GetAccount(account.ChartOfAccountsCode, account.AccountNumber);
            rtValue.KfsAccount.AccountName = accountLookup.AccountName;
            rtValue.KfsAccount.OrganizationCode = accountLookup.OrganizationCode;
            rtValue.KfsAccount.SubFundGroupCode = accountLookup.SubFundGroupCode;
            rtValue.KfsAccount.SubFundGroupTypeCode = accountLookup.SubFundGroupTypeCode;
            rtValue.KfsAccount.SubFundGroupName = accountLookup.SubFundGroupName;
            rtValue.KfsAccount.AccountManager = accountLookup.AccountManager;
            rtValue.KfsAccount.FiscalOfficer = accountLookup.FiscalOfficer;

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

        /// <summary>
        /// Note This doesn't validate
        /// This is just to get the KFS account parts without calling the KFS api each time.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public KfsAccount Parse(string account)
        {
            var rtValue = new KfsAccount();

            account = account.Trim();
            var delimiter = new string[] { "-" };
            var accountArray = account.Split(delimiter, StringSplitOptions.None);
            if (accountArray.Length < 2)
            {
                throw new Exception("Invalid Account format");
            }

            rtValue.ChartOfAccountsCode = accountArray[0].ToUpper();
            rtValue.AccountNumber = accountArray[1].ToUpper();
            if (accountArray.Length > 2)
            {
                rtValue.SubAccount = accountArray[2].ToUpper();
            }
            //TODO: Maybe a project? or leave a placeholder for project?
            if (accountArray.Length > 3)
            {
                rtValue.ObjectCode = accountArray[3].Trim().ToUpper();
            }
            
            return rtValue;
        }
    }
}
