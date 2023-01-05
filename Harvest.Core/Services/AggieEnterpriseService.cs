using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Harvest.Core.Services.AggieEnterpriseService;

namespace Harvest.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true);

        Task<FinancialOfficerDetails> GetFinancialOfficer(string financialSegmentString);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService{

        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }


        public async Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationModel();
            rtValue.IsValid = false;
            rtValue.Field = "Aggie Enterprise COA";
            rtValue.FinancialSegmentString = financialSegmentString;

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                rtValue.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;
                rtValue.IsPpm = false;

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

                return rtValue;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var result = await _aggieClient.PpmStringSegmentsValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                rtValue.IsValid = data.PpmStringSegmentsValidate.ValidationResponse.Valid;
                rtValue.IsPpm = true;
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

                return rtValue;
            }

            rtValue.IsValid = false; //Just in case.
            rtValue.Messages.Add("This is not a valid Aggie Enterprise COA format");
            
            return rtValue;
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


        //public class AccountValidationInfo
        //{
        //    public bool IsValid { get; set; }
        //    public FinancialChartStringType FinancialStringType { get; set; }

        //    public GlSegments GlSegments { get; set; }
        //    public PpmSegments PpmSegments { get; set; }

        //}

        public class FinancialOfficerDetails
        {
            public string FinancialOfficerId { get; set; }
        }
    }
}
