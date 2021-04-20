
using System;

namespace Harvest.Core.Models.SlothModels
{
    public class SlothResponseModel
    {
        public string Id { get; set; }
        public string KfsTrackingNumber { get; set; }
        public string Status { get; set; }

        public bool Success { get; set; } = true;
        public string Message { get; set; } //Error Message
    }
}
