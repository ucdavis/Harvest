namespace Harvest.Email.Models.Ticket
{
    public class TicketReplyModel
    {
        public string From { get; set; }
        public string ProjectName { get; set; }
        public string CreatedOn { get; set; }
        public string Subject { get; set; }


        public string Reply { get; set; }
        public string ButtonUrlForTicket { get; set; }
        public string ButtonUrlForProject { get; set; }
        public string ButtonTicketText { get; set; } = "View Ticket";
        public string ButtonProjectText { get; set; } = "View Project";
    }
}
