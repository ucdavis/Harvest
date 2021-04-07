using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Models.FinancialAccountModels;

namespace Harvest.Web.Models.RateModels
{
    public class RateDetailsModel
    {
        public Rate Rate { get; set; }
        public AccountValidationModel AccountValidation { get; set; }
    }
}
