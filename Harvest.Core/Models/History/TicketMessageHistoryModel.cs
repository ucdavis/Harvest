using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class TicketMessageHistoryModel
    {
        public TicketMessageHistoryModel() { }

        public TicketMessageHistoryModel(TicketMessage message)
        {
            Id = message.Id;
            Message = message.Message;
            CreatedById = message.CreatedById;
            CreatedOn = message.CreatedOn;
            TicketId = message.TicketId;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TicketId { get; set; }
    }
}
