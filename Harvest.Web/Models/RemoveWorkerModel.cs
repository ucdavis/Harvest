namespace Harvest.Web.Models
{
    public class RemoveWorkerModel
    {
        public int SupervisorId { get; set; }
        public int WorkerId { get; set; }

        public string WorkerName { get; set; }
        public string SupervisorName { get; set; }
    }
}
