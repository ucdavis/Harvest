namespace Harvest.Email.Models.Invoice
{
    /// <summary>
    /// Use for Invoice created and processed in sloth
    /// </summary>
    public class InvoiceEmailModel
    {
        public string InvoiceAmount { get; set; }
        public string InvoiceStatus { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceCreatedOn { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; } //Invoice has been created, Invoice has been processed, etc.
        public string PiName { get; set; }

        public string ButtonUrlForInvoice { get; set; }
        public string ButtonUrlForProject { get; set; }
        public string ButtonInvoiceText { get; set; } = "View Invoice";
        public string ButtonProjectText { get; set; } = "View Project";
    }
}
