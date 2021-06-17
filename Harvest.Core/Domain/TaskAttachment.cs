using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Domain
{
    public class TaskAttachment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int TaskId { get; set; }
        public ProjectTask Task { get; set; }
    }
}
