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
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.CurrentDocument)
                .WithOne(d => d.Quote)
                .HasForeignKey<Quote>(q => q.CurrentDocumentId);
        }
    }
}
