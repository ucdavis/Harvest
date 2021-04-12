using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public DateTime CreatedOn { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>().HasIndex(a => a.ProjectId);

            modelBuilder.Entity<Invoice>().Property(a => a.Total).HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .HasOne(a => a.Invoice)
                .WithMany(p => p.Expenses)
                .HasForeignKey(a => a.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
