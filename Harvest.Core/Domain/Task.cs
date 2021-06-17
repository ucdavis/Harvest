using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Domain
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [Required]
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }

        public int UpdatedById { get; set; }
        public DateTime UpdatedOn { get; set; }

        [Required]
        public string Requirements { get; set; }
        public DateTime DueDate { get; set; }

        public string WorkNotes { get; set; } //For the person doing the Task
        public string Status { get; set; } = "Created";
        public bool Completed { get; set; } = false;
        
    }
}
