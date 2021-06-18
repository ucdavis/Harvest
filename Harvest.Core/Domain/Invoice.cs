using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public decimal Total { get; set; }

        public List<Expense> Expenses { get; set; }
        public List<Transfer> Transfers { get; set; }

        public DateTime CreatedOn { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }

        [Display(Name = "Sloth Transaction Id")]
        [StringLength(50)]
        public string SlothTransactionId { get; set; }
        [Display(Name = "Kfs Tracking Number")]
        [StringLength(20)] //Probably only 10, but this gives growth if it changes
        public string KfsTrackingNumber { get; set; }

        public List<Ticket> Tickets { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>().HasIndex(a => a.ProjectId);

            modelBuilder.Entity<Invoice>().Property(a => a.Total).HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .HasOne(a => a.Invoice)
                .WithMany(p => p.Expenses)
                .HasForeignKey(a => a.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(a => a.Invoice)
                .WithMany(p => p.Transfers)
                .HasForeignKey(a => a.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(a => a.Invoice)
                .WithMany(p => p.Tickets)
                .HasForeignKey(a => a.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public class Statuses
        {
            public const string Created = "Created";
            public const string Pending = "Pending";
            public const string Completed = "Completed";

            public static List<string> TypeList = new List<string>
            {
                Created,
                Pending,
                Completed,
            }.ToList();
        }
    }
}
