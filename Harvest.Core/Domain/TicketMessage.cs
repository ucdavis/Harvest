using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Core.Domain
{
    public class TicketMessage
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketMessage>().HasIndex(a => a.TicketId);
            modelBuilder.Entity<TicketMessage>().HasIndex(a => a.CreatedOn);
        }
    }
}
