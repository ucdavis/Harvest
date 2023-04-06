using System.Collections.Generic;
using Harvest.Core.Domain;

namespace Harvest.Web.Models.RateModels
{
    public class RateEditModel
    {
        public Rate Rate { get; set; }
        public List<string> TypeList { get; set; }

        public bool UseCoA { get; set; }

        public string TeamName { get; set; }
    }
}
