using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Harvest.Core.Services.AggieEnterpriseService;

namespace Harvest.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true, bool validateRate = false);

        Task<FinancialOfficerDetails> GetFinancialOfficer(string financialSegmentString);

        string GetNaturalAccount(string financialSegmentString);
        string ReplaceNaturalAccount(string financialSegmentString, string naturalAccount);

        Task<string> ConvertKfsAccount(string account);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService{

        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseOptions Options { get; set;}

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            try
            {
                _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating Aggie Enterprise Client");
                _aggieClient = null;
            }
            Options = options.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="financialSegmentString"></param>
        /// <param name="validateCVRs"></param>
        /// <param name="validateRate">Performs extra validation needed for Rates</param>
        /// <returns></returns>
        public async Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true, bool validateRate = false)
        {
            var rtValue = new AccountValidationModel();
            rtValue.IsValid = false;
            rtValue.Field = "Aggie Enterprise COA";
            rtValue.FinancialSegmentString = financialSegmentString;

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);
            rtValue.CoaChartType = segmentStringType;

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                rtValue.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;

                if (!rtValue.IsValid)
                {
                    foreach (var err in data.GlValidateChartstring.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }
                rtValue.Details.Add(new KeyValuePair<string, string>("Entity", $"{data.GlValidateChartstring.SegmentNames.EntityName} ({data.GlValidateChartstring.Segments.Entity})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Fund", $"{data.GlValidateChartstring.SegmentNames.FundName} ({data.GlValidateChartstring.Segments.Fund})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Department", $"{data.GlValidateChartstring.SegmentNames.DepartmentName} ({data.GlValidateChartstring.Segments.Department})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Account", $"{data.GlValidateChartstring.SegmentNames.AccountName} ({data.GlValidateChartstring.Segments.Account})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Purpose", $"{data.GlValidateChartstring.SegmentNames.PurposeName} ({data.GlValidateChartstring.Segments.Purpose})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Project", $"{data.GlValidateChartstring.SegmentNames.ProjectName} ({data.GlValidateChartstring.Segments.Project})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Program", $"{data.GlValidateChartstring.SegmentNames.ProgramName} ({data.GlValidateChartstring.Segments.Program})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Activity", $"{data.GlValidateChartstring.SegmentNames.ActivityName} ({data.GlValidateChartstring.Segments.Activity})"));

                if (data.GlValidateChartstring.Warnings != null)
                {
                    foreach (var warn in data.GlValidateChartstring.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.GlSegments = FinancialChartValidation.GetGlSegments(financialSegmentString);

                //TODO: Rate Validations

                return rtValue;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var result = await _aggieClient.PpmStringSegmentsValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                rtValue.IsValid = data.PpmStringSegmentsValidate.ValidationResponse.Valid;
                if (!rtValue.IsValid)
                {
                    foreach (var err in data.PpmStringSegmentsValidate.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }

                rtValue.Details.Add(new KeyValuePair<string, string>("Project", data.PpmStringSegmentsValidate.Segments.Project));
                rtValue.Details.Add(new KeyValuePair<string, string>("Task", data.PpmStringSegmentsValidate.Segments.Task));
                rtValue.Details.Add(new KeyValuePair<string, string>("Organization", data.PpmStringSegmentsValidate.Segments.Organization));
                rtValue.Details.Add(new KeyValuePair<string, string>("Expenditure Type", data.PpmStringSegmentsValidate.Segments.ExpenditureType));
                rtValue.Details.Add(new KeyValuePair<string, string>("Award", data.PpmStringSegmentsValidate.Segments.Award));
                rtValue.Details.Add(new KeyValuePair<string, string>("Funding Source", data.PpmStringSegmentsValidate.Segments.FundingSource));

                if (data.PpmStringSegmentsValidate.Warnings != null)
                {
                    foreach (var warn in data.PpmStringSegmentsValidate.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.PpmSegments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                //TODO: Rate Validations
                if (validateRate)
                {
                    rtValue.IsValid = false;
                    rtValue.Messages.Add("Harvest Rates can't have PPM COA's");
                }

                await GetPpmAccountManager(rtValue);

                return rtValue;
            }
            
            rtValue.IsValid = false; //Just in case.
            rtValue.Messages.Add("Invalid Aggie Enterprise COA format");
            
            return rtValue;
        }

        private async Task GetPpmAccountManager(AccountValidationModel rtValue)
        {
            var result = await _aggieClient.PpmProjectManager.ExecuteAsync(rtValue.PpmSegments.Project);

            var data = result.ReadData();

            if (data.PpmProjectByNumber?.ProjectNumber == rtValue.PpmSegments.Project)
            {
                rtValue.AccountManager = data.PpmProjectByNumber.PrimaryProjectManagerName;
                rtValue.AccountManagerEmail = data.PpmProjectByNumber.PrimaryProjectManagerEmail;
                rtValue.ProjectName = data.PpmProjectByNumber.Name;
            }
            return;  
        }

        /// <summary>
        /// This is just a stub to get the financial officer details.  It is not currently implemented.
        /// </summary>
        /// <param name="financialSegmentString"></param>
        /// <returns></returns>
        public async Task<FinancialOfficerDetails> GetFinancialOfficer(string financialSegmentString)
        {
            //TODO: If AE can supply the query....
            var rtValue = new FinancialOfficerDetails();
            rtValue.FinancialOfficerId = null;

            return rtValue;
        }

        public string GetNaturalAccount(string financialSegmentString)
        {
            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);
            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var segments = FinancialChartValidation.GetGlSegments(financialSegmentString);
                return segments.Account;
            }
            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var segments = FinancialChartValidation.GetPpmSegments(financialSegmentString);
                return segments.ExpenditureType;
            }
            return null;
        }

        public string ReplaceNaturalAccount(string financialSegmentString, string naturalAccount)
        {
            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);
            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var segments = FinancialChartValidation.GetGlSegments(financialSegmentString);
                segments.Account = naturalAccount;
                return segments.ToSegmentString();
            }
            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var segments = FinancialChartValidation.GetPpmSegments(financialSegmentString);
                segments.ExpenditureType = naturalAccount;
                return segments.ToSegmentString();
            }
            Log.Error($"Invalid financial segment string: {financialSegmentString}");
            return financialSegmentString;
        }

        public async Task<string> ConvertKfsAccount(string account)
        {
            var parts = account.Split('-');

            if (parts.Length < 2)
            {
                return null;
            }

            var chart = parts[0].Trim();
            var accountPart = parts[1].Trim();
            var subAcct = parts.Length > 2 ? parts[2].Trim() : null;

            var result = await _aggieClient.KfsConvertAccount.ExecuteAsync(chart, accountPart, subAcct);
            var data = result.ReadData();
            if (data.KfsConvertAccount.GlSegments != null)
            {
                var tempGlSegments = new GlSegments(data.KfsConvertAccount.GlSegments);
                tempGlSegments.Account = Options.NormalCoaNaturalAccount;
                return tempGlSegments.ToSegmentString();
            }

            if (data.KfsConvertAccount.PpmSegments != null)
            {
                var tempPpmSegments = new PpmSegments(data.KfsConvertAccount.PpmSegments);
                tempPpmSegments.ExpenditureType = Options.NormalCoaNaturalAccount;
                return tempPpmSegments.ToSegmentString();
            }
            else
            {
                return null;
            }
        }


        public class FinancialOfficerDetails
        {
            public string FinancialOfficerId { get; set; }
        }
    }
}
