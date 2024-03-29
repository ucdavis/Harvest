﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Harvest.Core.Domain
{
    public class Ticket
    {
        public Ticket()
        {
            Attachments = new List<TicketAttachment>();
            Messages = new List<TicketMessage>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [Required]
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int? UpdatedById { get; set; }
        public User UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        [StringLength(50)]
        [Display(Name = "Subject")]
        public string Name { get; set; }
        [Required]
        public string Requirements { get; set; }
        public DateTime? DueDate { get; set; }

        public string WorkNotes { get; set; } //For the person doing the Task
        [StringLength(25)]
        public string Status { get; set; } = Statuses.Created;
        public bool Completed { get; set; } = false;
        public List<TicketMessage> Messages { get; set; }
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
            modelBuilder.Entity<Ticket>().Property(a => a.Status).HasMaxLength(25);
            modelBuilder.Entity<Ticket>().Property(a => a.Name).HasMaxLength(50);

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

        public class Statuses
        {
            public const string Created = "Created";
            public const string Updated = "Updated";
            public const string Complete = "Complete";
        }
    }
}
