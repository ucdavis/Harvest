using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
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
        Task<AccountValidationInfo> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService{

        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }


        public async Task<AccountValidationInfo> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationInfo();

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                rtValue.FinancialStringType = FinancialChartStringType.Gl;

                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                rtValue.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;

                if (rtValue.IsValid)
                {
                    //TODO: Do we have other validation?
                    rtValue.GlSegments = FinancialChartValidation.GetGlSegments(financialSegmentString);
                }

                return rtValue;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                rtValue.FinancialStringType = FinancialChartStringType.Ppm;

                var result = await _aggieClient.PpmStringSegmentsValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                rtValue.IsValid = data.PpmStringSegmentsValidate.ValidationResponse.Valid;

                rtValue.PpmSegments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                //TODO: Extra validation for PPM strings?

                return rtValue;
            }

            //TODO: Review this logic I had to change it to get it to compile so I was working on this before I switched to OPP and may have forgot what I was doing...
            rtValue.FinancialStringType = FinancialChartStringType.Invalid;
            rtValue.IsValid = false;
            return rtValue;
        }


        public class AccountValidationInfo
        {
            public bool IsValid { get; set; }
            public FinancialChartStringType FinancialStringType { get; set; }

            public GlSegments GlSegments { get; set; }
            public PpmSegments PpmSegments { get; set; }

        }
    }
}
