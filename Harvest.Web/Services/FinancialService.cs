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
    }
}
