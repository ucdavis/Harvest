namespace Harvest.Email.Models
{
    public class ProjectClosedModel
    {
        public string PI { get; set; }
        public string NotificationText { get; set; } = "has approved the closeout of the project.";
        public string ProjectName { get; set; }
        public string ProjectEnd { get; set; }
        public string ProjectStart { get; set; }

        public string ButtonUrl1 { get; set; }
        public string ButtonText1 { get; set; } = "View Project";
    }
}
