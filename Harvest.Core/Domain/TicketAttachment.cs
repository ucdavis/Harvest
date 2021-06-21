using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class TicketAttachment : AttachmentBase
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.TicketId);
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<TicketAttachment>().HasIndex(a => a.CreatedById);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.FileName).HasMaxLength(250);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.ContentType).HasMaxLength(250);
            modelBuilder.Entity<TicketAttachment>().Property(a => a.Identifier).HasMaxLength(128);
        }
    }
}
