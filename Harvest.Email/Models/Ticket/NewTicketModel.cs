namespace Harvest.Email.Models.Ticket
{
    public class NewTicketModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string CreatedOn { get; set; }
        public string Subject { get; set; }
        public string DueDate { get; set; }

        public string Requirements { get; set; }
        public string ButtonUrlForTicket { get; set; }
        public string ButtonUrlForProject { get; set; }
        public string ButtonTicketText { get; set; } = "View Ticket";
        public string ButtonProjectText { get; set; } = "View Project";
    }
}
