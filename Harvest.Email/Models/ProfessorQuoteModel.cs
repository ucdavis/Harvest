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
        public string ButtonUrl { get; set; }
        public string ProfName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStart { get; set; }
        public string ProjectEnd { get; set; }
        public string QuoteAmount { get; set; }

        /// <summary>
        /// Sets model values when running TestController.TestBody
        /// </summary>
        public void InitForMjml()
        {
            ProfName     = "@Model.ProfName";
            ProjectName  = "@Model.ProjectName";
            ProjectStart = "@Model.ProjectStart";
            ProjectEnd   = "@Model.ProjectEnd";
            QuoteAmount  = "@Model.QuoteAmount";
            ButtonUrl    = "@Model.ButtonUrl";
        }
    }
}
