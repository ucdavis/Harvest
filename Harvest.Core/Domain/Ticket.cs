using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Harvest.Core.Domain
{
    public class Ticket
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

        [StringLength(50)]
        [Display(Name = "Task Name")]
        public string Name { get; set; }
        [Required]
        public string Requirements { get; set; }
        public DateTime DueDate { get; set; }

        public string WorkNotes { get; set; } //For the person doing the Task
        public string Status { get; set; } = "Created";
        public bool Completed { get; set; } = false;
        [JsonIgnore]
        public List<TicketMessage> Messages { get; set; }
        [JsonIgnore]
        public List<TicketAttachment> Attachments { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>().HasIndex(a => a.Name);
            modelBuilder.Entity<Ticket>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<Ticket>().HasIndex(a => a.InvoiceId);
            modelBuilder.Entity<Ticket>().HasIndex(a => a.CreatedOn);
            modelBuilder.Entity<Ticket>().HasIndex(a => a.DueDate);
            modelBuilder.Entity<Ticket>().HasIndex(a => a.Completed);

            modelBuilder.Entity<Ticket>().Property(a => a.Completed).HasDefaultValue(false);

            modelBuilder.Entity<TicketMessage>()
                .HasOne(a => a.Ticket)
                .WithMany(a => a.Messages)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAttachment>()
                .HasOne(a => a.Ticket)
                .WithMany(a => a.Attachments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
