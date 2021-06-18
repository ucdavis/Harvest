using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

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

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.TicketId);
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.CreatedOn);
        }
    }
}
