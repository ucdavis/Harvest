using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class TicketHistoryModel
    {
        public TicketHistoryModel() { }

        public TicketHistoryModel(Ticket ticket)
        {
            Id = ticket.Id;
            ProjectId = ticket.ProjectId;
            InvoiceId = ticket.InvoiceId;
            CreatedById = ticket.CreatedById;
            CreatedOn = ticket.CreatedOn;
            UpdatedById = ticket.UpdatedById;
            UpdatedOn = ticket.UpdatedOn;
            Name = ticket.Name;
            Requirements = ticket.Requirements;
            DueDate = ticket.DueDate;
            WorkNotes = ticket.WorkNotes;
            Status = ticket.Status;
            Completed = ticket.Completed;
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? InvoiceId { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedById { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Name { get; set; }
        public string Requirements { get; set; }
        public DateTime? DueDate { get; set; }
        public string WorkNotes { get; set; }
        public string Status { get; set; }
        public bool Completed { get; set; }

    }
}
