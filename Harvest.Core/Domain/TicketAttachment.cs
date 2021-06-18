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
        [StringLength(250)]
        public string FileName { get; set; }
        [StringLength(250)]
        public string ContentType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(128)]
        public string Identifier { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.TicketId);
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.FileName).HasMaxLength(250);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.ContentType).HasMaxLength(250);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.Identifier).HasMaxLength(128);
        }
    }
}
