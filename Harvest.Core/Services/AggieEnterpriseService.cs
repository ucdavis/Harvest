using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Validation;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }

    public class AggieEnterpriseService : IAggieEnterpriseService{

        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }


        public async Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                var isValid = data.GlValidateChartstring.ValidationResponse.Valid;

                if (isValid)
                {
                    //TODO: Do we have other validation?
                }

                return isValid;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var result = await _aggieClient.PpmStringSegmentsValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                var isValid = data.PpmStringSegmentsValidate.ValidationResponse.Valid;

                //TODO: Extra validation for PPM strings?

                return isValid;
            }



            return false;
        }
    }
}
