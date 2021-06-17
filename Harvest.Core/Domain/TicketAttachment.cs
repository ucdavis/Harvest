using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Domain
{
    public class TicketAttachment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}
