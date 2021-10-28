using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Harvest.Email.Models.Invoice
{
    public class InvoiceExceedsQuoteModel
    {
        public string InvoiceAmount { get; set; }
        public string RemainingAmount { get; set; }
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }

        public string ButtonUrl { get; set; }
        public string ButtonProjectText { get; set; } = "View Project";

    }
}
