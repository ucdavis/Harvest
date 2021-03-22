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

        public decimal Amount { get; set; }

        [StringLength(40)]
        public string Description { get; set; }

        public int FromAccountId { get; set; }

        public Account FromAccount { get; set; }

        public int ToAccountId { get; set; }

        public Account ToAccount { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Transfer>().HasIndex(a => a.FromAccountId);
            modelBuilder.Entity<Transfer>().HasIndex(a => a.ToAccountId);

            modelBuilder.Entity<Transfer>().Property(a => a.Amount).HasPrecision(18, 2);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
