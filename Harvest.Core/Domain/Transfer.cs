using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Account { get; set; }

        [Range(0.01, 1000000.00)]
        public decimal Total { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transfer>().HasIndex(a => a.InvoiceId);

            modelBuilder.Entity<Transfer>().Property(a => a.Total).HasPrecision(18, 2);
        }

        public class Types
        {
            public const string Debit = "Debit";
            public const string Credit = "Credit";
        }

    }
}
