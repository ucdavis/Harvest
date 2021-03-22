using System;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Core.Domain
{
    public class TransferHistory
    {
        public TransferHistory()
        {
            ActionDateTime = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int TransferId { get; set; }

        [Required]
        public DateTime ActionDateTime { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(50)]
        public string ActorId { get; set; }

        [MaxLength(250)]
        public string ActorName { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public string Notes { get; set; }
    }   
}
