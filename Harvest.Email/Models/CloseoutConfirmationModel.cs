namespace Harvest.Email.Models
{
    public class CloseoutConfirmationModel
    {
        public string PI { get; set; }
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }

        public string NotificationText { get; set; } =
            "Please confirm the closeout of your project or go to the project details and create a help ticket.";

        public string WarningText { get; set; } =
            "If you don't confirm the closeout of your project, it may incur additional charges.";
        public string ButtonUrl1 { get; set; }
        public string ButtonText1 { get; set; } = "View Project";
        public string ButtonUrl2 { get; set; }
        public string ButtonText2 { get; set; } = "Approve Closeout";
    }
}
