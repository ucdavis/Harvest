using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using System.Collections.Generic;

namespace Harvest.Core.Models.FinancialAccountModels
{
    public class AccountValidationModel
    {
        public bool IsValid { get; set; } = true;
        public string Field { get; set; }
        //public string Message { get; set; } //Changed to use the list instead.

        public KfsAccount KfsAccount { get; set; }

        //AE Stuff:
        public string FinancialSegmentString { get; set; } //Not sure if I want this here, but I have the KFS Account above, so maybe.
        //public bool IsValid { get; set; } = false; So above this defaults to true, need to set to false for AE stuff.
        public FinancialChartStringType CoaChartType { get; set; }
        public GlSegments GlSegments { get; set; }
        public PpmSegments PpmSegments { get; set; }
        /// <summary>
        /// Return Segment info.
        /// </summary>
        public List<KeyValuePair<string, string>> Details { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> Warnings { get; set; } = new List<KeyValuePair<string, string>>();
        public string Message
        {
            get
            {
                if (Messages.Count <= 0)
                {
                    return string.Empty;
                }

                return string.Join(" ", Messages);
            }
        }
        public List<string> Messages { get; set; } = new List<string>();
    }
}
