﻿using System;
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
        [StringLength(80)]
        public string Account { get; set; }

        [Range(0.01, Double.MaxValue, ErrorMessage = "Total must be greater than 0.01")] //Actually, sloth does have a max value...
        public decimal Total { get; set; }

        public bool IsProjectAccount { get; set; }

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
