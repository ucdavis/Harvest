
using System;

namespace Harvest.Core.Models.SlothModels
{
    public class SlothResponseModel
    {
        public string Id { get; set; }
        public string KfsTrackingNumber { get; set; }
        public string Status { get; set; }
    }

    public static class SlothStatuses
    {
        public const string PendingApproval = "PendingApproval";
        public const string Scheduled = "Scheduled";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
