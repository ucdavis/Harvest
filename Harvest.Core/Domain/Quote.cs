using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Domain
{
    public class Quote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [MaxLength]
        public string Text { get; set; }

        public decimal Total { get; set; }

        public int InitatedById { get; set; }

        public int? CurrentDocumentId { get; set; }

        public int? ApprovedById { get; set; }

        [Display(Name = "Aproved On")]
        public DateTime? ApprovedOn { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public Project Project { get; set; }

        [Display(Name = "Initiated By")]
        public User InitiatedBy { get; set; }

        [Display(Name = "Approved By")]
        public User ApprovedBy { get; set; }

        public List<Document> Documents { get; set; }

        public Document CurrentDocument { get; set; }

        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quote>().HasIndex(a => a.ProjectId);
            modelBuilder.Entity<Quote>().HasIndex(a => a.ApprovedById);
            modelBuilder.Entity<Quote>().HasIndex(a => a.CurrentDocumentId);
            modelBuilder.Entity<Quote>().HasIndex(a => a.InitatedById);

            modelBuilder.Entity<Quote>().Property(a => a.Total).HasPrecision(18, 2);
        }
    }
}
