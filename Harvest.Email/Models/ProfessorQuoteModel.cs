using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Email.Models
{
    public class ProfessorQuoteModel
    {
        /// <summary>
        /// Link to quote
        /// </summary>
        public string ButtonUrl = "https://harvest.caes.ucdavis.edu";
        public string ProfName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStart { get; set; }
        public string ProjectEnd { get; set; }
        public string QuoteAmount { get; set; }
    }
}
