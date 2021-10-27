using System.Collections.Generic;

namespace Harvest.Email.Models.Invoice
{
    /// <summary>
    /// Use for Invoice created and processed in sloth
    /// </summary>
    public class InvoiceErrorModel
    {
        public string InvoiceAmount { get; set; }
        public string InvoiceStatus { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceCreatedOn { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; } //Your project has one or more invalid accounts
        public string PiName { get; set; }

        public string NotificationText { get; set; } =
            "One or more of the accounts listed below were invalid when we tried to process the invoice. This could be because the account is expired or no longer active. Or it is possible that there was a problem validating the accounts against the financial system. If they are invalid, please update the accounts on your project.";

        public string ButtonUrlForInvoice { get; set; }
        public string ButtonUrlForProject { get; set; }
        public string ButtonInvoiceText { get; set; } = "View Invoice";
        public string ButtonProjectText { get; set; } = "View Project";

        public List<AccountsInProjectModel> AccountsList { get; set; }
    }

    public class AccountsInProjectModel
    {
        public string Account { get; set; }
        public string Message { get; set; }
    }
}
