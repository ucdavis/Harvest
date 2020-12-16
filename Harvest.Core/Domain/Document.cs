using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        [Required]
        public string Identifier { get; set; }

        public Quote Quote { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Quote)
                .WithOne(q => q.CurrentDocument)
                .HasForeignKey<Quote>(q => q.CurrentDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Quote>()
                .HasMany(q => q.Documents)
                .WithOne()
                .HasForeignKey(d => d.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
